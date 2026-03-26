using FintachartsAPI.Data;
using FintachartsAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace FintachartsAPI.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();

            var dataService = scope.ServiceProvider.GetRequiredService<IFintachartsDataService>();
            await dataService.InitializeAssetsAsync();
        }
    }
}