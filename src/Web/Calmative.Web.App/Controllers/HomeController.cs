using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Calmative.Web.App.Models;

namespace Calmative.Web.App.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
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

    private bool IsUserLoggedIn()
    {
        return Request.Cookies.ContainsKey("jwt_token") && 
               !string.IsNullOrEmpty(Request.Cookies["jwt_token"]);
    }
}
