using System.ComponentModel.DataAnnotations;
using Calmative.Server.API.Models;

namespace Calmative.Server.API.DTOs
{
    public class UpdateAssetDto
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

        [Range(0.01, double.MaxValue)]
        public decimal CurrentPrice { get; set; }

        public DateTime PurchaseDate { get; set; }
    }
} 