using Ardalis.GuardClauses;
namespace Ecommerce.Domain.Entities;
using Ecommerce.Domain.Common;

public sealed class Category : AuditableEntity<Guid>
{
    public string Name { get; private set; } = default!;
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products;

    private Category() { }

    public Category(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Name = name;
    }

    public void AddProduct(Product product)
    {
        Guard.Against.Null(product);
        _products.Add(product);
    }
}
