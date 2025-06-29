using Calmative.Server.API.DTOs;

namespace Calmative.Server.API.Services
{
    public interface IPortfolioService
    {
        Task<IEnumerable<PortfolioDto>> GetUserPortfoliosAsync(int userId);
        Task<PortfolioDto?> GetPortfolioByIdAsync(int portfolioId, int userId);
        Task<PortfolioDto?> CreatePortfolioAsync(CreatePortfolioDto createPortfolioDto, int userId);
        Task<PortfolioDto?> UpdatePortfolioAsync(int portfolioId, UpdatePortfolioDto updatePortfolioDto, int userId);
        Task<bool> DeletePortfolioAsync(int portfolioId, int userId);
        Task<PortfolioSummaryDto> GetPortfolioSummaryAsync(int portfolioId, int userId);
    }
} 