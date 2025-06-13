using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Repositories;
using Ecommerce.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Tests.Infrastructure.Repositories;

[Collection("Test Collection")]
public class ProductRepositoryTests
{
    private readonly TestFixture _fixture;

    public ProductRepositoryTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnProduct()
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

        var repository = new ProductRepository(context);

        // Act
        var result = await repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithProducts_ShouldReturnAllProducts()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext();

        var category = _fixture.Fixture.Create<Category>();
        context.Categories.Add(category);

        var products = _fixture.Fixture.Build<Product>()
            .With(p => p.CategoryId, category.Id)
            .CreateMany(5)
            .ToList();

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(5);
        result.Should().BeEquivalentTo(products, options => options.Including(p => p.Id));
    }

    [Fact]
    public async Task GetByCategoryAsync_WithCategoryProducts_ShouldReturnFilteredProducts()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext();

        var category1 = _fixture.Fixture.Create<Category>();
        var category2 = _fixture.Fixture.Create<Category>();
        context.Categories.AddRange(category1, category2);

        var productsCategory1 = _fixture.Fixture.Build<Product>()
            .With(p => p.CategoryId, category1.Id)
            .CreateMany(3)
            .ToList();

        var productsCategory2 = _fixture.Fixture.Build<Product>()
            .With(p => p.CategoryId, category2.Id)
            .CreateMany(2)
            .ToList();

        context.Products.AddRange(productsCategory1);
        context.Products.AddRange(productsCategory2);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        // Act
        var result = await repository.GetByCategoryAsync(category1.Id);

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(p => p.CategoryId == category1.Id);
    }

    [Fact]
    public async Task AddAsync_WithValidProduct_ShouldAddProduct()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext();

        var category = _fixture.Fixture.Create<Category>();
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = _fixture.Fixture.Build<Product>()
            .With(p => p.CategoryId, category.Id)
            .Create();

        var repository = new ProductRepository(context);

        // Act
        await repository.AddAsync(product);
        await context.SaveChangesAsync();

        // Assert
        var addedProduct = await context.Products.FindAsync(product.Id);
        addedProduct.Should().NotBeNull();
        addedProduct!.Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingProduct_ShouldUpdateProduct()
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

        var repository = new ProductRepository(context);
        var newName = "Updated Product Name";

        // Act
        product.UpdateName(newName);
        repository.Update(product);
        await context.SaveChangesAsync();

        // Assert
        var updatedProduct = await context.Products.FindAsync(product.Id);
        updatedProduct.Should().NotBeNull();
        updatedProduct!.Name.Should().Be(newName);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingProduct_ShouldRemoveProduct()
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

        var repository = new ProductRepository(context);

        // Act
        repository.Delete(product);
        await context.SaveChangesAsync();

        // Assert
        var deletedProduct = await context.Products.FindAsync(product.Id);
        deletedProduct.Should().BeNull();
    }
}
