using Calmative.Server.API.DTOs;
using Calmative.Server.API.Models;

namespace Calmative.Server.API.Services
{
    public interface IRecommendationService
    {
        /// <summary>
        /// Get investment recommendations for a specific user
        /// </summary>
        Task<RecommendationDto> GetRecommendationsForUserAsync(int userId);
        
        /// <summary>
        /// Get investment recommendations for a specific portfolio
        /// </summary>
        Task<RecommendationDto> GetRecommendationsForPortfolioAsync(int portfolioId, int userId);
    }
} 