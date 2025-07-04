using Microsoft.AspNetCore.Mvc;

namespace Calmative.Server.API.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Simple liveness probe for Docker and orchestration platforms.
        /// </summary>
        [HttpGet]
        public IActionResult Get() => Ok(new { status = "Healthy" });
    }
} 