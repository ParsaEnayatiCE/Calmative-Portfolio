using System.ComponentModel.DataAnnotations;

namespace Calmative.Web.App.Models.ViewModels
{
    public class RecommendationViewModel
    {
        public List<AssetRecommendationViewModel> RecommendedAssets { get; set; } = new();
        public List<PortfolioRecommendationViewModel> PortfolioSuggestions { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public string GeneratedAtFormatted => GeneratedAt.ToString("yyyy/MM/dd HH:mm");
    }
    
    public class AssetRecommendationViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public AssetType Type { get; set; }
        public string TypeDisplayName { get; set; } = string.Empty;
        public decimal RecommendedAllocation { get; set; }
        public decimal EstimatedGrowthPercentage { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string AnalysisText { get; set; } = string.Empty;
        public RecommendationStrength Strength { get; set; }
        public string StrengthClass => RecommendationHelper.GetStrengthClass(Strength);
        public string StrengthText => RecommendationHelper.GetStrengthText(Strength);
        public bool IsPositiveGrowth => EstimatedGrowthPercentage >= 0;
    }
    
    public class PortfolioRecommendationViewModel
    {
        public string RecommendationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public RecommendationStrength Strength { get; set; }
        public string StrengthClass => RecommendationHelper.GetStrengthClass(Strength);
        public string StrengthText => RecommendationHelper.GetStrengthText(Strength);
    }
    
    public enum RecommendationStrength
    {
        Low,
        Medium,
        High
    }
    
    public static class RecommendationHelper
    {
        public static string GetStrengthClass(RecommendationStrength strength)
        {
            return strength switch
            {
                RecommendationStrength.Low => "text-info",
                RecommendationStrength.Medium => "text-warning",
                RecommendationStrength.High => "text-danger",
                _ => "text-secondary"
            };
        }
        
        public static string GetStrengthText(RecommendationStrength strength)
        {
            return strength switch
            {
                RecommendationStrength.Low => "کم",
                RecommendationStrength.Medium => "متوسط",
                RecommendationStrength.High => "زیاد",
                _ => "نامشخص"
            };
        }
    }
} 