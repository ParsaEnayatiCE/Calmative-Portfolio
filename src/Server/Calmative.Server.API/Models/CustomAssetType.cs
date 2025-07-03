using System.ComponentModel.DataAnnotations;

namespace Calmative.Server.API.Models
{
    public class CustomAssetType
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string DisplayName { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 