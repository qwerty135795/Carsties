using AuctionService.Data;
using AuctionService.IntegrationTests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;
public static class ServiceCollectionExtensions
{
    public static void RemoveDbContext<T>(this IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(s =>
                       s.ServiceType == typeof(DbContextOptions<AuctionDbContext>));
        services.Remove(descriptor);
    }
    public static void CreateDb<T>(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var service = scope.ServiceProvider;
        var context = service.GetRequiredService<AuctionDbContext>();
        context.Database.Migrate();
        DbHelper.InitDb(context);
    }
}
