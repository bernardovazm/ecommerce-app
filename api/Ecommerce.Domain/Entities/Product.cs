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
    public decimal Weight { get; private set; } = 0.1m;
    public decimal Length { get; private set; } = 10m;
    public decimal Width { get; private set; } = 10m;
    public decimal Height { get; private set; } = 5m;

    public Category? Category { get; private set; }

    protected Product() { }

    public Product(string name, string description, decimal price, string imageUrl, Guid categoryId,
        decimal weight = 0.1m, decimal length = 10m, decimal width = 10m, decimal height = 5m)
    {
        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
        CategoryId = categoryId;
        Weight = weight;
        Length = length;
        Width = width;
        Height = height;
    }

    public void UpdateDimensions(decimal weight, decimal length, decimal width, decimal height)
    {
        Weight = weight;
        Length = length;
        Width = width;
        Height = height;
    }
}