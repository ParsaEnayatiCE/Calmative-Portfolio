using AutoMapper;
using Calmative.Server.API.Data;
using Calmative.Server.API.DTOs;
using Calmative.Server.API.Models;
using Calmative.Server.API.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Calmative.Server.Tests;

public class PortfolioServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly PortfolioService _service;
    private const int UserId = 1;

    public PortfolioServiceTests()
    {
        _context = TestHelper.GetInMemoryDbContext(Guid.NewGuid().ToString());
        _mapper = TestHelper.GetMapper();
        _context.Users.Add(new User { Id = UserId, Email = "u@e.com", FirstName = "F", LastName = "L", PasswordHash = "h", IsEmailConfirmed = true });
        _context.SaveChanges();
        _service = new PortfolioService(_context, _mapper);
    }

    [Fact]
    public async Task CreateAndGetPortfolioSummary_ShouldReturnCorrectValues()
    {
        // Arrange
        var createDto = new CreatePortfolioDto { Name = "MyPortfolio", Description = "Test" };

        // Act – Create
        var portfolio = await _service.CreatePortfolioAsync(createDto, UserId);
        portfolio.Should().NotBeNull();

        // Seed one asset
        _context.Assets.Add(new Asset
        {
            Symbol = "BTC",
            Type = AssetType.Crypto,
            Name = "Bitcoin",
            Quantity = 1,
            PurchasePrice = 40000,
            CurrentPrice = 45000,
            PurchaseDate = DateTime.UtcNow,
            PortfolioId = portfolio!.Id
        });
        _context.PriceHistories.Add(new PriceHistory { Symbol = "BTC", AssetType = AssetType.Crypto, Price = 45000, Timestamp = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        // Act – Summary
        var summary = await _service.GetPortfolioSummaryAsync(portfolio.Id, UserId);

        // Assert
        summary.TotalValue.Should().Be(45000);
        summary.TotalProfitLoss.Should().Be(5000);
        summary.AssetTypeDistribution.Should().HaveCount(1);
        summary.TopPerformingAssets.Should().ContainSingle(a => a.Symbol == "BTC");
    }

    [Fact]
    public async Task DeletePortfolio_ShouldRemove()
    {
        var p = await _service.CreatePortfolioAsync(new CreatePortfolioDto{Name="Del",Description=""},UserId);
        var ok = await _service.DeletePortfolioAsync(p!.Id,UserId);
        ok.Should().BeTrue();
        (await _context.Portfolios.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task GetUserPortfolios_ShouldReturnList()
    {
        await _service.CreatePortfolioAsync(new CreatePortfolioDto{Name="P1"},UserId);
        await _service.CreatePortfolioAsync(new CreatePortfolioDto{Name="P2"},UserId);
        var list = await _service.GetUserPortfoliosAsync(UserId);
        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPortfolioById_InvalidUser_ShouldReturnNull()
    {
        var p = await _service.CreatePortfolioAsync(new CreatePortfolioDto{Name="X"},UserId);
        var res = await _service.GetPortfolioByIdAsync(p!.Id, 999);
        res.Should().BeNull();
    }

    [Fact]
    public async Task Summary_ZeroInvestment_ShouldHaveZeroProfitPercent()
    {
        var p = await _service.CreatePortfolioAsync(new CreatePortfolioDto{Name="Zero"},UserId);
        _context.Assets.Add(new Asset{Symbol="USD",Type=AssetType.Currency,Name="Cash",Quantity=0,PurchasePrice=0,CurrentPrice=0,PurchaseDate=DateTime.UtcNow,PortfolioId=p!.Id});
        await _context.SaveChangesAsync();
        var summary = await _service.GetPortfolioSummaryAsync(p.Id,UserId);
        summary.TotalInvestment.Should().Be(0);
        summary.TotalProfitLossPercentage.Should().Be(0);
    }
} 