using Calmative.Web.App.Models.ViewModels;
using Calmative.Web.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Calmative.Web.App.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IApiService _apiService;
        private readonly IAssetTypeService _assetTypeService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IApiService apiService, 
            IAssetTypeService assetTypeService,
            ILogger<DashboardController> logger)
        {
            _apiService = apiService;
            _assetTypeService = assetTypeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsUserLoggedIn())
            {
                _logger.LogWarning("User not logged in, redirecting to login");
                return RedirectToAction("Login", "Auth");
            }

            _logger.LogInformation("User is logged in, fetching dashboard data");
            var dashboard = await _apiService.GetDashboard();

            if (dashboard != null)
            {
                _logger.LogInformation("Dashboard data retrieved successfully");
                
                // Populate asset type display names for top performing assets
                await PopulateAssetTypeDisplayNames(dashboard.TopPerformingAssets);
                
                // Update asset type distribution labels
                await UpdateAssetTypeDistributionLabels(dashboard.AssetTypeDistribution);
                
                // Populate asset type names in portfolio summaries
                foreach (var summary in dashboard.PortfolioSummaries)
                {
                    await PopulateAssetTypeDisplayNames(summary.Assets);
                    await UpdateAssetTypeDistributionLabels(summary.AssetTypeDistribution);
                }
                
                return View(dashboard);
            }
            else
            {
                _logger.LogError("Failed to get dashboard data");
                TempData["ErrorMessage"] = "خطا در دریافت اطلاعات داشبورد";
                return View();
            }
        }
        
        // Helper method to populate asset type display names
        private async Task PopulateAssetTypeDisplayNames(List<AssetViewModel> assets)
        {
            try
            {
                _logger.LogInformation("Populating asset type display names for {AssetCount} assets", assets.Count);
                
                foreach (var asset in assets)
                {
                    int typeId = (int)asset.Type;
                    
                    // Handle custom types (>= 1000)
                    if (typeId >= 1000)
                    {
                        _logger.LogInformation("Found custom asset type: {TypeId} for asset {AssetName}", typeId, asset.Name);
                    }
                    
                    // Get display name from service
                    asset.TypeDisplayName = await _assetTypeService.GetAssetTypeDisplayName(asset.Type);
                    _logger.LogInformation("Asset {AssetId} ({AssetName}) type {TypeId} display name: {DisplayName}", 
                        asset.Id, asset.Name, typeId, asset.TypeDisplayName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating asset type display names");
                // If there's an error, we'll fall back to the default display names
                foreach (var asset in assets)
                {
                    // Use the static method as fallback
                    asset.TypeDisplayName = AssetViewModel.GetAssetTypeDisplayName(asset.Type);
                }
            }
        }
        
        // Helper method to update asset type distribution labels
        private async Task UpdateAssetTypeDistributionLabels(List<AssetTypeDistribution> distributions)
        {
            try
            {
                var allTypes = await _assetTypeService.GetAllAssetTypeDisplayNames();
                
                foreach (var distribution in distributions)
                {
                    // Try to extract the type ID from the AssetType string
                    if (int.TryParse(distribution.AssetType, out int typeId))
                    {
                        if (allTypes.TryGetValue(typeId, out string? displayName) && !string.IsNullOrEmpty(displayName))
                        {
                            distribution.AssetType = displayName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating asset type distribution labels");
                // If there's an error, we'll keep the original labels
            }
        }

        private bool IsUserLoggedIn()
        {
            var hasToken = Request.Cookies.ContainsKey("jwt_token") && 
                           !string.IsNullOrEmpty(Request.Cookies["jwt_token"]);
            
            _logger.LogInformation("User login check: HasToken={HasToken}", hasToken);
            if (hasToken)
            {
                var tokenValue = Request.Cookies["jwt_token"];
                _logger.LogInformation("JWT Token exists, length: {TokenLength}", tokenValue?.Length ?? 0);
            }
            
            return hasToken;
        }
    }
} 