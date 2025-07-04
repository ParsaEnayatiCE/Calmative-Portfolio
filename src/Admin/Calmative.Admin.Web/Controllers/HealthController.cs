using Microsoft.AspNetCore.Mvc;

namespace Calmative.Admin.Web.Controllers
{
    public class HealthController : Controller
    {
        [HttpGet("/health")]
        public IActionResult Index() => Content("Healthy");
    }
} 