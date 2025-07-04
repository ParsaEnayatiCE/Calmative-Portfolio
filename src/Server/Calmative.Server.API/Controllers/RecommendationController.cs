using Calmative.Server.API.DTOs;
using Calmative.Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Calmative.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(
            IRecommendationService recommendationService,
            ILogger<RecommendationController> logger)
        {
            _recommendationService = recommendationService;
            _logger = logger;
        }

        /// <summary>
        /// Debug endpoint to test the controller without authentication
        /// </summary>
        [HttpGet("debug")]
        [AllowAnonymous]
        public ActionResult<object> Debug()
        {
            _logger.LogWarning("DEBUG: RecommendationController debug endpoint called at {Time}", DateTime.UtcNow);
            
            return Ok(new { 
                message = "RecommendationController is working", 
                timestamp = DateTime.UtcNow,
                endpoints = new[] { 
                    "/api/recommendation/user", 
                    "/api/recommendation/portfolio/{portfolioId}" 
                }
            });
        }

        /// <summary>
        /// Get recommendations for all user portfolios combined
        /// </summary>
        [HttpGet("user")]
        public async Task<ActionResult<RecommendationDto>> GetUserRecommendations()
        {
            _logger.LogWarning("DEBUG: GetUserRecommendations endpoint called at {Time}", DateTime.UtcNow);
            
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("DEBUG: Unauthorized - no user ID found in claims");
                return Unauthorized();
            }

            _logger.LogWarning("DEBUG: Processing recommendations for user {UserId}", userId.Value);
            
            try
            {
                var recommendations = await _recommendationService.GetRecommendationsForUserAsync(userId.Value);
                _logger.LogWarning("DEBUG: Successfully retrieved recommendations for user {UserId}", userId.Value);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DEBUG: Error getting user recommendations");
                _logger.LogWarning("DEBUG: Exception details: {Message}, {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, "خطا در دریافت توصیه‌ها. لطفا مجددا تلاش کنید.");
            }
        }

        /// <summary>
        /// Get recommendations for a specific portfolio
        /// </summary>
        [HttpGet("portfolio/{portfolioId}")]
        public async Task<ActionResult<RecommendationDto>> GetPortfolioRecommendations(int portfolioId)
        {
            _logger.LogWarning("DEBUG: GetPortfolioRecommendations endpoint called for portfolioId={PortfolioId} at {Time}", 
                portfolioId, DateTime.UtcNow);
            
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("DEBUG: Unauthorized - no user ID found in claims");
                return Unauthorized();
            }

            _logger.LogWarning("DEBUG: Processing recommendations for portfolio {PortfolioId}, user {UserId}", 
                portfolioId, userId.Value);
            
            try
            {
                var recommendations = await _recommendationService.GetRecommendationsForPortfolioAsync(portfolioId, userId.Value);
                _logger.LogWarning("DEBUG: Successfully retrieved recommendations for portfolio {PortfolioId}", portfolioId);
                return Ok(recommendations);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("DEBUG: Portfolio not found: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DEBUG: Error getting portfolio recommendations for portfolio {PortfolioId}", portfolioId);
                _logger.LogWarning("DEBUG: Exception details: {Message}, {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, "خطا در دریافت توصیه‌ها. لطفا مجددا تلاش کنید.");
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
} 