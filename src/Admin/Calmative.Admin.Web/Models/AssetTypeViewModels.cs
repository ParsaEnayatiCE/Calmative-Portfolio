using System.ComponentModel.DataAnnotations;

namespace Calmative.Admin.Web.Models
{
    public class AssetTypeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsBuiltIn { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AssetTypesListViewModel
    {
        public List<AssetTypeViewModel> BuiltInTypes { get; set; } = new();
        public List<AssetTypeViewModel> CustomTypes { get; set; } = new();
    }

    public class CreateAssetTypeViewModel
    {
        [Required(ErrorMessage = "نام اجباری است")]
        [Display(Name = "نام (انگلیسی)")]
        [StringLength(50, ErrorMessage = "نام نباید بیش از 50 کاراکتر باشد", MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "نام باید فقط شامل حروف انگلیسی، اعداد و _ باشد")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "نام نمایشی اجباری است")]
        [Display(Name = "نام نمایشی (فارسی)")]
        [StringLength(50, ErrorMessage = "نام نمایشی نباید بیش از 50 کاراکتر باشد", MinimumLength = 2)]
        public string DisplayName { get; set; } = string.Empty;
        
        [Display(Name = "توضیحات")]
        [StringLength(200, ErrorMessage = "توضیحات نباید بیش از 200 کاراکتر باشد")]
        public string? Description { get; set; }
        
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
    }

    public class UpdateAssetTypeViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "نام اجباری است")]
        [Display(Name = "نام (انگلیسی)")]
        [StringLength(50, ErrorMessage = "نام نباید بیش از 50 کاراکتر باشد", MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "نام باید فقط شامل حروف انگلیسی، اعداد و _ باشد")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "نام نمایشی اجباری است")]
        [Display(Name = "نام نمایشی (فارسی)")]
        [StringLength(50, ErrorMessage = "نام نمایشی نباید بیش از 50 کاراکتر باشد", MinimumLength = 2)]
        public string DisplayName { get; set; } = string.Empty;
        
        [Display(Name = "توضیحات")]
        [StringLength(200, ErrorMessage = "توضیحات نباید بیش از 200 کاراکتر باشد")]
        public string? Description { get; set; }
        
        [Display(Name = "فعال")]
        public bool IsActive { get; set; }
    }
} 