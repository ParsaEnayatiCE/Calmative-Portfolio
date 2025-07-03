using System.ComponentModel.DataAnnotations;

namespace Calmative.Web.App.Models.ViewModels
{
    public enum AssetType
    {
        Unknown = 0,
        Currency = 1,
        Gold = 2,
        Silver = 3,
        Crypto = 4,
        PreciousMetals = 5,
        Car = 6,
        RealEstate = 7,
        Stock = 8,
        Bond = 9,
        ETF = 10,
        Custom = 11
    }

    public class PortfolioViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<AssetViewModel> Assets { get; set; } = new();
        public decimal TotalValue => Assets.Sum(a => a.TotalValue);
        public decimal TotalInvestment => Assets.Sum(a => a.Quantity * a.PurchasePrice);
        public decimal TotalProfitLoss => TotalValue - TotalInvestment;
        public decimal TotalProfitLossPercentage => TotalInvestment > 0 ? (TotalProfitLoss / TotalInvestment) * 100 : 0;
        public int AssetCount => Assets.Count;
        public bool IsProfit => TotalProfitLoss >= 0;
    }

    public class CreatePortfolioViewModel
    {
        [Required(ErrorMessage = "نام پورتفولیو اجباری است")]
        [Display(Name = "نام پورتفولیو")]
        [StringLength(100, ErrorMessage = "نام نباید بیش از 100 کاراکتر باشد", MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نباید بیش از 500 کاراکتر باشد")]
        public string? Description { get; set; }
    }

    public class UpdatePortfolioViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "نام پورتفولیو اجباری است")]
        [Display(Name = "نام پورتفولیو")]
        [StringLength(100, ErrorMessage = "نام نباید بیش از 100 کاراکتر باشد", MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نباید بیش از 500 کاراکتر باشد")]
        public string? Description { get; set; }
    }

    public class AssetViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public AssetType Type { get; set; }
        public string TypeDisplayName => GetAssetTypeDisplayName(Type);
        public decimal Quantity { get; set; }
        public decimal PurchasePrice { get; set; }

        [Display(Name = "قیمت فعلی")]
        public decimal CurrentPrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int PortfolioId { get; set; }
        public decimal TotalValue => Quantity * CurrentPrice;
        public decimal ProfitLoss => TotalValue - (Quantity * PurchasePrice);
        public decimal ProfitLossPercentage => PurchasePrice > 0 ? ((CurrentPrice - PurchasePrice) / PurchasePrice) * 100 : 0;
        public bool IsProfit => ProfitLoss >= 0;

        public static string GetAssetTypeDisplayName(AssetType type)
        {
            // Handle built-in types
            if (type <= AssetType.ETF)
            {
                return type switch
                {
                    AssetType.Currency => "ارز",
                    AssetType.Gold => "طلا",
                    AssetType.Silver => "نقره",
                    AssetType.Crypto => "رمزارز",
                    AssetType.PreciousMetals => "فلزات گرانبها",
                    AssetType.Car => "ماشین",
                    AssetType.RealEstate => "املاک",
                    AssetType.Stock => "سهام",
                    AssetType.Bond => "اوراق قرضه",
                    AssetType.ETF => "صندوق‌های قابل معامله",
                    AssetType.Custom => "سفارشی",
                    AssetType.Unknown => "نامشخص",
                    _ => "نامشخص"
                };
            }
            
            // For custom types (with values >= 1000), we need to fetch from the API
            // But for display purposes, we'll just show "سفارشی" here
            // The actual name will be displayed in the API response when fetching asset types
            return "سفارشی";
        }
    }

    public class CreateAssetViewModel
    {
        [Required(ErrorMessage = "نام دارایی اجباری است")]
        [Display(Name = "نام دارایی")]
        [StringLength(100, ErrorMessage = "نام نباید بیش از 100 کاراکتر باشد")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "نماد دارایی اجباری است")]
        [Display(Name = "نماد دارایی")]
        [StringLength(10, ErrorMessage = "نماد نباید بیش از 10 کاراکتر باشد")]
        public string Symbol { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع دارایی اجباری است")]
        [Display(Name = "نوع دارایی")]
        public AssetType Type { get; set; }

        [Required(ErrorMessage = "مقدار اجباری است")]
        [Display(Name = "مقدار")]
        [Range(0.00000001, double.MaxValue, ErrorMessage = "مقدار باید بیشتر از صفر باشد")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "قیمت خرید اجباری است")]
        [Display(Name = "قیمت خرید")]
        [Range(0.01, double.MaxValue, ErrorMessage = "قیمت خرید باید بیشتر از صفر باشد")]
        public decimal PurchasePrice { get; set; }

        [Required(ErrorMessage = "قیمت فعلی اجباری است")]
        [Display(Name = "قیمت فعلی")]
        [Range(0.01, double.MaxValue, ErrorMessage = "قیمت فعلی باید بیشتر از صفر باشد")]
        public decimal CurrentPrice { get; set; }

        [Required(ErrorMessage = "تاریخ خرید اجباری است")]
        [Display(Name = "تاریخ خرید")]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; } = DateTime.Today;

        public int PortfolioId { get; set; }
    }

    public class UpdateAssetViewModel
    {
        public int Id { get; set; }
        public int PortfolioId { get; set; }
        [Required(ErrorMessage = "نام دارایی اجباری است")]
        [Display(Name = "نام دارایی")]
        [StringLength(100, ErrorMessage = "نام نباید بیش از 100 کاراکتر باشد")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "نماد دارایی اجباری است")]
        [Display(Name = "نماد دارایی")]
        [StringLength(10, ErrorMessage = "نماد نباید بیش از 10 کاراکتر باشد")]
        public string Symbol { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع دارایی اجباری است")]
        [Display(Name = "نوع دارایی")]
        public AssetType Type { get; set; }

        [Required(ErrorMessage = "مقدار اجباری است")]
        [Display(Name = "مقدار")]
        [Range(0.00000001, double.MaxValue, ErrorMessage = "مقدار باید بیشتر از صفر باشد")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "قیمت خرید اجباری است")]
        [Display(Name = "قیمت خرید")]
        [Range(0.01, double.MaxValue, ErrorMessage = "قیمت خرید باید بیشتر از صفر باشد")]
        public decimal PurchasePrice { get; set; }

        [Required(ErrorMessage = "قیمت فعلی اجباری است")]
        [Display(Name = "قیمت فعلی")]
        [Range(0.01, double.MaxValue, ErrorMessage = "قیمت فعلی باید بیشتر از صفر باشد")]
        public decimal CurrentPrice { get; set; }

        [Required(ErrorMessage = "تاریخ خرید اجباری است")]
        [Display(Name = "تاریخ خرید")]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; }
    }

    public class DashboardViewModel
    {
        public int PortfolioCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalInvestment { get; set; }
        public decimal TotalProfitLoss { get; set; }
        public decimal TotalProfitLossPercentage { get; set; }
        public List<AssetTypeDistribution> AssetTypeDistribution { get; set; } = new();
        public List<AssetViewModel> TopPerformingAssets { get; set; } = new();
        public List<PortfolioSummaryViewModel> PortfolioSummaries { get; set; } = new();
        public bool IsProfit => TotalProfitLoss >= 0;
    }

    public class AssetTypeDistribution
    {
        public string AssetType { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
        public int Count { get; set; }
    }

    public class PortfolioSummaryViewModel
    {
        public int Id { get; set; }
        public int PortfolioId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalInvestment { get; set; }
        public decimal TotalProfitLoss { get; set; }
        public decimal TotalProfitLossPercentage { get; set; }
        public int AssetCount { get; set; }
        public List<AssetViewModel> Assets { get; set; } = new();
        public List<AssetTypeDistribution> AssetTypeDistribution { get; set; } = new();
        public bool IsProfit => TotalProfitLoss >= 0;
    }

    public class PriceHistoryDto
    {
        public DateTime Timestamp { get; set; }
        public decimal Price { get; set; }
    }
} 