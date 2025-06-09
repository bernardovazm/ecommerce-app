using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Repositories;
using MediatR;

namespace Ecommerce.Application.Products.Queries;

public record GetFeaturedProductsQuery : IRequest<IEnumerable<Product>>;

public class GetFeaturedProductsQueryHandler : IRequestHandler<GetFeaturedProductsQuery, IEnumerable<Product>>
{
    private readonly IProductReadRepository _productRepository;

    public GetFeaturedProductsQueryHandler(IProductReadRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<Product>> Handle(GetFeaturedProductsQuery request, CancellationToken cancellationToken)
    {
        return await _productRepository.GetFeaturedProductsAsync();
    }
}
