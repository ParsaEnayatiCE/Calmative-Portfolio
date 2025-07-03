using System.ComponentModel.DataAnnotations;

namespace Calmative.Server.API.DTOs
{
    public class CustomAssetTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCustomAssetTypeDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string DisplayName { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCustomAssetTypeDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string DisplayName { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; }
    }
} 