using Microsoft.AspNetCore.Mvc;

namespace FintachartsAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AssetsController : ControllerBase
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}