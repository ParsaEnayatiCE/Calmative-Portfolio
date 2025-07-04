using Calmative.Web.App.Models.ViewModels;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace Calmative.Web.App.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new(System.Text.Json.JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        static ApiService()
        {
            _jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        }

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
        }

        private void AddJwtTokenToHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(token))
            {
                // Remove any existing Authorization header first
                if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");
                }
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("JWT token added to default request headers. Token length: {Length}", token.Length);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                _logger.LogWarning("No JWT token found in cookies, Authorization header cleared");
            }
        }

        public async Task<(bool Success, string? Message)> Login(LoginViewModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/login", model);

                if (response.IsSuccessStatusCode)
                {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            var token = result.GetProperty("token").GetString();

            if (string.IsNullOrWhiteSpace(token))
            {
                return (false, "Token was not provided by the server.");
            }

                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTime.UtcNow.AddMinutes(60),
                        SameSite = SameSiteMode.Strict
                    };
                    _httpContextAccessor.HttpContext?.Response.Cookies.Append("jwt_token", token, cookieOptions);

            return (true, null);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Login failed with status {StatusCode}: {Response}", response.StatusCode, errorContent);
                    var message = "Invalid credentials.";
                    try {
                        var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent);
                        if(errorJson.TryGetProperty("message", out var msg)) {
                            message = msg.GetString() ?? message;
                        }
                    } catch { /* Ignore deserialize error */ }

                    return (false, message);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to login endpoint failed.");
                return (false, "Could not connect to the server. Please try again later.");
            }
        }

        public async Task<(bool Success, string? Message)> Register(RegisterViewModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/register", model);

                if (response.IsSuccessStatusCode)
                {
            var content = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var message = content.TryGetProperty("message", out var msg) ? msg.GetString() : "Registration successful.";
                    return (true, message);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Registration failed with status {StatusCode}: {Response}", response.StatusCode, errorContent);
                     var message = "Registration failed.";
                    try {
                        var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent);
                        if(errorJson.TryGetProperty("message", out var msg)) {
                            message = msg.GetString() ?? message;
                        }
                    } catch { /* Ignore deserialize error */ }
                    return (false, message);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to registration endpoint failed.");
                return (false, "Could not connect to the server. Please try again later.");
            }
        }

        public async Task<(bool Success, string? Message)> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                AddJwtTokenToHeader();
                var response = await _httpClient.PostAsJsonAsync("auth/change-password", model);

                if (response.IsSuccessStatusCode)
                {
            var content = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var message = content.TryGetProperty("message", out var msg) ? msg.GetString() : "Password changed successfully.";
                    return (true, message);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Change password failed with status {StatusCode}: {Response}", response.StatusCode, errorContent);
                    var message = "Could not change password.";
                    try {
                        var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent);
                        if(errorJson.TryGetProperty("message", out var msg)) {
                            message = msg.GetString() ?? message;
                        }
                    } catch { /* Ignore deserialize error */ }
                    return (false, message);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to change password endpoint failed.");
                return (false, "Could not connect to the server. Please try again later.");
            }
        }

        public async Task<(bool Success, string? Message)> RequestPasswordReset(string email)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/request-password-reset", new { Email = email });
            var content = await response.Content.ReadFromJsonAsync<JsonElement>();
            var message = content.TryGetProperty("message", out var msg) ? msg.GetString() : "An error occurred.";

            return (response.IsSuccessStatusCode, message);
        }

        public async Task<(bool Success, string? Message)> ResetPassword(ResetPasswordViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/reset-password", model);
            var content = await response.Content.ReadFromJsonAsync<JsonElement>();
            var message = content.TryGetProperty("message", out var msg) ? msg.GetString() : "An error occurred.";

            return (response.IsSuccessStatusCode, message);
        }

        public async Task<UserViewModel?> GetUserProfile()
        {
            try
            {
                AddJwtTokenToHeader();
                return await _httpClient.GetFromJsonAsync<UserViewModel>("auth/profile");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to get user profile failed.");
                return null;
            }
        }

        public async Task<List<PortfolioViewModel>?> GetPortfolios()
        {
            try
            {
                AddJwtTokenToHeader();
                return await _httpClient.GetFromJsonAsync<List<PortfolioViewModel>>("portfolio", _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to get portfolios failed.");
                return null;
            }
        }

        public async Task<PortfolioViewModel?> GetPortfolio(int id)
        {
            try
            {
                AddJwtTokenToHeader();
                return await _httpClient.GetFromJsonAsync<PortfolioViewModel>($"portfolio/{id}", _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to get portfolio failed.");
                return null;
            }
        }

        public async Task<bool> CreatePortfolio(CreatePortfolioViewModel model)
        {
            try
            {
                AddJwtTokenToHeader();
                var response = await _httpClient.PostAsJsonAsync("portfolio", model);
            return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to create portfolio failed.");
                return false;
            }
        }

        public async Task<bool> UpdatePortfolio(int id, UpdatePortfolioViewModel model)
        {
            try
            {
                AddJwtTokenToHeader();
                var response = await _httpClient.PutAsJsonAsync($"portfolio/{id}", model);
            return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to update portfolio failed.");
                return false;
            }
        }

        public async Task<bool> DeletePortfolio(int id)
        {
            try
            {
                AddJwtTokenToHeader();
                var response = await _httpClient.DeleteAsync($"portfolio/{id}");
            return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to delete portfolio failed.");
                return false;
            }
        }

        public async Task<bool> CreateAsset(int portfolioId, CreateAssetViewModel model)
        {
            try
            {
                AddJwtTokenToHeader();
                
                // Create a DTO that matches the server's expected format
                var assetDto = new
                {
                    model.Name,
                    model.Symbol,
                    Type = (int)model.Type, // Send as integer to avoid enum serialization issues
                    model.Quantity,
                    model.PurchasePrice,
                    model.CurrentPrice,
                    model.PurchaseDate,
                    PortfolioId = portfolioId
                };
                
                _logger.LogInformation("Creating asset with Type: {Type}, Name: {Name}", (int)model.Type, model.Name);
                
                var response = await _httpClient.PostAsJsonAsync("asset", assetDto);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create asset. Status: {Status}, Error: {Error}", 
                        response.StatusCode, errorContent);
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to create asset failed.");
                return false;
            }
        }

        public async Task<bool> UpdateAsset(int portfolioId, int assetId, UpdateAssetViewModel model)
        {
            try
            {
                AddJwtTokenToHeader();
                
                // Create a DTO that matches the server's expected format
                var assetDto = new
                {
                    model.Id,
                    model.Name,
                    model.Symbol,
                    Type = (int)model.Type, // Send as integer to avoid enum serialization issues
                    model.Quantity,
                    model.PurchasePrice,
                    model.CurrentPrice,
                    model.PurchaseDate,
                    PortfolioId = portfolioId
                };
                
                _logger.LogInformation("Updating asset {AssetId} with Type: {Type}, Name: {Name}", 
                    assetId, (int)model.Type, model.Name);
                
                var response = await _httpClient.PutAsJsonAsync($"asset/{assetId}", assetDto);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to update asset. Status: {Status}, Error: {Error}", 
                        response.StatusCode, errorContent);
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to update asset failed.");
                return false;
            }
        }

        public async Task<bool> DeleteAsset(int assetId)
        {
            try
            {
                AddJwtTokenToHeader();
                var response = await _httpClient.DeleteAsync($"asset/{assetId}");
            return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to delete asset failed.");
                return false;
            }
        }

        public async Task<DashboardViewModel?> GetDashboard()
        {
            try
            {
                AddJwtTokenToHeader();
                return await _httpClient.GetFromJsonAsync<DashboardViewModel>("dashboard/overview", _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to get dashboard failed.");
                return null;
            }
        }

        public async Task<(bool Success, string? Message)> ConfirmEmail(string userId, string token)
        {
            try
            {
                // Use a dedicated handler to prevent automatic redirect so we can inspect the Location header
                using var handler = new HttpClientHandler { AllowAutoRedirect = false };
                using var client = new HttpClient(handler) { BaseAddress = _httpClient.BaseAddress };

                var response = await client.GetAsync($"auth/confirm-email?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}");

                bool isSuccess = false;
                string? message = null;

                if (response.StatusCode == System.Net.HttpStatusCode.Redirect ||
                    response.StatusCode == System.Net.HttpStatusCode.Moved ||
                    response.StatusCode == System.Net.HttpStatusCode.SeeOther ||
                    response.StatusCode == System.Net.HttpStatusCode.TemporaryRedirect)
                {
                    var location = response.Headers.Location?.ToString() ?? string.Empty;
                    // The API includes isSuccess query param in redirect target
                    if (location.Contains("isSuccess=true", StringComparison.OrdinalIgnoreCase) ||
                        location.Contains("isSuccess=True", StringComparison.OrdinalIgnoreCase))
                    {
                        isSuccess = true;
                    }
                }
                else
                {
                    isSuccess = response.IsSuccessStatusCode;
                }

                if (!isSuccess)
                {
                    message = "تأیید ایمیل ناموفق بود. لینک منقضی یا نامعتبر است.";
                }

                return (isSuccess, message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to confirm email failed.");
                return (false, "اتصال به سرور امکان‌پذیر نیست. لطفاً بعداً دوباره تلاش کنید.");
            }
        }

        public async Task<List<PriceHistoryDto>?> GetAssetPriceHistory(int assetId)
        {
            try
            {
                AddJwtTokenToHeader();
                return await _httpClient.GetFromJsonAsync<List<PriceHistoryDto>>($"asset/{assetId}/price-history");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to get asset price history failed.");
                return null;
            }
        }

        public async Task<RecommendationViewModel?> GetUserRecommendations()
        {
            try
            {
                _logger.LogWarning("DEBUG: Starting GetUserRecommendations API call");
                
                // Check if token exists
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
                _logger.LogWarning("DEBUG: JWT token exists: {HasToken}, Length: {TokenLength}", 
                    !string.IsNullOrEmpty(token), token?.Length ?? 0);
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("DEBUG: No JWT token found in cookies");
                    return null;
                }
                
                // Explicitly set the Authorization header for this request
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "recommendation/user");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                _logger.LogWarning("DEBUG: Making request to recommendation/user with explicit Authorization header");
                
                // Make the request with a timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                try
                {
                    var response = await _httpClient.SendAsync(requestMessage, cts.Token);
                    _logger.LogWarning("DEBUG: Response received, status code: {StatusCode}", response.StatusCode);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("DEBUG: Error response content: {ErrorContent}", errorContent);
                        return null;
                    }
                    
                    // Deserialize directly from the response stream to avoid string size limits or encoding issues
                    var result = await response.Content.ReadFromJsonAsync<RecommendationViewModel>(_jsonOptions);
                    _logger.LogWarning("DEBUG: Deserialization successful: {HasResult}", result != null);
                    
                    if (result != null)
                    {
                        foreach (var a in result.RecommendedAssets)
                        {
                            a.TypeDisplayName = GetPersianAssetType(a.Type);
                        }
                    }
                    
                    return result;
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("DEBUG: API request timed out after 30 seconds");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DEBUG: HTTP request to get user recommendations failed");
                _logger.LogWarning("DEBUG: Exception details: {Message}, {StackTrace}", ex.Message, ex.StackTrace);
                
                if (ex.InnerException != null)
                {
                    _logger.LogWarning("DEBUG: Inner exception: {Message}, {StackTrace}", 
                        ex.InnerException.Message, ex.InnerException.StackTrace);
                }
                
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "DEBUG: JSON deserialization error in GetUserRecommendations");
                _logger.LogWarning("DEBUG: Exception details: {Message}, {StackTrace}", ex.Message, ex.StackTrace);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DEBUG: Unexpected error in GetUserRecommendations");
                _logger.LogWarning("DEBUG: Exception details: {Message}, {StackTrace}", ex.Message, ex.StackTrace);
                return null;
            }
        }
        
        public async Task<RecommendationViewModel?> GetPortfolioRecommendations(int portfolioId)
        {
            try
            {
                _logger.LogWarning("DEBUG: Starting GetPortfolioRecommendations API call for portfolioId={PortfolioId}", portfolioId);
                
                // Check if token exists
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
                _logger.LogWarning("DEBUG: JWT token exists: {HasToken}, Length: {TokenLength}", 
                    !string.IsNullOrEmpty(token), token?.Length ?? 0);
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("DEBUG: No JWT token found in cookies");
                    return null;
                }
                
                // Explicitly set the Authorization header for this request
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"recommendation/portfolio/{portfolioId}");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                _logger.LogWarning("DEBUG: Making request to recommendation/portfolio/{PortfolioId} with explicit Authorization header", portfolioId);
                
                // Make the request with a timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                try
                {
                    var response = await _httpClient.SendAsync(requestMessage, cts.Token);
                    _logger.LogWarning("DEBUG: Response received, status code: {StatusCode}", response.StatusCode);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("DEBUG: Error response content: {ErrorContent}", errorContent);
                        return null;
                    }
                    
                    // Deserialize directly from the response stream to avoid string size limits or encoding issues
                    var result = await response.Content.ReadFromJsonAsync<RecommendationViewModel>(_jsonOptions);
                    _logger.LogWarning("DEBUG: Deserialization successful: {HasResult}", result != null);
                    
                    if (result != null)
                    {
                        foreach (var a in result.RecommendedAssets)
                        {
                            a.TypeDisplayName = GetPersianAssetType(a.Type);
                        }
                    }
                    
                    return result;
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("DEBUG: API request timed out after 30 seconds");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DEBUG: HTTP request to get portfolio recommendations failed");
                _logger.LogWarning("DEBUG: Exception details: {Message}, {StackTrace}", ex.Message, ex.StackTrace);
                
                if (ex.InnerException != null)
                {
                    _logger.LogWarning("DEBUG: Inner exception: {Message}, {StackTrace}", 
                        ex.InnerException.Message, ex.InnerException.StackTrace);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DEBUG: Unexpected error in GetPortfolioRecommendations");
                _logger.LogWarning("DEBUG: Exception details: {Message}, {StackTrace}", ex.Message, ex.StackTrace);
                return null;
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                AddJwtTokenToHeader();
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                return result ?? throw new InvalidOperationException($"Failed to deserialize response to {typeof(T).Name}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to {Endpoint} failed.", endpoint);
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception for {Endpoint}.", endpoint);
                }
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from {Endpoint}.", endpoint);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetAsync for {Endpoint}.", endpoint);
                throw;
            }
        }

        private static string GetPersianAssetType(AssetType type)
        {
            return Models.ViewModels.AssetTypeHelper.GetAssetTypeDisplayName(type);
        }
    }
} 