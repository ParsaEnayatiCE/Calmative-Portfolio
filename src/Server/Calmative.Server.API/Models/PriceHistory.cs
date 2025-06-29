using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calmative.Server.API.Models
{
    public class PriceHistory
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(10)]
        public string Symbol { get; set; } = string.Empty;
        
        public AssetType AssetType { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [MaxLength(50)]
        public string? Source { get; set; }
    }
} 