using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var electronics = new Category("Electronics");
        var clothing = new Category("Clothing");
        var books = new Category("Books");
        var home = new Category("Home & Garden");

        await context.Categories.AddRangeAsync(electronics, clothing, books, home);
        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            new("Electronic 1", "Mock Product Description", 1.00m, "https://picsum.photos/200", electronics.Id),
            new("Electronic 2", "Mock Product Description", 799.99m, "https://picsum.photos/300", electronics.Id),
            new("Clothing 3", "Mock Product Description", 129.99m, "https://picsum.photos/400", clothing.Id),
            new("Clothing 4", "Mock Product Description", 79.99m, "https://picsum.photos/500", clothing.Id),
            new("Books 5", "Mock Product Description", 49.99m, "https://picsum.photos/600/300", books.Id),
            new("Home 6", "Mock Product Description", 8.99m, "https://picsum.photos/700", home.Id),
            new("Electronic 7", "Mock Product Description", 99.99m, "https://picsum.photos/800", electronics.Id),
            new("Clothing 8", "", 59.99m, "", clothing.Id)
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
