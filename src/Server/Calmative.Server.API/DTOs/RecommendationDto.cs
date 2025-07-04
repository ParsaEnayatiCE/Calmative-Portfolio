using Calmative.Server.API.Models;

namespace Calmative.Server.API.DTOs
{
    public class RecommendationDto
    {
        public List<AssetRecommendationDto> RecommendedAssets { get; set; } = new();
        public List<PortfolioRecommendationDto> PortfolioSuggestions { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
    
    public class AssetRecommendationDto
    {
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public AssetType Type { get; set; }
        public decimal RecommendedAllocation { get; set; }
        public decimal EstimatedGrowthPercentage { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string AnalysisText { get; set; } = string.Empty;
        public RecommendationStrength Strength { get; set; }
    }
    
    public class PortfolioRecommendationDto
    {
        public string RecommendationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public RecommendationStrength Strength { get; set; }
    }
    
    public enum RecommendationStrength
    {
        Low,
        Medium,
        High
    }
} 