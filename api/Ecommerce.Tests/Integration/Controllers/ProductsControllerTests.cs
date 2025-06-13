using System.Net;
using System.Text;
using System.Text.Json;
using Ecommerce.Api;
using Ecommerce.Tests.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Ecommerce.Tests.Integration.Controllers;

[Collection("Test Collection")]
public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly TestFixture _fixture;

    public ProductsControllerTests(CustomWebApplicationFactory<Program> factory, TestFixture fixture)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _fixture = fixture;
    }

    [Fact]
    public async Task GetProducts_ShouldReturnOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetProductById_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        // First, create a product by getting the context and adding one
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var category = _fixture.Fixture.Create<Category>();
        context.Categories.Add(category);

        var product = _fixture.Fixture.Build<Product>()
            .With(p => p.CategoryId, category.Id)
            .Create();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/products/{product.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var returnedProduct = JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        returnedProduct.Should().NotBeNull();
        returnedProduct!.Id.Should().Be(product.Id);
        returnedProduct.Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/products/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

// DTO classes for test deserialization
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
}
