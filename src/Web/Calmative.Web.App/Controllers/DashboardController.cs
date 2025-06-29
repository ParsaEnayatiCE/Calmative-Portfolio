using Calmative.Web.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Calmative.Web.App.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IApiService apiService, ILogger<DashboardController> logger)
        {
            _apiService = apiService;
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
                return View(dashboard);
            }
            else
            {
                _logger.LogError("Failed to get dashboard data");
                TempData["ErrorMessage"] = "خطا در دریافت اطلاعات داشبورد";
                return View();
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