using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var electronics = new Category("Tecnologia");
        var clothing = new Category("Roupas");
        var books = new Category("Leitura");
        var home = new Category("Casa e Jardim");

        await context.Categories.AddRangeAsync(electronics, clothing, books, home);
        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            new("Tech 1", "Descrição do produto", 1.00m, "https://picsum.photos/200", electronics.Id),
            new("Tech 2", "Descrição do produto", 799.99m, "https://picsum.photos/300", electronics.Id),
            new("Acessórios 3", "Descrição do produto", 129.99m, "https://picsum.photos/400", clothing.Id),
            new("Acessórios 4", "Descrição do produto", 79.99m, "https://picsum.photos/500", clothing.Id),
            new("Livros 5", "Descrição do produto", 49.99m, "https://picsum.photos/600/300", books.Id),
            new("Jardim 6", "Descrição do produto", 8.99m, "https://picsum.photos/700", home.Id),
            new("Tech 7", "", 99.99m, "https://picsum.photos/800", electronics.Id),
            new("Acessórios 8", "Produto sem imagem", 59.99m, "", clothing.Id)
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
