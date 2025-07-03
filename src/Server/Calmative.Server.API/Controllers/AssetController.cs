using Calmative.Server.API.Data;
using Calmative.Server.API.DTOs;
using Calmative.Server.API.Models;
using Calmative.Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Calmative.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AssetController : ControllerBase
    {
        private readonly IAssetService _assetService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AssetController> _logger;

        public AssetController(
            IAssetService assetService, 
            ApplicationDbContext context,
            ILogger<AssetController> logger)
        {
            _assetService = assetService;
            _context = context;
            _logger = logger;
        }

        [HttpGet("portfolio/{portfolioId}")]
        public async Task<ActionResult<IEnumerable<AssetDto>>> GetPortfolioAssets(int portfolioId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var assets = await _assetService.GetPortfolioAssetsAsync(portfolioId, userId.Value);
            return Ok(assets);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AssetDto>> GetAsset(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var asset = await _assetService.GetAssetByIdAsync(id, userId.Value);
            if (asset == null)
                return NotFound();

            return Ok(asset);
        }

        [HttpPost]
        public async Task<ActionResult<AssetDto>> CreateAsset(CreateAssetDto createAssetDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var asset = await _assetService.CreateAssetAsync(createAssetDto, userId.Value);
            if (asset == null)
                return BadRequest("Failed to create asset or portfolio not found");

            return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, asset);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AssetDto>> UpdateAsset(int id, UpdateAssetDto updateAssetDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var asset = await _assetService.UpdateAssetAsync(id, updateAssetDto, userId.Value);
            if (asset == null)
                return NotFound();

            return Ok(asset);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var success = await _assetService.DeleteAssetAsync(id, userId.Value);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id}/price-history")]
        public async Task<ActionResult<IEnumerable<PriceHistoryDto>>> GetAssetPriceHistory(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var history = await _assetService.GetAssetPriceHistoryAsync(id, userId.Value);
            return Ok(history);
        }

        [HttpPost("update-prices")]
        public async Task<IActionResult> UpdateAssetPrices()
        {
            await _assetService.UpdateAssetPricesAsync();
            return Ok(new { message = "Asset prices updated successfully" });
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetAssetTypes()
        {
            try
            {
                _logger.LogInformation("GetAssetTypes endpoint called");
                
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
                
                _logger.LogInformation("Built-in asset types count: {Count}", builtInAssetTypes.Count);
                foreach (var type in builtInAssetTypes)
                {
                    _logger.LogInformation("Built-in type: {Id} - {Name} - {DisplayName}", type.Id, type.Name, type.DisplayName);
                }

                // Get all custom asset types that are active
                var customAssetTypes = await _context.CustomAssetTypes
                    .Where(t => t.IsActive)
                    .Select(t => new
                    {
                        t.Id,
                        t.Name,
                        t.DisplayName,
                        t.Description,
                        IsActive = t.IsActive
                    })
                    .ToListAsync();
                
                _logger.LogInformation("Custom asset types count: {Count}", customAssetTypes.Count);
                foreach (var type in customAssetTypes)
                {
                    _logger.LogInformation("Custom type: {Id} - {Name} - {DisplayName}", type.Id, type.Name, type.DisplayName);
                }

                var result = new
                {
                    BuiltInTypes = builtInAssetTypes,
                    CustomTypes = customAssetTypes
                };
                
                _logger.LogInformation("Returning asset types: {BuiltInCount} built-in, {CustomCount} custom", 
                    builtInAssetTypes.Count, customAssetTypes.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving asset types");
                return StatusCode(500, new { message = "An error occurred while retrieving asset types." });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

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
    }
} 