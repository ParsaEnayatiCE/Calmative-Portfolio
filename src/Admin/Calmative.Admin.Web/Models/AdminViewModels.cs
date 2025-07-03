namespace Calmative.Admin.Web.Models
{
    public class LoginViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }

    public class UsersListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public int TotalUsers { get; set; }
    }

    public class UserPortfolioViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int AssetsCount { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class UserActivityViewModel
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
    }

    public class UserDetailsViewModel
    {
        public UserViewModel User { get; set; } = new UserViewModel();
        public List<UserPortfolioViewModel> Portfolios { get; set; } = new List<UserPortfolioViewModel>();
        public List<UserActivityViewModel> Activities { get; set; } = new List<UserActivityViewModel>();
    }

    public class PortfolioViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int AssetCount { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPortfolios { get; set; }
        public int TotalAssets { get; set; }
        public decimal TotalValue { get; set; }
        public List<UserViewModel> RecentUsers { get; set; } = new List<UserViewModel>();
    }
} 