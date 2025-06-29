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
    public class DashboardController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;
        private readonly IAssetService _assetService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IPortfolioService portfolioService, IAssetService assetService, ILogger<DashboardController> logger)
        {
            _portfolioService = portfolioService;
            _assetService = assetService;
            _logger = logger;
        }

        [HttpGet("overview")]
        public async Task<ActionResult<DashboardOverviewDto>> GetDashboardOverview()
        {
            try
            {
                _logger.LogInformation("Dashboard overview requested");
                
                // Log all claims
                var claims = User.Claims.ToList();
                _logger.LogInformation("User has {ClaimCount} claims", claims.Count);
                foreach (var claim in claims)
                {
                    _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                }
                
                var userId = GetCurrentUserId();
                _logger.LogInformation("Extracted UserId: {UserId}", userId);
                
                if (userId == null)
                {
                    _logger.LogWarning("No valid userId found in token");
                    return Unauthorized();
                }

                var portfolios = await _portfolioService.GetUserPortfoliosAsync(userId.Value);
                _logger.LogInformation("Found {PortfolioCount} portfolios for user {UserId}", portfolios.Count(), userId);
                
                var portfolioSummaries = new List<PortfolioSummaryDto>();

                foreach (var portfolio in portfolios)
                {
                    try
                    {
                        var summary = await _portfolioService.GetPortfolioSummaryAsync(portfolio.Id, userId.Value);
                        portfolioSummaries.Add(summary);
                    }
                    catch (NotFoundException)
                    {
                        // Skip if portfolio not found
                    }
                }

                var totalValue = portfolioSummaries.Sum(p => p.TotalValue);
                var totalInvestment = portfolioSummaries.Sum(p => p.TotalInvestment);
                var totalProfitLoss = totalValue - totalInvestment;
                var totalProfitLossPercentage = totalInvestment > 0 ? (totalProfitLoss / totalInvestment) * 100 : 0;

                // Asset type distribution across all portfolios
                var assetTypeDistribution = portfolioSummaries
                    .SelectMany(p => p.AssetTypeDistribution)
                    .GroupBy(d => d.AssetType)
                    .Select(g => new AssetTypeDistribution
                    {
                        AssetType = g.Key,
                        Value = g.Sum(d => d.Value),
                        Percentage = totalValue > 0 ? (g.Sum(d => d.Value) / totalValue) * 100 : 0,
                        Count = g.Sum(d => d.Count)
                    })
                    .OrderByDescending(d => d.Value)
                    .ToList();

                // Top performing assets across all portfolios
                var topPerformingAssets = portfolioSummaries
                    .SelectMany(p => p.TopPerformingAssets)
                    .OrderByDescending(a => a.ProfitLossPercentage)
                    .Take(10)
                    .ToList();

                var result = new DashboardOverviewDto
                {
                    PortfolioCount = portfolios.Count(),
                    TotalValue = totalValue,
                    TotalInvestment = totalInvestment,
                    TotalProfitLoss = totalProfitLoss,
                    TotalProfitLossPercentage = totalProfitLossPercentage,
                    AssetTypeDistribution = assetTypeDistribution,
                    TopPerformingAssets = topPerformingAssets,
                    PortfolioSummaries = portfolioSummaries.Take(5).ToList()
                };

                _logger.LogInformation("Dashboard overview completed successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting dashboard overview");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("performance")]
        public async Task<ActionResult<PerformanceDto>> GetPerformanceData()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var portfolios = await _portfolioService.GetUserPortfoliosAsync(userId.Value);
            var allAssets = new List<AssetDto>();

            foreach (var portfolio in portfolios)
            {
                allAssets.AddRange(portfolio.Assets);
            }

            var performance = new PerformanceDto
            {
                BestPerformers = allAssets
                    .OrderByDescending(a => a.ProfitLossPercentage)
                    .Take(10)
                    .ToList(),
                WorstPerformers = allAssets
                    .OrderBy(a => a.ProfitLossPercentage)
                    .Take(10)
                    .ToList(),
                RecentPurchases = allAssets
                    .OrderByDescending(a => a.PurchaseDate)
                    .Take(10)
                    .ToList()
            };

            return Ok(performance);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
} 