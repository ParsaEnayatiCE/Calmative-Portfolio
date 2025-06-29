using Calmative.Server.API.DTOs;
using Calmative.Server.API.Models;

namespace Calmative.Server.API.Services
{
    public interface IAssetService
    {
        Task<IEnumerable<AssetDto>> GetPortfolioAssetsAsync(int portfolioId, int userId);
        Task<AssetDto?> GetAssetByIdAsync(int assetId, int userId);
        Task<AssetDto?> CreateAssetAsync(CreateAssetDto createAssetDto, int userId);
        Task<AssetDto?> UpdateAssetAsync(int assetId, UpdateAssetDto updateAssetDto, int userId);
        Task<bool> DeleteAssetAsync(int assetId, int userId);
        Task UpdateAssetPricesAsync();
        Task<decimal> GetLatestPriceAsync(string symbol, AssetType assetType);
        Task<IEnumerable<PriceHistoryDto>> GetAssetPriceHistoryAsync(int assetId, int userId);
    }
} 