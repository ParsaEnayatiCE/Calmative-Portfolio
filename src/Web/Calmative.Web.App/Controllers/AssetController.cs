using Calmative.Web.App.Models.ViewModels;
using Calmative.Web.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Calmative.Web.App.Controllers
{
    public class AssetController : Controller
    {
        private readonly IApiService _apiService;

        public AssetController(IApiService apiService)
        {
            _apiService = apiService;
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

            if (!ModelState.IsValid)
            {
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

            if (!ModelState.IsValid)
            {
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

        private Task SetupAssetTypeSelectList()
        {
            var assetTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "ارز" },
                new SelectListItem { Value = "2", Text = "طلا" },
                new SelectListItem { Value = "3", Text = "نقره" },
                new SelectListItem { Value = "4", Text = "رمزارز" },
                new SelectListItem { Value = "5", Text = "فلزات گرانبها" },
                new SelectListItem { Value = "6", Text = "ماشین" }
            };

            ViewBag.AssetTypes = assetTypes;
            return Task.CompletedTask;
        }

        private bool IsUserLoggedIn()
        {
            return Request.Cookies.ContainsKey("jwt_token") && 
                   !string.IsNullOrEmpty(Request.Cookies["jwt_token"]);
        }
    }
} 