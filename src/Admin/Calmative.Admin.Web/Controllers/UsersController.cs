using Calmative.Admin.Web.Models;
using Calmative.Admin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace Calmative.Admin.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IApiService apiService, ILogger<UsersController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // In a real application, you would need to implement an admin API endpoint
                // For this demo, we're simulating fetching users
                var users = await _apiService.GetAsync<List<UserViewModel>>("/api/admin/users", GetAdminToken());
                
                var model = new UsersListViewModel
                {
                    Users = users ?? new List<UserViewModel>(),
                    TotalUsers = users?.Count ?? 0
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user list");
                
                // For demo purposes, populate with mock data
                var model = new UsersListViewModel
                {
                    Users = GetMockUsers(),
                    TotalUsers = 5
                };
                
                return View(model);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var model = new UserDetailsViewModel();
                
                // Fetch user details
                var user = await _apiService.GetAsync<UserViewModel>($"/api/admin/users/{id}", GetAdminToken());
                if (user != null)
                {
                    model.User = user;
                }

                try
                {
                    // Fetch portfolios
                    var portfolios = await _apiService.GetAsync<List<UserPortfolioViewModel>>($"/api/admin/users/{id}/portfolios", GetAdminToken());
                    if (portfolios != null)
                    {
                        model.Portfolios = portfolios;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving portfolios for user ID {UserId}", id);
                }

                try
                {
                    // Fetch activities
                    var activities = await _apiService.GetAsync<List<UserActivityViewModel>>($"/api/admin/users/{id}/activities", GetAdminToken());
                    if (activities != null)
                    {
                        model.Activities = activities;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving activities for user ID {UserId}", id);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user details for ID: {UserId}", id);
                
                // For demo purposes, return a mock user
                var mockUser = GetMockUsers().FirstOrDefault(u => u.Id == id) ?? GetMockUsers().First();
                
                var model = new UserDetailsViewModel
                {
                    User = mockUser,
                    Portfolios = GetMockPortfolios(),
                    Activities = GetMockActivities()
                };
                
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _apiService.DeleteAsync($"/api/admin/users/{id}", GetAdminToken());
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "User successfully deleted.";
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to delete user {UserId}. Status: {StatusCode}, Response: {Response}", 
                        id, response.StatusCode, content);
                    
                    TempData["ErrorMessage"] = "Failed to delete user. Please try again.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the user.";
                return RedirectToAction(nameof(Index));
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
                },
                new UserViewModel 
                { 
                    Id = 4, 
                    FirstName = "Alice", 
                    LastName = "Williams", 
                    Email = "alice.williams@example.com", 
                    CreatedAt = DateTime.Now.AddDays(-3),
                    IsEmailConfirmed = true
                },
                new UserViewModel 
                { 
                    Id = 5, 
                    FirstName = "Charlie", 
                    LastName = "Brown", 
                    Email = "charlie.brown@example.com", 
                    CreatedAt = DateTime.Now.AddDays(-1),
                    IsEmailConfirmed = false
                }
            };
        }

        private List<UserPortfolioViewModel> GetMockPortfolios()
        {
            return new List<UserPortfolioViewModel>
            {
                new UserPortfolioViewModel 
                { 
                    Id = 1, 
                    Name = "Main Portfolio", 
                    Description = "My primary investment portfolio", 
                    CreatedAt = DateTime.Now.AddDays(-9),
                    AssetsCount = 5,
                    TotalValue = 15000.00m
                },
                new UserPortfolioViewModel 
                { 
                    Id = 2, 
                    Name = "Crypto Holdings", 
                    Description = "Digital assets only", 
                    CreatedAt = DateTime.Now.AddDays(-7),
                    AssetsCount = 3,
                    TotalValue = 8500.25m
                },
            };
        }

        private List<UserActivityViewModel> GetMockActivities()
        {
            return new List<UserActivityViewModel>
            {
                new UserActivityViewModel 
                { 
                    Type = "PortfolioCreated", 
                    Description = "Created portfolio 'Main Portfolio'",
                    Timestamp = DateTime.Now.AddDays(-9),
                    EntityId = 1,
                    EntityType = "Portfolio"
                },
                new UserActivityViewModel 
                { 
                    Type = "AssetAdded", 
                    Description = "Added 2.5 BTC to portfolio 'Crypto Holdings'",
                    Timestamp = DateTime.Now.AddDays(-7),
                    EntityId = 3,
                    EntityType = "Asset"
                },
                new UserActivityViewModel 
                { 
                    Type = "PortfolioCreated", 
                    Description = "Created portfolio 'Crypto Holdings'",
                    Timestamp = DateTime.Now.AddDays(-7),
                    EntityId = 2,
                    EntityType = "Portfolio"
                },
                new UserActivityViewModel 
                { 
                    Type = "AssetAdded", 
                    Description = "Added 10 ETH to portfolio 'Crypto Holdings'",
                    Timestamp = DateTime.Now.AddDays(-6),
                    EntityId = 4,
                    EntityType = "Asset"
                },
            };
        }
    }
} 