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

    [Fact]
    public async Task Asset_Growth_Positive_Should_Recommend_Increase()
    {
        var (svc, ctx) = Build(Guid.NewGuid().ToString());
        var user = new User { Email="g@test.com", FirstName="G", LastName="Up", PasswordHash="h", IsEmailConfirmed=true };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var port = new Portfolio { Name="Crypto", UserId=user.Id };
        ctx.Portfolios.Add(port);
        await ctx.SaveChangesAsync();

        ctx.Assets.Add(new Asset{ Name="ADA", Symbol="ADA", Type=AssetType.Crypto, Quantity=100, CurrentPrice=0.6m,PurchasePrice=0.5m, PurchaseDate=DateTime.UtcNow, PortfolioId=port.Id});
        ctx.PriceHistories.AddRange(
            new PriceHistory{Symbol="ADA", AssetType=AssetType.Crypto, Price=0.5m, Timestamp=DateTime.UtcNow.AddDays(-2)},
            new PriceHistory{Symbol="ADA", AssetType=AssetType.Crypto, Price=0.6m, Timestamp=DateTime.UtcNow}
        );
        await ctx.SaveChangesAsync();

        var rec = await svc.GetRecommendationsForUserAsync(user.Id);
        rec.RecommendedAssets.Should().Contain(r=>r.Symbol=="ADA" && r.Reason.Contains("افزایش"));
    }

    [Fact]
    public async Task Asset_Growth_Negative_Should_Recommend_Review()
    {
        var (svc, ctx) = Build(Guid.NewGuid().ToString());
        var user = new User { Email="d@test.com", FirstName="D", LastName="Down", PasswordHash="h", IsEmailConfirmed=true };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var port = new Portfolio { Name="Crypto", UserId=user.Id };
        ctx.Portfolios.Add(port);
        await ctx.SaveChangesAsync();

        ctx.Assets.Add(new Asset{ Name="DOT", Symbol="DOT", Type=AssetType.Crypto, Quantity=50, CurrentPrice=5m,PurchasePrice=6m, PurchaseDate=DateTime.UtcNow, PortfolioId=port.Id});
        ctx.PriceHistories.AddRange(
            new PriceHistory{Symbol="DOT", AssetType=AssetType.Crypto, Price=6m, Timestamp=DateTime.UtcNow.AddDays(-2)},
            new PriceHistory{Symbol="DOT", AssetType=AssetType.Crypto, Price=5m, Timestamp=DateTime.UtcNow}
        );
        await ctx.SaveChangesAsync();

        var rec = await svc.GetRecommendationsForUserAsync(user.Id);
        rec.RecommendedAssets.Should().Contain(r=>r.Symbol=="DOT" && r.Reason.Contains("بررسی"));
    }
} 