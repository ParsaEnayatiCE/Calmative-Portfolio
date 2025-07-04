using Microsoft.AspNetCore.Mvc;

namespace Calmative.Web.App.Controllers
{
    public class HealthController : Controller
    {
        [HttpGet("/health")]
        public IActionResult Index() => Content("Healthy");
    }
} 