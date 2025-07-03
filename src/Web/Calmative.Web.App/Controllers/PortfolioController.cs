using Calmative.Web.App.Models.ViewModels;
using Calmative.Web.App.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calmative.Web.App.Controllers
{
    public class PortfolioController : Controller
    {
        private readonly IApiService _apiService;
        private readonly IAssetTypeService _assetTypeService;
        private readonly ILogger<PortfolioController> _logger;

        public PortfolioController(
            IApiService apiService, 
            IAssetTypeService assetTypeService,
            ILogger<PortfolioController> logger)
        {
            _apiService = apiService;
            _assetTypeService = assetTypeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var portfolios = await _apiService.GetPortfolios();

            if (portfolios != null)
            {
                // Populate asset type display names for all portfolios
                await PopulateAssetTypeDisplayNames(portfolios);
                return View(portfolios);
            }
            else
            {
                TempData["ErrorMessage"] = "خطا در دریافت لیست پورتفولیوها";
                return View(new List<PortfolioViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var portfolio = await _apiService.GetPortfolio(id);

            if (portfolio != null)
            {
                // Populate asset type display names
                await PopulateAssetTypeDisplayNames(new List<PortfolioViewModel> { portfolio });
                return View(portfolio);
            }
            else
            {
                TempData["ErrorMessage"] = "پورتفولیو یافت نشد";
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method to populate asset type display names
        private async Task PopulateAssetTypeDisplayNames(List<PortfolioViewModel> portfolios)
        {
            try
            {
                _logger.LogInformation("Populating asset type display names for {PortfolioCount} portfolios", portfolios.Count);
                
                foreach (var portfolio in portfolios)
                {
                    foreach (var asset in portfolio.Assets)
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating asset type display names");
                // If there's an error, we'll fall back to the default display names
                foreach (var portfolio in portfolios)
                {
                    foreach (var asset in portfolio.Assets)
                    {
                        // Use the static method as fallback
                        asset.TypeDisplayName = AssetViewModel.GetAssetTypeDisplayName(asset.Type);
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePortfolioViewModel model)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _apiService.CreatePortfolio(model);

            if (success)
            {
                TempData["SuccessMessage"] = "پورتفولیو با موفقیت ایجاد شد!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", "خطا در ایجاد پورتفولیو");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var portfolio = await _apiService.GetPortfolio(id);

            if (portfolio != null)
            {
                var model = new UpdatePortfolioViewModel
                {
                    Id = id,
                    Name = portfolio.Name,
                    Description = portfolio.Description
                };
                ViewBag.PortfolioId = id;
                return View(model);
            }
            else
            {
                TempData["ErrorMessage"] = "پورتفولیو یافت نشد";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdatePortfolioViewModel model)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PortfolioId = id;
                return View(model);
            }

            var success = await _apiService.UpdatePortfolio(id, model);

            if (success)
            {
                TempData["SuccessMessage"] = "پورتفولیو با موفقیت ویرایش شد!";
                return RedirectToAction(nameof(Details), new { id });
            }
            else
            {
                ModelState.AddModelError("", "خطا در ویرایش پورتفولیو");
                ViewBag.PortfolioId = id;
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var success = await _apiService.DeletePortfolio(id);

            if (success)
            {
                TempData["SuccessMessage"] = "پورتفولیو با موفقیت حذف شد!";
            }
            else
            {
                TempData["ErrorMessage"] = "خطا در حذف پورتفولیو";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetAssetPriceHistory(int id)
        {
            if (!IsUserLoggedIn())
            {
                return Unauthorized();
            }
            var history = await _apiService.GetAssetPriceHistory(id) ?? new List<PriceHistoryDto>();
            return Json(history);
        }

        private bool IsUserLoggedIn()
        {
            return Request.Cookies.ContainsKey("jwt_token") && 
                   !string.IsNullOrEmpty(Request.Cookies["jwt_token"]);
        }
    }
} 