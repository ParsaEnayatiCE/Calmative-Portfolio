using System.Reflection;
using AutoMapper;
using Calmative.Server.API.Data;
using Calmative.Server.API.Models;
using Calmative.Server.API.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calmative.Server.Tests;

public class PriceUpdateServiceTests
{
    private IServiceProvider BuildServiceProvider(string dbName)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task UpdatePrices_Should_AddPriceHistory_And_UpdateAssetPrices()
    {
        var dbName = Guid.NewGuid().ToString();
        var provider = BuildServiceProvider(dbName);
        var logger = provider.GetRequiredService<ILogger<PriceUpdateService>>();
        using (var scope = provider.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            ctx.Assets.AddRange(
                new Asset { Symbol = "USD", Type = AssetType.Currency, Quantity = 100, CurrentPrice = 1, Name = "US Dollar" },
                new Asset { Symbol = "BTC", Type = AssetType.Crypto, Quantity = 2, CurrentPrice = 45000, Name = "Bitcoin" }
            );
            await ctx.SaveChangesAsync();
        }

        var service = new PriceUpdateService(provider, logger);

        // Use reflection to invoke the private UpdatePricesAsync method directly
        var method = typeof(PriceUpdateService).GetMethod("UpdatePricesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(service, null)!;
        await task; // wait for internal logic

        using (var scope = provider.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            ctx.PriceHistories.Should().NotBeEmpty();
            // Ensure asset prices were updated (no longer default values)
            var btc = await ctx.Assets.FirstAsync(a => a.Symbol == "BTC");
            btc.CurrentPrice.Should().BeGreaterThan(0);
        }
    }

    [Theory]
    [InlineData("USD", AssetType.Currency)]
    [InlineData("EUR", AssetType.Currency)]
    [InlineData("BTC", AssetType.Crypto)]
    [InlineData("ETH", AssetType.Crypto)]
    [InlineData("GOLD", AssetType.Gold)]
    [InlineData("SILVER", AssetType.Silver)]
    public async Task GetLatestPriceFromExternalApi_Should_Return_Positive_Price(string symbol, AssetType type)
    {
        var provider = BuildServiceProvider(Guid.NewGuid().ToString());
        var logger = provider.GetRequiredService<ILogger<PriceUpdateService>>();
        var service = new PriceUpdateService(provider, logger);
        var method = typeof(PriceUpdateService).GetMethod("GetLatestPriceFromExternalApi", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();

        var task = (Task<decimal>)method!.Invoke(service, new object[] { symbol, type })!;
        var price = await task;
        price.Should().BeGreaterThan(0);
    }
} 