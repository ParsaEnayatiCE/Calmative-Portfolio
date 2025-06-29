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
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;

        public PortfolioController(IPortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortfolioDto>>> GetUserPortfolios()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var portfolios = await _portfolioService.GetUserPortfoliosAsync(userId.Value);
            return Ok(portfolios);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PortfolioDto>> GetPortfolio(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var portfolio = await _portfolioService.GetPortfolioByIdAsync(id, userId.Value);
            if (portfolio == null)
                return NotFound();

            return Ok(portfolio);
        }

        [HttpPost]
        public async Task<ActionResult<PortfolioDto>> CreatePortfolio(CreatePortfolioDto createPortfolioDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var portfolio = await _portfolioService.CreatePortfolioAsync(createPortfolioDto, userId.Value);
            if (portfolio == null)
                return BadRequest("Failed to create portfolio");

            return CreatedAtAction(nameof(GetPortfolio), new { id = portfolio.Id }, portfolio);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PortfolioDto>> UpdatePortfolio(int id, UpdatePortfolioDto updatePortfolioDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var portfolio = await _portfolioService.UpdatePortfolioAsync(id, updatePortfolioDto, userId.Value);
            if (portfolio == null)
                return NotFound();

            return Ok(portfolio);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePortfolio(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var success = await _portfolioService.DeletePortfolioAsync(id, userId.Value);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id}/summary")]
        public async Task<ActionResult<PortfolioSummaryDto>> GetPortfolioSummary(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                var summary = await _portfolioService.GetPortfolioSummaryAsync(id, userId.Value);
                return Ok(summary);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
} 