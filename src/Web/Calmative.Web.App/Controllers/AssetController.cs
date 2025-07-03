using Calmative.Web.App.Models.ViewModels;
using Calmative.Web.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace Calmative.Web.App.Controllers
{
    public class AssetController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AssetController> _logger;

        public AssetController(IApiService apiService, ILogger<AssetController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int portfolioId)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new CreateAssetViewModel
            {
                PortfolioId = portfolioId
            };

            await SetupAssetTypeSelectList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAssetViewModel model)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            // Log all form values for debugging
            _logger.LogInformation("Form data received:");
            foreach (var key in Request.Form.Keys)
            {
                _logger.LogInformation("  {Key}: {Value}", key, Request.Form[key]);
            }

            // Check if there's a binding error on the Type property
            if (ModelState["Type"] != null && ModelState["Type"]?.Errors.Any() == true)
            {
                _logger.LogWarning("Type binding errors: {Errors}", 
                    string.Join(", ", ModelState["Type"]?.Errors.Select(e => e.ErrorMessage) ?? Array.Empty<string>()));
                
                // Try to manually bind the asset type if it's a custom type
                if (Request.Form.ContainsKey("Type") && int.TryParse(Request.Form["Type"], out int typeValue))
                {
                    _logger.LogInformation("Manually binding Type value from form: {TypeValue}", typeValue);
                    
                    if (AssetTypeHelper.IsValidAssetType(typeValue))
                    {
                        model.Type = (AssetType)typeValue;
                        ModelState["Type"]?.Errors.Clear();
                        _logger.LogInformation("Successfully bound custom asset type: {TypeValue}", typeValue);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid asset type value: {TypeValue}", typeValue);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to parse Type from form data: {FormValue}", 
                        Request.Form.ContainsKey("Type") ? Request.Form["Type"].ToString() : "not present");
                }
            }

            _logger.LogInformation("Creating asset with Type: {Type} ({TypeInt})", model.Type, (int)model.Type);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Errors by property:");
                foreach (var key in ModelState.Keys)
                {
                    var modelStateEntry = ModelState[key];
                    if (modelStateEntry != null && modelStateEntry.Errors.Count > 0)
                    {
                        _logger.LogWarning("  {Property}: {Errors}", key, 
                            string.Join(", ", modelStateEntry.Errors.Select(e => e.ErrorMessage)));
                    }
                }
                
                await SetupAssetTypeSelectList();
                return View(model);
            }

            var success = await _apiService.CreateAsset(model.PortfolioId, model);

            if (success)
            {
                TempData["SuccessMessage"] = "دارایی با موفقیت اضافه شد!";
                return RedirectToAction("Details", "Portfolio", new { id = model.PortfolioId });
            }
            else
            {
                ModelState.AddModelError("", "خطا در اضافه کردن دارایی");
                await SetupAssetTypeSelectList();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, int portfolioId)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get portfolio to find the asset
            var portfolio = await _apiService.GetPortfolio(portfolioId);
            if (portfolio == null)
            {
                TempData["ErrorMessage"] = "پورتفولیو یافت نشد";
                return RedirectToAction("Index", "Portfolio");
            }

            var asset = portfolio.Assets.FirstOrDefault(a => a.Id == id);
            if (asset == null)
            {
                TempData["ErrorMessage"] = "دارایی یافت نشد";
                return RedirectToAction("Details", "Portfolio", new { id = portfolioId });
            }

            var model = new UpdateAssetViewModel
            {
                Id = id,
                PortfolioId = portfolioId,
                Name = asset.Name,
                Symbol = asset.Symbol,
                Type = asset.Type,
                Quantity = asset.Quantity,
                PurchasePrice = asset.PurchasePrice,
                CurrentPrice = asset.CurrentPrice,
                PurchaseDate = asset.PurchaseDate
            };

            ViewBag.AssetId = id;
            ViewBag.PortfolioId = portfolioId;
            await SetupAssetTypeSelectList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, int portfolioId, UpdateAssetViewModel model)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            // Log all form values for debugging
            _logger.LogInformation("Form data received for Edit:");
            foreach (var key in Request.Form.Keys)
            {
                _logger.LogInformation("  {Key}: {Value}", key, Request.Form[key]);
            }

            // Check if there's a binding error on the Type property
            if (ModelState["Type"] != null && ModelState["Type"]?.Errors.Any() == true)
            {
                _logger.LogWarning("Type binding errors: {Errors}", 
                    string.Join(", ", ModelState["Type"]?.Errors.Select(e => e.ErrorMessage) ?? Array.Empty<string>()));
                
                // Try to manually bind the asset type if it's a custom type
                if (Request.Form.ContainsKey("Type") && int.TryParse(Request.Form["Type"], out int typeValue))
                {
                    _logger.LogInformation("Manually binding Type value from form: {TypeValue}", typeValue);
                    
                    if (AssetTypeHelper.IsValidAssetType(typeValue))
                    {
                        model.Type = (AssetType)typeValue;
                        ModelState["Type"]?.Errors.Clear();
                        _logger.LogInformation("Successfully bound custom asset type: {TypeValue}", typeValue);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid asset type value: {TypeValue}", typeValue);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to parse Type from form data: {FormValue}", 
                        Request.Form.ContainsKey("Type") ? Request.Form["Type"].ToString() : "not present");
                }
            }

            _logger.LogInformation("Updating asset {Id} with Type: {Type} ({TypeInt})", id, model.Type, (int)model.Type);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid for Edit. Errors by property:");
                foreach (var key in ModelState.Keys)
                {
                    var modelStateEntry = ModelState[key];
                    if (modelStateEntry != null && modelStateEntry.Errors.Count > 0)
                    {
                        _logger.LogWarning("  {Property}: {Errors}", key, 
                            string.Join(", ", modelStateEntry.Errors.Select(e => e.ErrorMessage)));
                    }
                }
                
                ViewBag.AssetId = id;
                ViewBag.PortfolioId = portfolioId;
                await SetupAssetTypeSelectList();
                return View(model);
            }

            var success = await _apiService.UpdateAsset(portfolioId, id, model);

            if (success)
            {
                TempData["SuccessMessage"] = "دارایی با موفقیت ویرایش شد!";
                return RedirectToAction("Details", "Portfolio", new { id = portfolioId });
            }
            else
            {
                ModelState.AddModelError("", "خطا در ویرایش دارایی");
                ViewBag.AssetId = id;
                ViewBag.PortfolioId = portfolioId;
                await SetupAssetTypeSelectList();
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int portfolioId)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var success = await _apiService.DeleteAsset(id);

            if (success)
            {
                TempData["SuccessMessage"] = "دارایی با موفقیت حذف شد!";
            }
            else
            {
                TempData["ErrorMessage"] = "خطا در حذف دارایی";
            }

            return RedirectToAction("Details", "Portfolio", new { id = portfolioId });
        }

        private async Task SetupAssetTypeSelectList()
        {
            try
            {
                // Get asset types from API
                var assetTypesResponse = await _apiService.GetAsync<AssetTypesResponse>("asset/types");
                
                var selectListItems = new List<SelectListItem>();
                
                // Add built-in types
                foreach (var type in assetTypesResponse.BuiltInTypes)
                {
                    selectListItems.Add(new SelectListItem
                    {
                        Value = type.Id.ToString(),
                        Text = type.DisplayName
                    });
                }
                
                // Add custom types that are active
                foreach (var type in assetTypesResponse.CustomTypes.Where(t => t.IsActive))
                {
                    // Custom types will have ID offset by 1000 to avoid conflicts with built-in types
                    selectListItems.Add(new SelectListItem
                    {
                        Value = (type.Id + 1000).ToString(),
                        Text = type.DisplayName
                    });
                }
                
                ViewBag.AssetTypes = selectListItems;
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error fetching asset types");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception when fetching asset types");
                }
                
                // Fallback to hardcoded types if API call fails
                var assetTypes = new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "ارز" },
                    new SelectListItem { Value = "2", Text = "طلا" },
                    new SelectListItem { Value = "3", Text = "نقره" },
                    new SelectListItem { Value = "4", Text = "رمزارز" },
                    new SelectListItem { Value = "5", Text = "فلزات گرانبها" },
                    new SelectListItem { Value = "6", Text = "ماشین" },
                    new SelectListItem { Value = "7", Text = "املاک" },
                    new SelectListItem { Value = "8", Text = "سهام" },
                    new SelectListItem { Value = "9", Text = "اوراق قرضه" },
                    new SelectListItem { Value = "10", Text = "صندوق‌های قابل معامله" }
                };

                ViewBag.AssetTypes = assetTypes;
            }
        }

        private bool IsUserLoggedIn()
        {
            return Request.Cookies.ContainsKey("jwt_token") && 
                   !string.IsNullOrEmpty(Request.Cookies["jwt_token"]);
        }
    }

    public class AssetTypesResponse
    {
        public List<AssetTypeItem> BuiltInTypes { get; set; } = new();
        public List<AssetTypeItem> CustomTypes { get; set; } = new();
    }

    public class AssetTypeItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
} 