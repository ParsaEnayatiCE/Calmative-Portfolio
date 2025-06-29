using AutoMapper;
using Calmative.Server.API.Data;
using Calmative.Server.API.DTOs;
using Calmative.Server.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calmative.Server.API.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PortfolioService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PortfolioDto>> GetUserPortfoliosAsync(int userId)
        {
            var portfolios = await _context.Portfolios
                .Where(p => p.UserId == userId)
                .Include(p => p.Assets)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();

            var portfolioDtos = _mapper.Map<List<PortfolioDto>>(portfolios);

            // Update current prices for all assets
            foreach (var portfolioDto in portfolioDtos)
            {
                await UpdateCurrentPricesAsync(portfolioDto.Assets);
            }

            return portfolioDtos;
        }

        public async Task<PortfolioDto?> GetPortfolioByIdAsync(int portfolioId, int userId)
        {
            var portfolio = await _context.Portfolios
                .Where(p => p.Id == portfolioId && p.UserId == userId)
                .Include(p => p.Assets)
                .FirstOrDefaultAsync();

            if (portfolio == null)
                return null;

            var portfolioDto = _mapper.Map<PortfolioDto>(portfolio);
            await UpdateCurrentPricesAsync(portfolioDto.Assets);

            return portfolioDto;
        }

        public async Task<PortfolioDto?> CreatePortfolioAsync(CreatePortfolioDto createPortfolioDto, int userId)
        {
            var portfolio = new Portfolio
            {
                Name = createPortfolioDto.Name,
                Description = createPortfolioDto.Description,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Portfolios.Add(portfolio);
            await _context.SaveChangesAsync();

            return _mapper.Map<PortfolioDto>(portfolio);
        }

        public async Task<PortfolioDto?> UpdatePortfolioAsync(int portfolioId, UpdatePortfolioDto updatePortfolioDto, int userId)
        {
            var portfolio = await _context.Portfolios
                .Where(p => p.Id == portfolioId && p.UserId == userId)
                .FirstOrDefaultAsync();

            if (portfolio == null)
                return null;

            portfolio.Name = updatePortfolioDto.Name;
            portfolio.Description = updatePortfolioDto.Description;
            portfolio.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<PortfolioDto>(portfolio);
        }

        public async Task<bool> DeletePortfolioAsync(int portfolioId, int userId)
        {
            var portfolio = await _context.Portfolios
                .Where(p => p.Id == portfolioId && p.UserId == userId)
                .FirstOrDefaultAsync();

            if (portfolio == null)
                return false;

            _context.Portfolios.Remove(portfolio);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PortfolioSummaryDto> GetPortfolioSummaryAsync(int portfolioId, int userId)
        {
            var portfolio = await _context.Portfolios
                .Where(p => p.Id == portfolioId && p.UserId == userId)
                .Include(p => p.Assets)
                .FirstOrDefaultAsync();

            if (portfolio == null)
                throw new NotFoundException($"Portfolio with ID {portfolioId} not found");

            var assets = _mapper.Map<List<AssetDto>>(portfolio.Assets);
            await UpdateCurrentPricesAsync(assets);

            var totalValue = assets.Sum(a => a.TotalValue);
            var totalInvestment = assets.Sum(a => a.Quantity * a.PurchasePrice);
            var totalProfitLoss = totalValue - totalInvestment;
            var totalProfitLossPercentage = totalInvestment > 0 ? (totalProfitLoss / totalInvestment) * 100 : 0;

            // Asset type distribution
            var assetTypeDistribution = assets
                .GroupBy(a => a.Type)
                .Select(g => new AssetTypeDistribution
                {
                    AssetType = g.Key.ToString(),
                    Value = g.Sum(a => a.TotalValue),
                    Percentage = totalValue > 0 ? (g.Sum(a => a.TotalValue) / totalValue) * 100 : 0,
                    Count = g.Count()
                })
                .OrderByDescending(d => d.Value)
                .ToList();

            // Top performing assets
            var topPerformingAssets = assets
                .OrderByDescending(a => a.ProfitLossPercentage)
                .Take(5)
                .ToList();

            return new PortfolioSummaryDto
            {
                PortfolioId = portfolioId,
                Name = portfolio.Name,
                TotalValue = totalValue,
                TotalInvestment = totalInvestment,
                TotalProfitLoss = totalProfitLoss,
                TotalProfitLossPercentage = totalProfitLossPercentage,
                AssetCount = assets.Count,
                AssetTypeDistribution = assetTypeDistribution,
                TopPerformingAssets = topPerformingAssets
            };
        }

        private async Task UpdateCurrentPricesAsync(List<AssetDto> assets)
        {
            foreach (var asset in assets)
            {
                var latestPrice = await _context.PriceHistories
                    .Where(ph => ph.Symbol == asset.Symbol && ph.AssetType == asset.Type)
                    .OrderByDescending(ph => ph.Timestamp)
                    .FirstOrDefaultAsync();

                if (latestPrice != null)
                {
                    asset.CurrentPrice = latestPrice.Price;
                }
            }
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
} 