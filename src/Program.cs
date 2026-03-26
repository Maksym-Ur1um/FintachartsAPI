using FintachartsAPI.BackgroundServices;
using FintachartsAPI.Clients;
using FintachartsAPI.Configuration;
using FintachartsAPI.Data;
using FintachartsAPI.Extensions;
using FintachartsAPI.Services;
using FintachartsAPI.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MarketStateCache>();
builder.Services.AddHostedService<FintachartsWebSocketListener>();
builder.Services.AddScoped<IFintachartsAuthService, FintachartsAuthService>();
builder.Services.AddScoped<IFintachartsDataService, FintachartsDataService>();
builder.Services.AddScoped<IAssetPriceService, AssetPriceService>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.Configure<FintachartsOptions>(
    builder.Configuration.GetSection(FintachartsOptions.SectionName));

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<IFintachartsApiClient, FintachartsApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<FintachartsOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await app.ApplyDatabaseMigrationsAsync();

using var scope = app.Services.CreateScope();
var dataService = scope.ServiceProvider.GetRequiredService<IFintachartsDataService>();
await dataService.InitializeAssetsAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();