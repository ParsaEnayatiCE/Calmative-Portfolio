using Calmative.Web.App.Models.ViewModels;
using Calmative.Web.App.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calmative.Web.App.Controllers
{
    public class PortfolioController : Controller
    {
        private readonly IApiService _apiService;

        public PortfolioController(IApiService apiService)
        {
            _apiService = apiService;
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
                return View(portfolio);
            }
            else
            {
                TempData["ErrorMessage"] = "پورتفولیو یافت نشد";
                return RedirectToAction(nameof(Index));
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