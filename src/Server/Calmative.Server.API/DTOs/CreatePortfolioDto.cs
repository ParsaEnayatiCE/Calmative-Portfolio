using System.ComponentModel.DataAnnotations;

namespace Calmative.Server.API.DTOs
{
    public class CreatePortfolioDto
    {
        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdatePortfolioDto
    {
        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class PortfolioSummaryDto
    {
        public int PortfolioId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalValue { get; set; }
        public decimal TotalInvestment { get; set; }
        public decimal TotalProfitLoss { get; set; }
        public decimal TotalProfitLossPercentage { get; set; }
        public int AssetCount { get; set; }
        public List<AssetTypeDistribution> AssetTypeDistribution { get; set; } = new();
        public List<AssetDto> TopPerformingAssets { get; set; } = new();
    }

    public class AssetTypeDistribution
    {
        public string AssetType { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
        public int Count { get; set; }
    }
} 