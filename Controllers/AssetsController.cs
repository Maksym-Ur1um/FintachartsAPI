using FintachartsAPI.Data;
using FintachartsAPI.DTOs;
using FintachartsAPI.State;
using Microsoft.AspNetCore.Mvc;

namespace FintachartsAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly MarketStateCache _marketStateCache;

        public AssetsController(AppDbContext dbContext, MarketStateCache marketStateCache)
        {
            _dbContext = dbContext;
            _marketStateCache = marketStateCache;
        }

        [HttpGet("assets")]
        public IActionResult GetSupportedAssets()
        {
            var marketAssets = _dbContext.Assets.Select(a => a.Symbol).ToList();

            return Ok(marketAssets);
        }

        [HttpGet("prices")]
        public IActionResult GetPrices([FromQuery] string[] assets)
        {
            if (!assets.Any())
            {
                return BadRequest("Please specify at least one asset");
            }

            var cacheData = _marketStateCache.GetAssetsData(assets);

            var results = cacheData.Select(kvp => new AssetPriceResponseDto(
                kvp.Key, kvp.Value.Price, kvp.Value.LastUpdate)).ToList();

            return Ok(results);
        }
    }
}