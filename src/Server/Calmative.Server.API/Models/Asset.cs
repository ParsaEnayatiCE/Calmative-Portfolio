using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calmative.Server.API.Models
{
    public class Asset
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(10)]
        public string Symbol { get; set; } = string.Empty;
        
        public AssetType Type { get; set; }
        
        [Column(TypeName = "decimal(18,8)")]
        public decimal Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentPrice { get; set; }
        
        public DateTime PurchaseDate { get; set; }
        
        public int PortfolioId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Calculated properties
        [NotMapped]
        public decimal TotalValue => Quantity * CurrentPrice;
        
        [NotMapped]
        public decimal ProfitLoss => TotalValue - (Quantity * PurchasePrice);
        
        [NotMapped]
        public decimal ProfitLossPercentage => 
            PurchasePrice > 0 ? ((CurrentPrice - PurchasePrice) / PurchasePrice) * 100 : 0;
        
        // Navigation properties
        public virtual Portfolio Portfolio { get; set; } = null!;
    }
} 