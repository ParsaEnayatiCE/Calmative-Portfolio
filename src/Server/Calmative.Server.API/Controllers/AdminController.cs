using AutoMapper;
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
        private readonly IMapper _mapper;

        public AdminController(
            ApplicationDbContext context, 
            ILogger<AdminController> logger,
            IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
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
                _logger.LogError(ex, "Error retrieving portfolios for user with ID {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving user portfolios." });
            }
        }

        [HttpGet("users/{id}/activities")]
        public async Task<IActionResult> GetUserActivities(int id)
        {
            try
            {
                // Check if user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == id);
                if (!userExists)
                {
                    return NotFound(new { message = $"User with ID {id} not found." });
                }

                // Get user registration
                var user = await _context.Users
                    .Where(u => u.Id == id)
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.CreatedAt
                    })
                    .FirstAsync();

                // Get portfolio creations
                var portfolioCreations = await _context.Portfolios
                    .Where(p => p.UserId == id)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new
                    {
                        ActivityType = "PortfolioCreation",
                        p.Id,
                        p.Name,
                        p.CreatedAt,
                        Description = $"Created portfolio: {p.Name}"
                    })
                    .ToListAsync();

                // Get asset additions
                var assetAdditions = await _context.Assets
                    .Where(a => a.Portfolio.UserId == id)
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => new
                    {
                        ActivityType = "AssetAddition",
                        a.Id,
                        a.Name,
                        a.CreatedAt,
                        PortfolioId = a.Portfolio.Id,
                        PortfolioName = a.Portfolio.Name,
                        Description = $"Added asset: {a.Name} to portfolio: {a.Portfolio.Name}"
                    })
                    .ToListAsync();

                // Combine all activities
                var activities = new List<object>
                {
                    new
                    {
                        ActivityType = "UserRegistration",
                        Id = user.Id,
                        Name = $"{user.FirstName} {user.LastName}",
                        user.CreatedAt,
                        Description = $"User registered with email: {user.Email}"
                    }
                };

                activities.AddRange(portfolioCreations);
                activities.AddRange(assetAdditions);

                // Sort by creation date descending
                var sortedActivities = activities
                    .OrderByDescending(a => ((dynamic)a).CreatedAt)
                    .ToList();

                return Ok(sortedActivities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activities for user with ID {UserId}", id);
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

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User with ID {UserId} successfully deleted", id);
                return Ok(new { message = $"User with ID {id} successfully deleted." });
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

        #region Asset Types Management

        [HttpGet("asset-types")]
        public async Task<IActionResult> GetAssetTypes()
        {
            try
            {
                // Get all built-in asset types from enum
                var builtInAssetTypes = Enum.GetValues(typeof(AssetType))
                    .Cast<AssetType>()
                    .Select(t => new
                    {
                        Id = (int)t,
                        Name = t.ToString(),
                        DisplayName = GetAssetTypeDisplayName(t),
                        IsBuiltIn = true
                    })
                    .ToList();

                // Get all custom asset types
                var customAssetTypes = await _context.CustomAssetTypes
                    .Select(t => new
                    {
                        t.Id,
                        t.Name,
                        t.DisplayName,
                        t.Description,
                        t.IsActive,
                        t.CreatedAt,
                        t.UpdatedAt,
                        IsBuiltIn = false
                    })
                    .ToListAsync();

                var result = new
                {
                    BuiltInTypes = builtInAssetTypes,
                    CustomTypes = customAssetTypes
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving asset types");
                return StatusCode(500, new { message = "An error occurred while retrieving asset types." });
            }
        }

        [HttpGet("asset-types/custom/{id}")]
        public async Task<IActionResult> GetCustomAssetType(int id)
        {
            try
            {
                var assetType = await _context.CustomAssetTypes.FindAsync(id);
                if (assetType == null)
                {
                    return NotFound(new { message = $"Custom asset type with ID {id} not found." });
                }

                var result = _mapper.Map<CustomAssetTypeDto>(assetType);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving custom asset type with ID {AssetTypeId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the custom asset type." });
            }
        }

        [HttpPost("asset-types/custom")]
        public async Task<IActionResult> CreateCustomAssetType([FromBody] CreateCustomAssetTypeDto dto)
        {
            try
            {
                // Check if name already exists
                var nameExists = await _context.CustomAssetTypes.AnyAsync(t => t.Name == dto.Name);
                if (nameExists)
                {
                    return BadRequest(new { message = $"An asset type with the name '{dto.Name}' already exists." });
                }

                var assetType = _mapper.Map<CustomAssetType>(dto);
                assetType.CreatedAt = DateTime.UtcNow;

                _context.CustomAssetTypes.Add(assetType);
                await _context.SaveChangesAsync();

                var result = _mapper.Map<CustomAssetTypeDto>(assetType);
                _logger.LogInformation("Custom asset type created: {AssetTypeName}", assetType.Name);

                return CreatedAtAction(nameof(GetCustomAssetType), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating custom asset type");
                return StatusCode(500, new { message = "An error occurred while creating the custom asset type." });
            }
        }

        [HttpPut("asset-types/custom/{id}")]
        public async Task<IActionResult> UpdateCustomAssetType(int id, [FromBody] UpdateCustomAssetTypeDto dto)
        {
            try
            {
                var assetType = await _context.CustomAssetTypes.FindAsync(id);
                if (assetType == null)
                {
                    return NotFound(new { message = $"Custom asset type with ID {id} not found." });
                }

                // Check if name already exists (excluding this asset type)
                var nameExists = await _context.CustomAssetTypes
                    .AnyAsync(t => t.Name == dto.Name && t.Id != id);
                    
                if (nameExists)
                {
                    return BadRequest(new { message = $"Another asset type with the name '{dto.Name}' already exists." });
                }

                _mapper.Map(dto, assetType);
                assetType.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var result = _mapper.Map<CustomAssetTypeDto>(assetType);
                _logger.LogInformation("Custom asset type updated: {AssetTypeName}", assetType.Name);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating custom asset type with ID {AssetTypeId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the custom asset type." });
            }
        }

        [HttpDelete("asset-types/custom/{id}")]
        public async Task<IActionResult> DeleteCustomAssetType(int id)
        {
            try
            {
                var assetType = await _context.CustomAssetTypes.FindAsync(id);
                if (assetType == null)
                {
                    return NotFound(new { message = $"Custom asset type with ID {id} not found." });
                }

                // Check if any assets are using this type
                var assetsUsingType = await _context.Assets
                    .AnyAsync(a => (int)a.Type == id + 1000); // Custom asset types start from 1000

                if (assetsUsingType)
                {
                    return BadRequest(new { message = "This asset type is in use and cannot be deleted." });
                }

                _context.CustomAssetTypes.Remove(assetType);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Custom asset type deleted: {AssetTypeName}", assetType.Name);
                return Ok(new { message = $"Custom asset type '{assetType.Name}' successfully deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting custom asset type with ID {AssetTypeId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the custom asset type." });
            }
        }

        #endregion

        #region Helper Methods

        private string GetAssetTypeDisplayName(AssetType type)
        {
            return type switch
            {
                AssetType.Currency => "ارز",
                AssetType.Gold => "طلا",
                AssetType.Silver => "نقره",
                AssetType.Crypto => "رمزارز",
                AssetType.PreciousMetals => "فلزات گرانبها",
                AssetType.Car => "ماشین",
                AssetType.RealEstate => "املاک",
                AssetType.Stock => "سهام",
                AssetType.Bond => "اوراق قرضه",
                AssetType.ETF => "صندوق‌های قابل معامله",
                AssetType.Custom => "سفارشی",
                _ => type.ToString()
            };
        }

        #endregion
    }
} 