using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Repositories;

public interface IProductReadRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    Task<IEnumerable<Product>> SearchByNameAsync(string name);
    Task<IEnumerable<Product>> GetFeaturedProductsAsync();
    Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(int page, int pageSize, string? category = null, string? searchTerm = null);
}