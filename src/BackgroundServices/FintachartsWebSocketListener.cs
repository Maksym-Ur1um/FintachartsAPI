using FintachartsAPI.Configuration;
using FintachartsAPI.Data;
using FintachartsAPI.Models;
using FintachartsAPI.Services;
using FintachartsAPI.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text.Json;

namespace FintachartsAPI.BackgroundServices
{
    public class FintachartsWebSocketListener : BackgroundService
    {
        private readonly MarketStateCache _marketStateCache;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<FintachartsWebSocketListener> _logger;
        private readonly IOptions<FintachartsOptions> _options;

        public FintachartsWebSocketListener(
            MarketStateCache marketStateCache,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<FintachartsWebSocketListener> logger,
            IOptions<FintachartsOptions> options)
        {
            _marketStateCache = marketStateCache;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WebSocket Listener starting...");

            List<Asset> assets = await GetAssetsFromDatabaseAsync();
            _logger.LogInformation($"Loaded {assets.Count} assets");

            Dictionary<string, string> idToSymbol = new Dictionary<string, string>();
            foreach (var asset in assets)
            {
                idToSymbol.Add(asset.FintachartsId.ToString(), asset.Symbol);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using ClientWebSocket clientWebSocket = new ClientWebSocket();

                    await ConnectAndSubscribeAsync(clientWebSocket, assets, stoppingToken);
                    await ListenForMessagesAsync(clientWebSocket, idToSymbol, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Critical error occurred while communicating with the Fintacharts WebSocket server.");
                }

                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task<List<Asset>> GetAssetsFromDatabaseAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return await dbContext.Assets.ToListAsync();
        }

        private async Task ConnectAndSubscribeAsync(ClientWebSocket clientWebSocket, List<Asset> assets, CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IFintachartsAuthService>();

            string token = await authService.GetTokenAsync();
            _logger.LogInformation("Token retrieved, preparing to connect...");

            Uri connectionUrl = new($"{_options.Value.WssUrl}/api/streaming/ws/v1/realtime?token={token}");

            await clientWebSocket.ConnectAsync(connectionUrl, stoppingToken);
            _logger.LogInformation("WebSocket successfully connected");

            foreach (var asset in assets)
            {
                var subRequest = new WssSubscriptionRequest(
                    type: "l1-subscription",
                    id: "1",
                    instrumentId: asset.FintachartsId.ToString(),
                    provider: asset.Provider,
                    subscribe: true,
                    kinds: ["last"]
                );

                string jsonMessage = JsonSerializer.Serialize(subRequest);
                byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(jsonMessage);

                await clientWebSocket.SendAsync(messageBytes, WebSocketMessageType.Text, true, stoppingToken);
            }

            _logger.LogInformation("Sent subscription requests for all assets.");
        }

        private async Task ListenForMessagesAsync(ClientWebSocket clientWebSocket, Dictionary<string, string> idToSymbol, CancellationToken stoppingToken)
        {
            var buffer = new byte[4096];

            while (clientWebSocket.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
            {
                using var memoryStream = new MemoryStream();
                ValueWebSocketReceiveResult result;

                do
                {
                    result = await clientWebSocket.ReceiveAsync(new Memory<byte>(buffer), stoppingToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("WebSocket connection closed");
                        return;
                    }

                    memoryStream.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                byte[] completeMessage = memoryStream.ToArray();
                string jsonMessage = System.Text.Encoding.UTF8.GetString(completeMessage);

                ProcessReceivedMessage(jsonMessage, idToSymbol);
            }
        }

        private void ProcessReceivedMessage(string jsonMessage, Dictionary<string, string> idToSymbol)
        {
            using JsonDocument document = JsonDocument.Parse(jsonMessage);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty("type", out JsonElement typeElement) && typeElement.GetString() == "l1-update")
            {
                string instrumentId = root.GetProperty("instrumentId").GetString()!;

                if (idToSymbol.TryGetValue(instrumentId, out var symbol))
                {
                    if (root.TryGetProperty("last", out JsonElement lastElement))
                    {
                        decimal price = lastElement.GetProperty("price").GetDecimal();
                        DateTime timestamp = lastElement.GetProperty("timestamp").GetDateTime();

                        _marketStateCache.UpdatePrice(symbol, price, timestamp);
                    }
                }
            }
        }

        private record WssSubscriptionRequest(
            string type,
            string id,
            string instrumentId,
            string provider,
            bool subscribe,
            string[] kinds
        );
    }
}