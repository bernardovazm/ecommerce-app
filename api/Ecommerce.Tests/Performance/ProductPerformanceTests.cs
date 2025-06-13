using System.Diagnostics;
using Ecommerce.Tests.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Tests.Performance;

[Collection("Test Collection")]
public class ProductPerformanceTests
{
    private readonly TestFixture _fixture;

    public ProductPerformanceTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetProducts_WithLargeDataset_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext();

        var category = _fixture.Fixture.Create<Category>();
        context.Categories.Add(category);

        // Create 1000 products for performance testing
        var products = _fixture.Fixture.Build<Product>()
            .With(p => p.CategoryId, category.Id)
            .CreateMany(1000)
            .ToList();

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(context);
        var query = new GetProductsQuery();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await handler.Handle(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().HaveCount(1000);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    public async Task CreateMultipleOrders_ShouldHandleConcurrentRequests()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext();

        var category = _fixture.Fixture.Create<Category>();
        context.Categories.Add(category);

        var product = _fixture.Fixture.Build<Product>()
            .With(p => p.CategoryId, category.Id)
            .Create();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var tasks = new List<Task<Guid>>();
        var handler = new CreateOrderCommandHandler(context);

        // Act
        for (int i = 0; i < 10; i++)
        {
            var command = new CreateOrderCommand
            {
                CustomerEmail = $"customer{i}@test.com",
                ShippingAddress = $"Address {i}",
                Items = new List<CreateOrderItemCommand>
                {
                    new()
                    {
                        ProductId = product.Id,
                        Quantity = 1
                    }
                }
            };

            tasks.Add(handler.Handle(command, CancellationToken.None));
        }

        var stopwatch = Stopwatch.StartNew();
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        results.Should().HaveCount(10);
        results.Should().OnlyHaveUniqueItems();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    public async Task QueryProducts_WithDifferentDataSizes_ShouldScaleLinearly(int productCount)
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext();

        var category = _fixture.Fixture.Create<Category>();
        context.Categories.Add(category);

        var products = _fixture.Fixture.Build<Product>()
            .With(p => p.CategoryId, category.Id)
            .CreateMany(productCount)
            .ToList();

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(context);
        var query = new GetProductsQuery();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await handler.Handle(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().HaveCount(productCount);
        // 0.1ms/prod
        var expectedMaxTime = productCount * 0.1;
        stopwatch.ElapsedMilliseconds.Should().BeLessThan((long)expectedMaxTime);
    }
}
