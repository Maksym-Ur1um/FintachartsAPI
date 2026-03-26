using FintachartsAPI.Data;
using FintachartsAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FintachartsAPI.Controllers
{
    [ApiController]
    [Route("/api")]
    public class AssetsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IAssetPriceService _assetPriceService;

        public AssetsController(AppDbContext dbContext, IAssetPriceService assetPriceService)
        {
            _dbContext = dbContext;
            _assetPriceService = assetPriceService;
        }

        [HttpGet("assets")]
        public IActionResult GetSupportedAssets()
        {
            var marketAssets = _dbContext.Assets.Select(a => a.Symbol).ToList();

            return Ok(marketAssets);
        }

        [HttpGet("prices")]
        public async Task<IActionResult> GetPrices([FromQuery] string[] assets)
        {
            if (!assets.Any())
            {
                return BadRequest("Please specify at least one asset");
            }

            if (assets.Length > 25)
            {
                return BadRequest("Please choose no more than 25 assets");
            }

            var results = await _assetPriceService.GetPricesAsync(assets);

            return Ok(results);
        }
    }
}