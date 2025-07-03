using Calmative.Server.API.Data;
using Calmative.Server.API.DTOs;
using Calmative.Server.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calmative.Server.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.CreatedAt,
                        u.IsEmailConfirmed
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users list");
                return StatusCode(500, new { message = "An error occurred while retrieving users." });
            }
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == id)
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.CreatedAt,
                        u.IsEmailConfirmed
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving user details." });
            }
        }

        [HttpGet("users/{id}/portfolios")]
        public async Task<IActionResult> GetUserPortfolios(int id)
        {
            try
            {
                var portfolios = await _context.Portfolios
                    .Where(p => p.UserId == id)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.CreatedAt,
                        p.UpdatedAt,
                        AssetsCount = p.Assets.Count,
                        TotalValue = p.Assets.Sum(a => a.Quantity * a.CurrentPrice)
                    })
                    .ToListAsync();

                return Ok(portfolios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolios for user ID {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving user portfolios." });
            }
        }

        [HttpGet("users/{id}/activities")]
        public async Task<IActionResult> GetUserActivities(int id)
        {
            try
            {
                // Gathering user activities from different sources
                
                // 1. Get portfolio creations
                var portfolioCreations = await _context.Portfolios
                    .Where(p => p.UserId == id)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .Select(p => new
                    {
                        Type = "PortfolioCreated",
                        Description = $"Created portfolio '{p.Name}'",
                        Timestamp = p.CreatedAt,
                        EntityId = p.Id,
                        EntityType = "Portfolio"
                    })
                    .ToListAsync();

                // 2. Get asset additions
                var assetAdditions = await _context.Assets
                    .Where(a => a.Portfolio.UserId == id)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .Select(a => new
                    {
                        Type = "AssetAdded",
                        Description = $"Added {a.Quantity} {a.Symbol} to portfolio '{a.Portfolio.Name}'",
                        Timestamp = a.CreatedAt,
                        EntityId = a.Id,
                        EntityType = "Asset"
                    })
                    .ToListAsync();

                // Combine and sort activities
                var activities = portfolioCreations
                    .Concat(assetAdditions)
                    .OrderByDescending(a => a.Timestamp)
                    .Take(10)
                    .ToList();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activities for user ID {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving user activities." });
            }
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                
                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                // Delete all portfolios associated with this user
                var portfolios = await _context.Portfolios.Where(p => p.UserId == id).ToListAsync();
                
                foreach (var portfolio in portfolios)
                {
                    // Assets will be automatically deleted due to cascade delete
                    _context.Portfolios.Remove(portfolio);
                }

                // Delete the user
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User with ID {UserId} successfully deleted", id);
                return Ok(new { message = $"User with ID {id} was successfully deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the user." });
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalPortfolios = await _context.Portfolios.CountAsync();
                var totalAssets = await _context.Assets.CountAsync();
                var totalValue = await _context.Assets.SumAsync(a => a.Quantity * a.CurrentPrice);
                
                var recentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.CreatedAt,
                        u.IsEmailConfirmed
                    })
                    .ToListAsync();

                var result = new
                {
                    TotalUsers = totalUsers,
                    TotalPortfolios = totalPortfolios,
                    TotalAssets = totalAssets,
                    TotalValue = totalValue,
                    RecentUsers = recentUsers
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin dashboard data");
                return StatusCode(500, new { message = "An error occurred while retrieving dashboard data." });
            }
        }
    }
} 