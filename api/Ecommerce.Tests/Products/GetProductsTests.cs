using Ecommerce.Application.Products.Queries;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Tests.Common;

namespace Ecommerce.Tests.Products;

[Collection("Test Collection")]
public class GetProductsTests
{
    private readonly TestFixture _fixture;

    public GetProductsTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_WithExistingProducts_ShouldReturnProducts()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext();

        var category = _fixture.Fixture.Create<Category>();
        context.Categories.Add(category);

        var products = _fixture.Fixture.Build<Product>()
            .With(p => p.CategoryId, category.Id)
            .CreateMany(3)
            .ToList();

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(context);
        var query = new GetProductsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(products, options => options
            .Including(p => p.Id)
            .Including(p => p.Name)
            .Including(p => p.Price));
    }

    [Fact]
    public async Task Handle_WithNoProducts_ShouldReturnEmptyList()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext();
        var handler = new GetProductsQueryHandler(context);
        var query = new GetProductsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldReturnFilteredProducts()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext();

        var category1 = _fixture.Fixture.Create<Category>();
        var category2 = _fixture.Fixture.Create<Category>();
        context.Categories.AddRange(category1, category2);

        var productsCategory1 = _fixture.Fixture.Build<Product>()
            .With(p => p.CategoryId, category1.Id)
            .CreateMany(2)
            .ToList();

        var productsCategory2 = _fixture.Fixture.Build<Product>()
            .With(p => p.CategoryId, category2.Id)
            .CreateMany(3)
            .ToList();

        context.Products.AddRange(productsCategory1);
        context.Products.AddRange(productsCategory2);
        await context.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(context);
        var query = new GetProductsQuery { CategoryId = category1.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.CategoryId == category1.Id);
    }
}