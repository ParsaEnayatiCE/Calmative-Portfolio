using Calmative.Server.API.Data;
using Calmative.Server.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calmative.Server.API.Services
{
    public class PriceUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PriceUpdateService> _logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(15); // Update every 15 minutes

        public PriceUpdateService(IServiceProvider serviceProvider, ILogger<PriceUpdateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Price Update Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdatePricesAsync();
                    _logger.LogInformation("Price update completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating prices");
                }

                await Task.Delay(_updateInterval, stoppingToken);
            }
        }

        private async Task UpdatePricesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get all unique symbols and asset types from current assets
            var uniqueAssets = await context.Assets
                .Select(a => new { a.Symbol, a.Type })
                .Distinct()
                .ToListAsync();

            foreach (var asset in uniqueAssets)
            {
                var newPrice = await GetLatestPriceFromExternalApi(asset.Symbol, asset.Type);
                
                if (newPrice > 0)
                {
                    // Add new price to history
                    var priceHistory = new PriceHistory
                    {
                        Symbol = asset.Symbol,
                        AssetType = asset.Type,
                        Price = newPrice,
                        Timestamp = DateTime.UtcNow,
                        Source = "Auto-Update"
                    };

                    context.PriceHistories.Add(priceHistory);

                    // Update current price in assets
                    var assetsToUpdate = await context.Assets
                        .Where(a => a.Symbol == asset.Symbol && a.Type == asset.Type)
                        .ToListAsync();

                    foreach (var assetToUpdate in assetsToUpdate)
                    {
                        assetToUpdate.CurrentPrice = newPrice;
                        assetToUpdate.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        private async Task<decimal> GetLatestPriceFromExternalApi(string symbol, AssetType assetType)
        {
            // Simulate external API call with mock data
            // In real implementation, you would call actual APIs like:
            // - CoinGecko API for crypto
            // - Currency API for currencies  
            // - Metal prices API for gold/silver
            
            await Task.Delay(100); // Simulate network delay

            var random = new Random();
            
            return assetType switch
            {
                AssetType.Currency => GetMockCurrencyPrice(symbol, random),
                AssetType.Crypto => GetMockCryptoPrice(symbol, random),
                AssetType.Gold => GetMockGoldPrice(random),
                AssetType.Silver => GetMockSilverPrice(random),
                AssetType.PreciousMetals => GetMockPreciousMetalPrice(symbol, random),
                _ => 0
            };
        }

        private decimal GetMockCurrencyPrice(string symbol, Random random)
        {
            return symbol switch
            {
                "USD" => 1.0m,
                "EUR" => 0.85m + (decimal)(random.NextDouble() * 0.1 - 0.05), // €0.80-0.90
                "GBP" => 0.75m + (decimal)(random.NextDouble() * 0.1 - 0.05), // £0.70-0.80
                "JPY" => 150m + (decimal)(random.NextDouble() * 20 - 10),     // ¥140-160
                _ => 1.0m
            };
        }

        private decimal GetMockCryptoPrice(string symbol, Random random)
        {
            return symbol switch
            {
                "BTC" => 45000m + (decimal)(random.NextDouble() * 10000 - 5000), // $40k-$50k
                "ETH" => 3000m + (decimal)(random.NextDouble() * 1000 - 500),    // $2.5k-$3.5k
                "ADA" => 0.5m + (decimal)(random.NextDouble() * 0.4 - 0.2),     // $0.3-$0.7
                "DOT" => 6m + (decimal)(random.NextDouble() * 4 - 2),           // $4-$8
                _ => 1.0m
            };
        }

        private decimal GetMockGoldPrice(Random random)
        {
            return 2000m + (decimal)(random.NextDouble() * 200 - 100); // $1900-$2100 per oz
        }

        private decimal GetMockSilverPrice(Random random)
        {
            return 25m + (decimal)(random.NextDouble() * 10 - 5); // $20-$30 per oz
        }

        private decimal GetMockPreciousMetalPrice(string symbol, Random random)
        {
            return symbol switch
            {
                "PLATINUM" => 1000m + (decimal)(random.NextDouble() * 200 - 100),
                "PALLADIUM" => 2000m + (decimal)(random.NextDouble() * 400 - 200),
                _ => 100m
            };
        }
    }
} 