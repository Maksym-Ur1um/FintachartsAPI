using FintachartsAPI.Clients;
using FintachartsAPI.Configuration;
using FintachartsAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace FintachartsAPI.Tests
{
    public class FintachartsAuthServiceTests
    {
        private readonly Mock<IFintachartsApiClient> _mockApiClient;
        private readonly IOptions<FintachartsOptions> _options;
        private readonly IMemoryCache _memoryCache;

        public FintachartsAuthServiceTests()
        {
            _mockApiClient = new Mock<IFintachartsApiClient>();

            _options = Options.Create(new FintachartsOptions
            {
                UserName = "testuser",
                Password = "testpassword"
            });

            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();
            _memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
        }

        [Fact]
        public async Task GetTokenAsync_WhenTokenNotInCache_ShouldFetchFromApiAndCacheIt()
        {
            var expectedToken = "new-api-token";
            var expiresIn = 3600;

            _mockApiClient
                .Setup(c => c.GetTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((expectedToken, expiresIn));

            var authService = new FintachartsAuthService(_mockApiClient.Object, _options, _memoryCache);

            var result = await authService.GetTokenAsync();

            result.Should().NotBeNullOrEmpty();
            result.Should().Be(expectedToken);

            _mockApiClient.Verify(c => c.GetTokenAsync("testuser", "testpassword"), Times.Once);

            bool isCached = _memoryCache.TryGetValue("FintaToken", out string? cachedToken);
            isCached.Should().BeTrue();
            cachedToken.Should().Be(expectedToken);
        }

        [Fact]
        public async Task GetTokenAsync_WhenTokenIsInCache_ShouldReturnCachedTokenWithoutApiCall()
        {
            var cachedToken = "existing-cached-token";
            _memoryCache.Set("FintaToken", cachedToken);

            var authService = new FintachartsAuthService(_mockApiClient.Object, _options, _memoryCache);

            var result = await authService.GetTokenAsync();

            result.Should().Be(cachedToken);

            _mockApiClient.Verify(c => c.GetTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}