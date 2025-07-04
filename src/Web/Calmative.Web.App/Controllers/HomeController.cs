using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Calmative.Web.App.Models;
using Calmative.Web.App.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Calmative.Web.App.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Create a direct HttpClient for debugging
        _httpClient = new HttpClient();
        var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7000/api/";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public IActionResult Index()
    {
        // If user is logged in, redirect to dashboard
        if (IsUserLoggedIn())
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Route("api-debug")]
    public async Task<IActionResult> ApiDebug()
    {
        var results = new Dictionary<string, object>();
        
        try
        {
            _logger.LogWarning("DEBUG: Testing API connectivity");
            results.Add("timestamp", DateTime.Now.ToString());
            results.Add("baseUrl", _httpClient.BaseAddress?.ToString() ?? "null");
            
            // Test API endpoints
            var endpoints = new[] {
                "asset/types",
                "dashboard/overview",
                "recommendation/user"
            };
            
                            // Get JWT token from cookies
                var token = HttpContext.Request.Cookies["jwt_token"];
                results.Add("jwt_token_exists", !string.IsNullOrEmpty(token));
                results.Add("jwt_token_length", token?.Length ?? 0);
                
                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        _logger.LogWarning("DEBUG: Testing endpoint {Endpoint}", endpoint);
                        
                        // Create request with explicit Authorization header
                        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                        if (!string.IsNullOrEmpty(token))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            _logger.LogWarning("DEBUG: Added Authorization header to request for {Endpoint}", endpoint);
                        }
                        
                        var response = await _httpClient.SendAsync(request);
                        
                        results.Add($"{endpoint}_status", (int)response.StatusCode);
                        results.Add($"{endpoint}_reason", response.ReasonPhrase);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            results.Add($"{endpoint}_content_length", content?.Length ?? 0);
                            results.Add($"{endpoint}_content_preview", 
                                content?.Length > 100 ? content.Substring(0, 100) + "..." : content);
                        }
                    }
                    catch (Exception ex)
                    {
                        results.Add($"{endpoint}_error", ex.Message);
                        if (ex.InnerException != null)
                        {
                            results.Add($"{endpoint}_inner_error", ex.InnerException.Message);
                        }
                    }
                }
            
            return Json(results);
        }
        catch (Exception ex)
        {
            results.Add("error", ex.Message);
            if (ex.InnerException != null)
            {
                results.Add("inner_error", ex.InnerException.Message);
            }
            return Json(results);
        }
    }

    private bool IsUserLoggedIn()
    {
        return Request.Cookies.ContainsKey("jwt_token") && 
               !string.IsNullOrEmpty(Request.Cookies["jwt_token"]);
    }
}
