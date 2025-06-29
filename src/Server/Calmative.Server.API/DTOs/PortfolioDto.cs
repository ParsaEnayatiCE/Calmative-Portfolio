using System.ComponentModel.DataAnnotations;
using Calmative.Server.API.Models;

namespace Calmative.Server.API.DTOs
{
    public class PortfolioDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<AssetDto> Assets { get; set; } = new List<AssetDto>();
        public decimal TotalValue => Assets.Sum(a => a.TotalValue);
        public decimal TotalInvestment => Assets.Sum(a => a.Quantity * a.PurchasePrice);
        public decimal TotalProfitLoss => TotalValue - TotalInvestment;
        public decimal TotalProfitLossPercentage => TotalInvestment > 0 ? (TotalProfitLoss / TotalInvestment) * 100 : 0;
        public List<AssetTypeDistribution> AssetTypeDistribution { get; set; } = new();
        public bool IsProfit => TotalProfitLoss >= 0;
    }

    public class AssetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public AssetType Type { get; set; }
        public decimal Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int PortfolioId { get; set; }
        public decimal TotalValue => Quantity * CurrentPrice;
        public decimal ProfitLoss => TotalValue - (Quantity * PurchasePrice);
        public decimal ProfitLossPercentage => 
            PurchasePrice > 0 ? ((CurrentPrice - PurchasePrice) / PurchasePrice) * 100 : 0;
    }

    public class CreateAssetDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Symbol { get; set; } = string.Empty;

        public AssetType Type { get; set; }

        [Range(0.00000001, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal PurchasePrice { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal CurrentPrice { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        public int PortfolioId { get; set; }
    }

    public class PriceHistoryDto
    {
        public DateTime Timestamp { get; set; }
        public decimal Price { get; set; }
    }
} 