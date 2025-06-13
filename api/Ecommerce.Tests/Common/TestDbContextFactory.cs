using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ecommerce.Infrastructure.Data;

namespace Ecommerce.Tests.Common;

public class TestDbContextFactory
{
    public static AppDbContext CreateInMemoryContext(string databaseName = null)
    {
        databaseName ??= Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .EnableSensitiveDataLogging()
            .Options;

        return new AppDbContext(options);
    }

    public static async Task<AppDbContext> CreateWithSeedDataAsync(string databaseName = null)
    {
        var context = CreateInMemoryContext(databaseName);
        await SeedTestDataAsync(context);
        return context;
    }

    private static async Task SeedTestDataAsync(AppDbContext context)
    {
        await context.SaveChangesAsync();
    }
}
