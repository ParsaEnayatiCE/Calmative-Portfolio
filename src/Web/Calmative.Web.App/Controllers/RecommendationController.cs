using Calmative.Web.App.Models.ViewModels;
using Calmative.Web.App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Calmative.Web.App.Controllers
{
    public class RecommendationController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(IApiService apiService, ILogger<RecommendationController> logger)
        {
            _apiService = apiService;
            _logger = logger;
            _logger.LogInformation("RecommendationController initialized");
        }

        /// <summary>
        /// Debug endpoint to test controller accessibility
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [Route("Recommendation/Debug")]
        public IActionResult Debug()
        {
            _logger.LogWarning("DEBUG: Debug action called at {Time}", DateTime.Now);
            return Content("RecommendationController is accessible. Debug endpoint working correctly.");
        }

        /// <summary>
        /// Display recommendations based on all user portfolios
        /// </summary>
        [HttpGet]
        [Route("Recommendation/Index")]
        [Route("Recommendation")]
        public async Task<IActionResult> Index()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogWarning("DEBUG: Recommendation Index action called at {Time}", DateTime.Now);
            
            try
            {
                // Check if JWT token exists
                var token = Request.Cookies["jwt_token"];
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("DEBUG: No JWT token found in cookies, redirecting to login");
                    return RedirectToAction("Login", "Auth");
                }
                
                _logger.LogWarning("DEBUG: JWT token found, length: {Length}", token.Length);
                _logger.LogWarning("DEBUG: About to call GetUserRecommendations API service");
                var recommendations = await _apiService.GetUserRecommendations();
                _logger.LogWarning("DEBUG: GetUserRecommendations returned {Result}", recommendations != null ? "data" : "null");
                
                if (recommendations == null)
                {
                    _logger.LogWarning("DEBUG: No recommendations returned from API");
                    TempData["ErrorMessage"] = "خطا در دریافت توصیه‌های هوشمند. لطفا مجددا تلاش کنید.";
                    return RedirectToAction("Index", "Dashboard");
                }
                
                stopwatch.Stop();
                _logger.LogWarning("DEBUG: Recommendations retrieved successfully in {ElapsedMs}ms, rendering view", stopwatch.ElapsedMilliseconds);
                return View(recommendations);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "DEBUG: Error getting user recommendations after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                _logger.LogWarning("DEBUG: Exception details: {Message}, {StackTrace}", ex.Message, ex.StackTrace);
                
                if (ex.InnerException != null)
                {
                    _logger.LogWarning("DEBUG: Inner exception: {Message}, {StackTrace}", 
                        ex.InnerException.Message, ex.InnerException.StackTrace);
                }
                
                TempData["ErrorMessage"] = "خطا در دریافت توصیه‌های هوشمند. لطفا مجددا تلاش کنید.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// Display recommendations for a specific portfolio
        /// </summary>
        [HttpGet]
        [Route("Recommendation/Portfolio/{portfolioId}")]
        public async Task<IActionResult> Portfolio(int portfolioId)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogWarning("DEBUG: Portfolio recommendation action called for portfolioId={PortfolioId} at {Time}", 
                portfolioId, DateTime.Now);
            
            try
            {
                // Check if JWT token exists
                var token = Request.Cookies["jwt_token"];
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("DEBUG: No JWT token found in cookies, redirecting to login");
                    return RedirectToAction("Login", "Auth");
                }
                
                _logger.LogWarning("DEBUG: JWT token found, length: {Length}", token.Length);
                _logger.LogWarning("DEBUG: About to call GetPortfolioRecommendations API service");
                var recommendations = await _apiService.GetPortfolioRecommendations(portfolioId);
                _logger.LogWarning("DEBUG: GetPortfolioRecommendations returned {Result}", recommendations != null ? "data" : "null");
                
                if (recommendations == null)
                {
                    _logger.LogWarning("DEBUG: No portfolio recommendations returned from API");
                    TempData["ErrorMessage"] = "خطا در دریافت توصیه‌های هوشمند. لطفا مجددا تلاش کنید.";
                    return RedirectToAction("Details", "Portfolio", new { id = portfolioId });
                }
                
                stopwatch.Stop();
                _logger.LogWarning("DEBUG: Portfolio recommendations retrieved successfully in {ElapsedMs}ms, rendering view", 
                    stopwatch.ElapsedMilliseconds);
                ViewBag.PortfolioId = portfolioId;
                return View("Index", recommendations);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "DEBUG: Error getting portfolio recommendations after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                _logger.LogWarning("DEBUG: Exception details: {Message}, {StackTrace}", ex.Message, ex.StackTrace);
                
                if (ex.InnerException != null)
                {
                    _logger.LogWarning("DEBUG: Inner exception: {Message}, {StackTrace}", 
                        ex.InnerException.Message, ex.InnerException.StackTrace);
                }
                
                TempData["ErrorMessage"] = "خطا در دریافت توصیه‌های هوشمند. لطفا مجددا تلاش کنید.";
                return RedirectToAction("Details", "Portfolio", new { id = portfolioId });
            }
        }
    }
} 