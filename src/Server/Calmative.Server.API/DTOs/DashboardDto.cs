namespace Calmative.Server.API.DTOs
{
    public class DashboardOverviewDto
    {
        public int PortfolioCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalInvestment { get; set; }
        public decimal TotalProfitLoss { get; set; }
        public decimal TotalProfitLossPercentage { get; set; }
        public List<AssetTypeDistribution> AssetTypeDistribution { get; set; } = new();
        public List<AssetDto> TopPerformingAssets { get; set; } = new();
        public List<PortfolioSummaryDto> PortfolioSummaries { get; set; } = new();
    }

    public class PerformanceDto
    {
        public List<AssetDto> BestPerformers { get; set; } = new();
        public List<AssetDto> WorstPerformers { get; set; } = new();
        public List<AssetDto> RecentPurchases { get; set; } = new();
    }
} 