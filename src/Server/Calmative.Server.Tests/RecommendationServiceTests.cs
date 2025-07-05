using AutoMapper;
using Calmative.Server.API.Data;
using Calmative.Server.API.DTOs;
using Calmative.Server.API.Models;
using Calmative.Server.API.Mappings;
using Calmative.Server.API.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calmative.Server.Tests;

public class RecommendationServiceTests
{
    private (RecommendationService Svc, ApplicationDbContext Ctx) Build(string dbName)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(opts => opts.UseInMemoryDatabase(dbName));

        // Mapper
        var mapperCfg = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        services.AddSingleton<IMapper>(mapperCfg.CreateMapper());

        // mocks (not used internally but required by constructor)
        var portMock = new Mock<IPortfolioService>();
        var assetMock = new Mock<IAssetService>();
        services.AddSingleton(portMock.Object);
        services.AddSingleton(assetMock.Object);

        var provider = services.BuildServiceProvider();
        var ctx = provider.GetRequiredService<ApplicationDbContext>();
        var logger = provider.GetRequiredService<ILogger<RecommendationService>>();

        var svc = new RecommendationService(ctx, logger, portMock.Object, assetMock.Object);
        return (svc, ctx);
    }

    [Fact]
    public async Task Concentrated_Portfolio_Should_Trigger_Diversification_Advice()
    {
        var (svc, ctx) = Build(Guid.NewGuid().ToString());
        var user = new User { Email = "u@test.com", FirstName = "U", LastName = "Ser", PasswordHash = "pwd", IsEmailConfirmed = true };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var port = new Portfolio { Name = "Crypto", UserId = user.Id };
        ctx.Portfolios.Add(port);
        await ctx.SaveChangesAsync();

        ctx.Assets.AddRange(
            new Asset { Name = "Bitcoin", Symbol = "BTC", Type = AssetType.Crypto, Quantity = 1, CurrentPrice = 45000, PortfolioId = port.Id },
            new Asset { Name = "Ethereum", Symbol = "ETH", Type = AssetType.Crypto, Quantity = 10, CurrentPrice = 3000, PortfolioId = port.Id }
        );
        await ctx.SaveChangesAsync();

        var rec = await svc.GetRecommendationsForUserAsync(user.Id);
        rec.PortfolioSuggestions.Should().Contain(s => s.RecommendationType == "Diversification");
    }

    [Fact]
    public async Task Portfolio_Level_Recommendation_Should_Return_Results()
    {
        var (svc, ctx) = Build(Guid.NewGuid().ToString());
        var user = new User { Email = "u2@test.com", FirstName = "U2", LastName = "Ser", PasswordHash = "pwd", IsEmailConfirmed = true };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var port = new Portfolio { Name = "Mix", UserId = user.Id };
        ctx.Portfolios.Add(port);
        await ctx.SaveChangesAsync();

        ctx.Assets.AddRange(
            new Asset { Name = "Gold", Symbol = "GOLD", Type = AssetType.Gold, Quantity = 1, CurrentPrice = 2000, PortfolioId = port.Id },
            new Asset { Name = "Dollar", Symbol = "USD", Type = AssetType.Currency, Quantity = 1000, CurrentPrice = 1, PortfolioId = port.Id }
        );
        await ctx.SaveChangesAsync();

        var rec = await svc.GetRecommendationsForPortfolioAsync(port.Id, user.Id);
        rec.PortfolioSuggestions.Should().NotBeNull();
    }
} 