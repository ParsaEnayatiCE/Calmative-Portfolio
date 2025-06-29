using AutoMapper;
using Calmative.Server.API.Data;
using Calmative.Server.API.DTOs;
using Calmative.Server.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calmative.Server.API.Services
{
    public class AssetService : IAssetService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AssetService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AssetDto>> GetPortfolioAssetsAsync(int portfolioId, int userId)
        {
            // Verify portfolio belongs to user
            var portfolioExists = await _context.Portfolios
                .AnyAsync(p => p.Id == portfolioId && p.UserId == userId);

            if (!portfolioExists)
                return Enumerable.Empty<AssetDto>();

            var assets = await _context.Assets
                .Where(a => a.PortfolioId == portfolioId)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();

            var assetDtos = _mapper.Map<List<AssetDto>>(assets);
            await UpdateCurrentPricesAsync(assetDtos);

            return assetDtos;
        }

        public async Task<AssetDto?> GetAssetByIdAsync(int assetId, int userId)
        {
            var asset = await _context.Assets
                .Include(a => a.Portfolio)
                .Where(a => a.Id == assetId && a.Portfolio.UserId == userId)
                .FirstOrDefaultAsync();

            if (asset == null)
                return null;

            var assetDto = _mapper.Map<AssetDto>(asset);
            await UpdateCurrentPricesAsync(new List<AssetDto> { assetDto });

            return assetDto;
        }

        public async Task<AssetDto?> CreateAssetAsync(CreateAssetDto createAssetDto, int userId)
        {
            var portfolio = await _context.Portfolios
                .FirstOrDefaultAsync(p => p.Id == createAssetDto.PortfolioId && p.UserId == userId);

            if (portfolio == null)
            {
                return null; // Portfolio not found or access denied
            }

            var asset = _mapper.Map<Asset>(createAssetDto);
            asset.CurrentPrice = createAssetDto.CurrentPrice; // Explicitly set CurrentPrice

            _context.Assets.Add(asset);
            
            // Add initial price to history
            var initialPriceHistory = new PriceHistory
            {
                Symbol = asset.Symbol,
                AssetType = asset.Type,
                Price = asset.CurrentPrice,
                Timestamp = DateTime.UtcNow,
                Source = "Initial"
            };
            _context.PriceHistories.Add(initialPriceHistory);

            await _context.SaveChangesAsync();
            return _mapper.Map<AssetDto>(asset);
        }

        public async Task<AssetDto?> UpdateAssetAsync(int assetId, UpdateAssetDto updateAssetDto, int userId)
        {
            var asset = await _context.Assets
                .Include(a => a.Portfolio)
                .FirstOrDefaultAsync(a => a.Id == assetId && a.Portfolio.UserId == userId);

            if (asset == null)
            {
                return null; // Not found or access denied
            }

            var oldPrice = asset.CurrentPrice;

            // Manually map properties to ensure they are tracked by EF Core
            asset.Name = updateAssetDto.Name;
            asset.Symbol = updateAssetDto.Symbol;
            asset.Type = updateAssetDto.Type;
            asset.Quantity = updateAssetDto.Quantity;
            asset.PurchasePrice = updateAssetDto.PurchasePrice;
            asset.PurchaseDate = updateAssetDto.PurchaseDate;
            asset.CurrentPrice = updateAssetDto.CurrentPrice;
            asset.UpdatedAt = DateTime.UtcNow;


            if (asset.CurrentPrice != oldPrice)
            {
                var priceHistory = new PriceHistory
                {
                    Symbol = asset.Symbol,
                    AssetType = asset.Type,
                    Price = asset.CurrentPrice,
                    Timestamp = DateTime.UtcNow,
                    Source = "Manual Update"
                };
                _context.PriceHistories.Add(priceHistory);
            }

            await _context.SaveChangesAsync();

            return _mapper.Map<AssetDto>(asset);
        }

        public async Task<bool> DeleteAssetAsync(int assetId, int userId)
        {
            var asset = await _context.Assets
                .Include(a => a.Portfolio)
                .Where(a => a.Id == assetId && a.Portfolio.UserId == userId)
                .FirstOrDefaultAsync();

            if (asset == null)
                return false;

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task UpdateAssetPricesAsync()
        {
            var assets = await _context.Assets.ToListAsync();
            var assetsToUpdate = new List<Asset>();

            foreach (var asset in assets)
            {
                var latestPrice = await GetLatestPriceAsync(asset.Symbol, asset.Type);
                if (latestPrice > 0 && latestPrice != asset.CurrentPrice)
                {
                    asset.CurrentPrice = latestPrice;
                    asset.UpdatedAt = DateTime.UtcNow;
                    assetsToUpdate.Add(asset);
                }
            }

            if (assetsToUpdate.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetLatestPriceAsync(string symbol, AssetType assetType)
        {
            var latestPrice = await _context.PriceHistories
                .Where(ph => ph.Symbol == symbol && ph.AssetType == assetType)
                .OrderByDescending(ph => ph.Timestamp)
                .FirstOrDefaultAsync();

            return latestPrice?.Price ?? 0;
        }

        private async Task UpdateCurrentPricesAsync(List<AssetDto> assets)
        {
            foreach (var asset in assets)
            {
                var latestPrice = await GetLatestPriceAsync(asset.Symbol, asset.Type);
                if (latestPrice > 0)
                {
                    asset.CurrentPrice = latestPrice;
                }
            }
        }

        public async Task<IEnumerable<PriceHistoryDto>> GetAssetPriceHistoryAsync(int assetId, int userId)
        {
            var asset = await _context.Assets
                .AsNoTracking()
                .Include(a => a.Portfolio)
                .FirstOrDefaultAsync(a => a.Id == assetId && a.Portfolio.UserId == userId);

            if (asset == null)
            {
                return Enumerable.Empty<PriceHistoryDto>();
            }

            var priceHistory = await _context.PriceHistories
                .AsNoTracking()
                .Where(ph => ph.Symbol == asset.Symbol && ph.AssetType == asset.Type)
                .OrderBy(ph => ph.Timestamp)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PriceHistoryDto>>(priceHistory);
        }
    }
} 