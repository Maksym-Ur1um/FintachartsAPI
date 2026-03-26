using FintachartsAPI.BackgroundServices;
using FintachartsAPI.Clients;
using FintachartsAPI.Configuration;
using FintachartsAPI.Data;
using FintachartsAPI.Services;
using FintachartsAPI.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FintachartsAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FintachartsOptions>(
                configuration.GetSection(FintachartsOptions.SectionName));

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddMemoryCache();
            services.AddSingleton<MarketStateCache>();

            services.AddHttpClient<IFintachartsApiClient, FintachartsApiClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<FintachartsOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });

            services.AddScoped<IFintachartsAuthService, FintachartsAuthService>();
            services.AddScoped<IFintachartsDataService, FintachartsDataService>();
            services.AddScoped<IAssetPriceService, AssetPriceService>();

            services.AddHostedService<FintachartsWebSocketListener>();

            return services;
        }
    }
}