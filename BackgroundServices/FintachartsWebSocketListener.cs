using FintachartsAPI.Data;
using FintachartsAPI.Services;
using FintachartsAPI.State;

namespace FintachartsAPI.BackgroundServices
{
    public class FintachartsWebSocketListener : BackgroundService
    {
        private readonly MarketStateCache _marketStateCache;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<FintachartsWebSocketListener> _logger;

        public FintachartsWebSocketListener(
            MarketStateCache marketStateCache,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<FintachartsWebSocketListener> logger)
        {
            _marketStateCache = marketStateCache;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WebSocket Listener starting...");

            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            List<Guid> assets = dbContext.Assets.Select(a => a.FintachartsId).ToList();
            _logger.LogInformation($"Loaded {assets.Count()} assets");

            var authService = scope.ServiceProvider.GetRequiredService<IFintachartsAuthService>();
            string token = await authService.GetTokenAsync();
            _logger.LogInformation("Token retrieved, preparing to connect...");
            throw new NotImplementedException();
        }
    }
}