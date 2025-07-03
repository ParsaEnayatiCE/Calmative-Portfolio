using Calmative.Admin.Web.Models;
using Calmative.Admin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calmative.Admin.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IApiService apiService, ILogger<DashboardController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // In a real application, you would need to implement an admin API endpoint
                // For this demo, we're simulating fetching dashboard data
                var dashboardData = await _apiService.GetAsync<DashboardViewModel>("/api/admin/dashboard", GetAdminToken());
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard data");
                
                // For demo purposes, return mock data
                var model = new DashboardViewModel
                {
                    TotalUsers = 5,
                    TotalPortfolios = 8,
                    TotalAssets = 24,
                    TotalValue = 145780.50m,
                    RecentUsers = GetMockUsers()
                };
                
                return View(model);
            }
        }

        private string GetAdminToken()
        {
            // In a real implementation, you would have an admin token
            // For this demo, we're returning an empty string
            return "";
        }

        // Mock data for demonstration
        private List<UserViewModel> GetMockUsers()
        {
            return new List<UserViewModel>
            {
                new UserViewModel 
                { 
                    Id = 1, 
                    FirstName = "John", 
                    LastName = "Doe", 
                    Email = "john.doe@example.com", 
                    CreatedAt = DateTime.Now.AddDays(-10),
                    IsEmailConfirmed = true
                },
                new UserViewModel 
                { 
                    Id = 2, 
                    FirstName = "Jane", 
                    LastName = "Smith", 
                    Email = "jane.smith@example.com", 
                    CreatedAt = DateTime.Now.AddDays(-8),
                    IsEmailConfirmed = true
                },
                new UserViewModel 
                { 
                    Id = 3, 
                    FirstName = "Bob", 
                    LastName = "Johnson", 
                    Email = "bob.johnson@example.com", 
                    CreatedAt = DateTime.Now.AddDays(-5),
                    IsEmailConfirmed = false
                }
            };
        }
    }
} 