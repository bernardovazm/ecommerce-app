// Ecommerce.Domain/Entities/Product.cs
namespace Ecommerce.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    protected Product() { }

    public Product(string name, string description, decimal price, string imageUrl, Guid categoryId)
    {
        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
        CategoryId = categoryId;
    }
}