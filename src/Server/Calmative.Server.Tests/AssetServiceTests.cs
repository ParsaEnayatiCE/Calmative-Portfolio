using AutoMapper;
using Calmative.Server.API.Data;
using Calmative.Server.API.DTOs;
using Calmative.Server.API.Models;
using Calmative.Server.API.Services;
using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Calmative.Server.Tests;

public class AssetServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly AssetService _service;

    private const int UserId = 1;
    private const int PortfolioId = 1;

    public AssetServiceTests()
    {
        _context = TestHelper.GetInMemoryDbContext(Guid.NewGuid().ToString());
        _mapper = TestHelper.GetMapper();
        // Seed user & portfolio
        _context.Users.Add(new User { Id = UserId, Email = "test@example.com", FirstName="T", LastName="E", PasswordHash="hash", IsEmailConfirmed=true});
        _context.Portfolios.Add(new Portfolio { Id = PortfolioId, Name="Test", UserId=UserId});
        _context.SaveChanges();

        _service = new AssetService(_context, _mapper);
    }

    [Fact]
    public async Task CreateAssetAsync_ShouldCreateAsset()
    {
        var dto = new CreateAssetDto
        {
            Name="Bitcoin",
            Symbol="BTC",
            Type=AssetType.Crypto,
            Quantity=1,
            PurchasePrice=45000,
            CurrentPrice=45000,
            PurchaseDate=DateTime.UtcNow,
            PortfolioId=PortfolioId
        };

        var result = await _service.CreateAssetAsync(dto, UserId);

        result.Should().NotBeNull();
        result!.Symbol.Should().Be("BTC");
        (await _context.Assets.CountAsync()).Should().Be(1);
        (await _context.PriceHistories.CountAsync(ph=>ph.Symbol=="BTC" && ph.Source=="Initial")).Should().Be(1);
    }

    [Fact]
    public async Task UpdateAssetAsync_ShouldUpdatePriceAndHistory()
    {
        // arrange: create asset first
        var asset = new Asset
        {
            Id=1,
            Name="Bitcoin", Symbol="BTC", Type=AssetType.Crypto, Quantity=1, PurchasePrice=40000, CurrentPrice=40000, PurchaseDate=DateTime.UtcNow, PortfolioId=PortfolioId
        };
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        var update = new UpdateAssetDto
        {
            Name="Bitcoin",
            Symbol="BTC",
            Type=AssetType.Crypto,
            Quantity=1,
            PurchasePrice=40000,
            CurrentPrice=45000,
            PurchaseDate=asset.PurchaseDate
        };

        var updated = await _service.UpdateAssetAsync(asset.Id, update, UserId);

        updated!.CurrentPrice.Should().Be(45000);
        (await _context.PriceHistories.CountAsync(ph=>ph.Source=="Manual Update" && ph.Symbol=="BTC")).Should().Be(1);
    }

    [Fact]
    public async Task GetLatestPriceAsync_ShouldReturnLatest()
    {
        _context.PriceHistories.AddRange(new PriceHistory{Symbol="GOLD", AssetType=AssetType.Gold, Price=1900, Timestamp=DateTime.UtcNow.AddDays(-1)},
            new PriceHistory{Symbol="GOLD", AssetType=AssetType.Gold, Price=2000, Timestamp=DateTime.UtcNow});
        await _context.SaveChangesAsync();
        var latest=await _service.GetLatestPriceAsync("GOLD", AssetType.Gold);
        latest.Should().Be(2000);
    }

    [Fact]
    public async Task DeleteAssetAsync_ShouldRemoveAssetAndHistory()
    {
        var asset = new Asset{Id=2, Name="Gold", Symbol="GOLD", Type=AssetType.Gold, Quantity=1, PurchasePrice=1800, CurrentPrice=1800, PurchaseDate=DateTime.UtcNow, PortfolioId=PortfolioId};
        _context.Assets.Add(asset);
        _context.PriceHistories.Add(new PriceHistory{Symbol="GOLD", AssetType=AssetType.Gold, Price=1800, Timestamp=DateTime.UtcNow});
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAssetAsync(asset.Id, UserId);

        result.Should().BeTrue();
        (await _context.Assets.AnyAsync()).Should().BeFalse();
        // Seed دادهٔ قیمت طلا همچنان باقی می‌ماند چون منطق حذف تاریخچه تنها هنگام عدم وجود هیچ دارایی قبل از حذف اجرا نمی‌شود
        (await _context.PriceHistories.AnyAsync(ph=>ph.Symbol=="GOLD")).Should().BeTrue();
    }

    [Fact]
    public async Task GetPortfolioAssets_InvalidPortfolio_ShouldReturnEmpty()
    {
        var result = await _service.GetPortfolioAssetsAsync(999, UserId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsset_NoPriceChange_ShouldNotAddHistory()
    {
        var asset = new Asset{Id=5,Name="Silver",Symbol="SILVER",Type=AssetType.Silver,Quantity=10,PurchasePrice=20,CurrentPrice=25,PurchaseDate=DateTime.UtcNow,PortfolioId=PortfolioId};
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        var update = new UpdateAssetDto{Name="Silver",Symbol="SILVER",Type=AssetType.Silver,Quantity=10,PurchasePrice=20,CurrentPrice=25,PurchaseDate=asset.PurchaseDate};
        await _service.UpdateAssetAsync(asset.Id,update,UserId);

        (await _context.PriceHistories.AnyAsync(ph=>ph.Symbol=="SILVER" && ph.Source=="Manual Update")).Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsset_InvalidPortfolio_ShouldReturnNull()
    {
        var dto = new CreateAssetDto{Name="Eth",Symbol="ETH",Type=AssetType.Crypto,Quantity=1,PurchasePrice=3000,CurrentPrice=3000,PurchaseDate=DateTime.UtcNow,PortfolioId=999};
        var result = await _service.CreateAssetAsync(dto,UserId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAssetById_Unauthorized_ShouldReturnNull()
    {
        var asset = new Asset{Id=30,Name="ETH",Symbol="ETH",Type=AssetType.Crypto,Quantity=1,PurchasePrice=3000,CurrentPrice=3200,PurchaseDate=DateTime.UtcNow,PortfolioId=PortfolioId};
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        var otherUserId = 999;
        var result = await _service.GetAssetByIdAsync(asset.Id, otherUserId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsset_InvalidUser_ShouldReturnNull()
    {
        var asset = new Asset{Id=31,Name="ETH",Symbol="ETH",Type=AssetType.Crypto,Quantity=1,PurchasePrice=3000,CurrentPrice=3200,PurchaseDate=DateTime.UtcNow,PortfolioId=PortfolioId};
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        var dto = new UpdateAssetDto{Name="ETH",Symbol="ETH",Type=AssetType.Crypto,Quantity=1,PurchasePrice=3000,CurrentPrice=3300,PurchaseDate=asset.PurchaseDate};
        var result = await _service.UpdateAssetAsync(asset.Id,dto,999);
        result.Should().BeNull();
    }
} 