using Calmative.Server.API.Data;
using Calmative.Server.API.DTOs;
using Calmative.Server.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Calmative.Server.API.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecommendationService> _logger;
        private readonly IPortfolioService _portfolioService;
        private readonly IAssetService _assetService;
        
        public RecommendationService(
            ApplicationDbContext context, 
            ILogger<RecommendationService> logger,
            IPortfolioService portfolioService,
            IAssetService assetService)
        {
            _context = context;
            _logger = logger;
            _portfolioService = portfolioService;
            _assetService = assetService;
        }
        
        public async Task<RecommendationDto> GetRecommendationsForUserAsync(int userId)
        {
            // Get all user portfolios
            var portfolios = await _context.Portfolios
                .Where(p => p.UserId == userId)
                .Include(p => p.Assets)
                .ToListAsync();
                
            if (!portfolios.Any())
            {
                return new RecommendationDto(); // Empty recommendations if no portfolios
            }
            
            var recommendations = new RecommendationDto();
            
            // Analyze portfolio distribution
            var allAssets = portfolios.SelectMany(p => p.Assets).ToList();
            var totalValue = allAssets.Sum(a => a.Quantity * a.CurrentPrice);
            
            // Get asset type distribution
            var assetTypeDistribution = allAssets
                .GroupBy(a => a.Type)
                .Select(g => new AssetTypeDistributionInfo
                {
                    Type = g.Key,
                    Value = g.Sum(a => a.Quantity * a.CurrentPrice),
                    Percentage = totalValue > 0 ? (g.Sum(a => a.Quantity * a.CurrentPrice) / totalValue) * 100 : 0
                })
                .OrderByDescending(d => d.Value)
                .ToList();
                
            // Generate portfolio recommendations
            recommendations.PortfolioSuggestions = GeneratePortfolioRecommendations(assetTypeDistribution, allAssets);
            
            // Generate asset recommendations
            recommendations.RecommendedAssets = await GenerateAssetRecommendations(userId, allAssets, assetTypeDistribution);
            
            return recommendations;
        }
        
        public async Task<RecommendationDto> GetRecommendationsForPortfolioAsync(int portfolioId, int userId)
        {
            // Get the portfolio with assets
            var portfolio = await _context.Portfolios
                .Where(p => p.Id == portfolioId && p.UserId == userId)
                .Include(p => p.Assets)
                .FirstOrDefaultAsync();
                
            if (portfolio == null)
            {
                throw new NotFoundException($"Portfolio with ID {portfolioId} not found");
            }
            
            var recommendations = new RecommendationDto();
            
            // Analyze portfolio distribution
            var assets = portfolio.Assets.ToList();
            var totalValue = assets.Sum(a => a.Quantity * a.CurrentPrice);
            
            // Get asset type distribution
            var assetTypeDistribution = assets
                .GroupBy(a => a.Type)
                .Select(g => new AssetTypeDistributionInfo
                {
                    Type = g.Key,
                    Value = g.Sum(a => a.Quantity * a.CurrentPrice),
                    Percentage = totalValue > 0 ? (g.Sum(a => a.Quantity * a.CurrentPrice) / totalValue) * 100 : 0
                })
                .OrderByDescending(d => d.Value)
                .ToList();
                
            // Generate portfolio recommendations
            recommendations.PortfolioSuggestions = GeneratePortfolioRecommendations(assetTypeDistribution, assets);
            
            // Generate asset recommendations specific to this portfolio
            recommendations.RecommendedAssets = await GenerateAssetRecommendations(userId, assets, assetTypeDistribution);
            
            return recommendations;
        }
        
        private List<PortfolioRecommendationDto> GeneratePortfolioRecommendations(
            List<AssetTypeDistributionInfo> assetTypeDistribution, 
            List<Asset> assets)
        {
            var recommendations = new List<PortfolioRecommendationDto>();
            
            // Check if portfolio is well diversified
            if (assetTypeDistribution.Count <= 2)
            {
                recommendations.Add(new PortfolioRecommendationDto
                {
                    RecommendationType = "Diversification",
                    Description = "تنوع پورتفولیو شما کم است. توصیه می‌شود در انواع مختلف دارایی سرمایه‌گذاری کنید.",
                    Reasoning = "پورتفولیوهای متنوع ریسک کمتری دارند و در برابر نوسانات بازار مقاوم‌تر هستند.",
                    Strength = RecommendationStrength.High
                });
            }
            
            // Check for overconcentration
            var highestConcentration = assetTypeDistribution.FirstOrDefault();
            if (highestConcentration != null && highestConcentration.Percentage > 70)
            {
                recommendations.Add(new PortfolioRecommendationDto
                {
                    RecommendationType = "Rebalancing",
                    Description = $"تمرکز بالا در {highestConcentration.Type}. توصیه می‌شود پورتفولیو خود را متعادل کنید.",
                    Reasoning = "تمرکز بیش از 70% در یک نوع دارایی می‌تواند ریسک پورتفولیو را افزایش دهد.",
                    Strength = RecommendationStrength.Medium
                });
            }
            
            // Check for risk exposure based on asset volatility
            var volatileAssets = assets.Where(a => 
                a.Type == AssetType.Crypto || 
                a.Type == AssetType.Stock).ToList();
                
            var volatileValue = volatileAssets.Sum(a => a.Quantity * a.CurrentPrice);
            var totalValue = assets.Sum(a => a.Quantity * a.CurrentPrice);
            var volatilePercentage = totalValue > 0 ? (volatileValue / totalValue) * 100 : 0;
            
            if (volatilePercentage > 60)
            {
                recommendations.Add(new PortfolioRecommendationDto
                {
                    RecommendationType = "Risk Management",
                    Description = "ریسک پورتفولیو شما بالاست. توصیه می‌شود بخشی از سرمایه را به دارایی‌های کم ریسک‌تر منتقل کنید.",
                    Reasoning = "دارایی‌های با نوسان بالا مانند ارزهای دیجیتال و سهام بیش از 60% پورتفولیو را تشکیل می‌دهند.",
                    Strength = RecommendationStrength.Medium
                });
            }
            
            return recommendations;
        }
        
        private async Task<List<AssetRecommendationDto>> GenerateAssetRecommendations(
            int userId, 
            List<Asset> currentAssets,
            List<AssetTypeDistributionInfo> assetTypeDistribution)
        {
            var recommendations = new List<AssetRecommendationDto>();
            
            // Build a hash set of symbols the user currently owns
            var userSymbols = currentAssets
                .Select(a => a.Symbol?.ToUpperInvariant())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToHashSet();

            // Get price history only for the user's symbols to avoid suggesting unknown assets
            var priceHistories = await _context.PriceHistories
                .Where(ph => userSymbols.Contains(ph.Symbol.ToUpper()))
                .OrderByDescending(ph => ph.Timestamp)
                .Take(1000) // recent data
                .ToListAsync();
                
            // Group price histories by symbol and asset type
            var groupedPrices = priceHistories
                .GroupBy(ph => new { ph.Symbol, ph.AssetType })
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(ph => ph.Timestamp).ToList()
                );
                
            // Find assets with strong recent performance
            foreach (var group in groupedPrices)
            {
                if (group.Value.Count < 2) continue;
                
                var oldestPrice = group.Value.First().Price;
                var newestPrice = group.Value.Last().Price;
                var growthPercentage = ((newestPrice - oldestPrice) / oldestPrice) * 100;
                
                // If asset shows good growth, recommend increasing position (user already owns it)
                if (growthPercentage > 5)
                {
                    // Check if user already has this asset
                    var existingAsset = currentAssets.FirstOrDefault(a => 
                        a.Symbol == group.Key.Symbol && a.Type == group.Key.AssetType);
                        
                    if (existingAsset != null)
                    {
                        // Recommend increasing existing position
                        recommendations.Add(new AssetRecommendationDto
                        {
                            Name = existingAsset?.Name ?? group.Key.Symbol,
                            Symbol = group.Key.Symbol,
                            Type = group.Key.AssetType,
                            RecommendedAllocation = 0, // Don't specify exact allocation for existing assets
                            EstimatedGrowthPercentage = growthPercentage,
                            Reason = "افزایش موقعیت",
                            AnalysisText = $"این دارایی عملکرد خوبی داشته ({growthPercentage:F2}% رشد). می‌توانید موقعیت خود را افزایش دهید.",
                            Strength = growthPercentage > 10 ? RecommendationStrength.High : 
                                       (growthPercentage > 7 ? RecommendationStrength.Medium : 
                                        RecommendationStrength.Low)
                        });
                    }
                }
                // For assets showing decline, consider recommending sell or hold
                else if (growthPercentage < -5)
                {
                    // Only recommend selling if user has this asset
                    var existingAsset = currentAssets.FirstOrDefault(a => 
                        a.Symbol == group.Key.Symbol && a.Type == group.Key.AssetType);
                        
                    if (existingAsset != null)
                    {
                        var strength = growthPercentage < -10 ? RecommendationStrength.High : 
                                      (growthPercentage < -7 ? RecommendationStrength.Medium : 
                                       RecommendationStrength.Low);
                                       
                        recommendations.Add(new AssetRecommendationDto
                        {
                            Name = existingAsset.Name,
                            Symbol = existingAsset.Symbol,
                            Type = existingAsset.Type,
                            RecommendedAllocation = 0,
                            EstimatedGrowthPercentage = growthPercentage,
                            Reason = "بررسی مجدد موقعیت",
                            AnalysisText = $"این دارایی {Math.Abs(growthPercentage):F2}% افت داشته است. بررسی مجدد موقعیت توصیه می‌شود.",
                            Strength = strength
                        });
                    }
                }
            }
            
            // Check portfolio diversification and recommend asset types that are underrepresented
            var recommendedAssetTypes = new List<AssetType>();
            
            bool hasRealEstate = currentAssets.Any(a => a.Type == AssetType.RealEstate);
            bool hasGoldLike   = currentAssets.Any(a => a.Type == AssetType.Gold ||
                                                     a.Type == AssetType.Silver ||
                                                     a.Type == AssetType.PreciousMetals);

            if (!hasRealEstate)
            {
                recommendedAssetTypes.Add(AssetType.RealEstate);
            }

            if (!hasGoldLike)
            {
                recommendedAssetTypes.Add(AssetType.Gold);
            }
            
            // Add generic recommendations for diversification
            foreach (var assetType in recommendedAssetTypes.Except(currentAssets.Select(a=>a.Type)))
            {
                recommendations.Add(new AssetRecommendationDto
                {
                    Name = GetAssetTypeDisplayName(assetType),
                    Symbol = assetType.ToString().ToUpper(),
                    Type = assetType,
                    RecommendedAllocation = 10, // Suggest 10% allocation for diversification
                    EstimatedGrowthPercentage = 0, // No specific growth prediction
                    Reason = "تنوع بخشی پورتفولیو",
                    AnalysisText = $"افزودن {GetAssetTypeDisplayName(assetType)} به پورتفولیو شما می‌تواند به متعادل‌سازی ریسک کمک کند.",
                    Strength = RecommendationStrength.Medium
                });
            }
            
            // Return ordered by strength
            return recommendations
                .OrderByDescending(r => r.Strength)
                .ThenByDescending(r => Math.Abs(r.EstimatedGrowthPercentage))
                .ToList();
        }
        
        private string GetAssetTypeDisplayName(AssetType type)
        {
            return type switch
            {
                AssetType.Currency => "ارز",
                AssetType.Gold => "طلا",
                AssetType.Silver => "نقره",
                AssetType.Crypto => "ارز دیجیتال",
                AssetType.PreciousMetals => "فلزات گرانبها",
                AssetType.Car => "خودرو",
                AssetType.RealEstate => "املاک و مستغلات",
                AssetType.Stock => "سهام",
                AssetType.Bond => "اوراق قرضه",
                AssetType.ETF => "صندوق‌های قابل معامله",
                _ => type.ToString()
            };
        }
    }
    
    // Helper class to fix the dynamic type conversion issues
    public class AssetTypeDistributionInfo
    {
        public AssetType Type { get; set; }
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
    }
} 