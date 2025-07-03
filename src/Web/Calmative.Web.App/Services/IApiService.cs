using Calmative.Web.App.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calmative.Web.App.Services
{
    public interface IApiService
    {
        // Auth
        Task<(bool Success, string? Message)> Register(RegisterViewModel model);
        Task<(bool Success, string? Message)> Login(LoginViewModel model);
        Task<(bool Success, string? Message)> ChangePassword(ChangePasswordViewModel model);
        Task<(bool Success, string? Message)> RequestPasswordReset(string email);
        Task<(bool Success, string? Message)> ResetPassword(ResetPasswordViewModel model);
        Task<UserViewModel?> GetUserProfile();
        Task<(bool Success, string? Message)> ConfirmEmail(string userId, string token);

        // Portfolio
        Task<List<PortfolioViewModel>?> GetPortfolios();
        Task<PortfolioViewModel?> GetPortfolio(int id);
        Task<bool> CreatePortfolio(CreatePortfolioViewModel model);
        Task<bool> UpdatePortfolio(int id, UpdatePortfolioViewModel model);
        Task<bool> DeletePortfolio(int id);

        // Asset
        Task<bool> CreateAsset(int portfolioId, CreateAssetViewModel model);
        Task<bool> UpdateAsset(int portfolioId, int assetId, UpdateAssetViewModel model);
        Task<bool> DeleteAsset(int assetId);

        // Dashboard
        Task<DashboardViewModel?> GetDashboard();
        Task<List<PriceHistoryDto>?> GetAssetPriceHistory(int assetId);
        
        // Generic API calls
        Task<T> GetAsync<T>(string endpoint);
    }
} 