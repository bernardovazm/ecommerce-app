using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Repositories;
using MediatR;

namespace Ecommerce.Application.Products.Queries;

public record SearchProductsQuery(string? SearchTerm, string? Category, int Page = 1, int PageSize = 12) 
    : IRequest<(IEnumerable<Product> Products, int TotalCount)>;

public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, (IEnumerable<Product> Products, int TotalCount)>
{
    private readonly IProductReadRepository _productRepository;

    public SearchProductsQueryHandler(IProductReadRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        return await _productRepository.GetPagedAsync(request.Page, request.PageSize, request.Category, request.SearchTerm);
    }
}
