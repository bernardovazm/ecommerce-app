using Ecommerce.Domain.Entities;

namespace Ecommerce.Tests.Domain.Entities;

[Collection("Test Collection")]
public class ProductTests
{
    private readonly TestFixture _fixture;

    public ProductTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Product_CreateWithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var name = _fixture.Fixture.Create<string>();
        var description = _fixture.Fixture.Create<string>();
        var price = _fixture.Fixture.Create<decimal>();
        var imageUrl = _fixture.Fixture.Create<string>();
        var categoryId = _fixture.Fixture.Create<Guid>();

        // Act
        var product = new Product(name, description, price, imageUrl, categoryId);

        // Assert
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.Price.Should().Be(price);
        product.ImageUrl.Should().Be(imageUrl);
        product.CategoryId.Should().Be(categoryId);
        product.Id.Should().NotBeEmpty();
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Product_CreateWithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var description = _fixture.Fixture.Create<string>();
        var price = _fixture.Fixture.Create<decimal>();
        var imageUrl = _fixture.Fixture.Create<string>();
        var categoryId = _fixture.Fixture.Create<Guid>();

        // Act & Assert
        var act = () => new Product(invalidName, description, price, imageUrl, categoryId);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Product_CreateWithNegativePrice_ShouldThrowArgumentException(decimal negativePrice)
    {
        // Arrange
        var name = _fixture.Fixture.Create<string>();
        var description = _fixture.Fixture.Create<string>();
        var imageUrl = _fixture.Fixture.Create<string>();
        var categoryId = _fixture.Fixture.Create<Guid>();

        // Act & Assert
        var act = () => new Product(name, description, negativePrice, imageUrl, categoryId);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Product_UpdatePrice_ShouldUpdateSuccessfully()
    {
        // Arrange
        var product = _fixture.Fixture.Create<Product>();
        var newPrice = _fixture.Fixture.Create<decimal>();

        // Act
        product.UpdatePrice(newPrice);

        // Assert
        product.Price.Should().Be(newPrice);
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Product_UpdateWithNegativePrice_ShouldThrowArgumentException(decimal negativePrice)
    {
        // Arrange
        var product = _fixture.Fixture.Create<Product>();

        // Act & Assert
        var act = () => product.UpdatePrice(negativePrice);
        act.Should().Throw<ArgumentException>();
    }
}
