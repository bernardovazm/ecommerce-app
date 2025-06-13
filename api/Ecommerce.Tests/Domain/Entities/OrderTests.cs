using Ecommerce.Domain.Entities;

namespace Ecommerce.Tests.Domain.Entities;

[Collection("Test Collection")]
public class OrderTests
{
    private readonly TestFixture _fixture;

    public OrderTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Order_CreateWithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var customerId = _fixture.Fixture.Create<Guid>();
        var customerEmail = _fixture.Fixture.Create<string>();
        var shippingAddress = _fixture.Fixture.Create<string>();

        // Act
        var order = new Order(customerId, customerEmail, shippingAddress);

        // Assert
        order.CustomerId.Should().Be(customerId);
        order.CustomerEmail.Should().Be(customerEmail);
        order.ShippingAddress.Should().Be(shippingAddress);
        order.Status.Should().Be("Pending");
        order.Items.Should().BeEmpty();
        order.TotalAmount.Should().Be(0);
        order.Id.Should().NotBeEmpty();
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Order_AddItem_ShouldAddItemAndUpdateTotal()
    {
        // Arrange
        var order = _fixture.Fixture.Create<Order>();
        var productId = _fixture.Fixture.Create<Guid>();
        var productName = _fixture.Fixture.Create<string>();
        var price = 100m;
        var quantity = 2;

        // Act
        order.AddItem(productId, productName, price, quantity);

        // Assert
        order.Items.Should().HaveCount(1);
        var item = order.Items.First();
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be(productName);
        item.Price.Should().Be(price);
        item.Quantity.Should().Be(quantity);
        order.TotalAmount.Should().Be(price * quantity);
    }

    [Fact]
    public void Order_AddMultipleItems_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var order = _fixture.Fixture.Create<Order>();

        // Act
        order.AddItem(Guid.NewGuid(), "Product 1", 50m, 2);
        order.AddItem(Guid.NewGuid(), "Product 2", 30m, 1);
        order.AddItem(Guid.NewGuid(), "Product 3", 20m, 3);

        // Assert
        order.Items.Should().HaveCount(3);
        order.TotalAmount.Should().Be(190m); // (50*2) + (30*1) + (20*3)
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Order_AddItemWithInvalidQuantity_ShouldThrowArgumentException(int invalidQuantity)
    {
        // Arrange
        var order = _fixture.Fixture.Create<Order>();
        var productId = _fixture.Fixture.Create<Guid>();
        var productName = _fixture.Fixture.Create<string>();
        var price = _fixture.Fixture.Create<decimal>();

        // Act & Assert
        var act = () => order.AddItem(productId, productName, price, invalidQuantity);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Order_AddItemWithNegativePrice_ShouldThrowArgumentException(decimal negativePrice)
    {
        // Arrange
        var order = _fixture.Fixture.Create<Order>();
        var productId = _fixture.Fixture.Create<Guid>();
        var productName = _fixture.Fixture.Create<string>();
        var quantity = _fixture.Fixture.Create<int>();

        // Act & Assert
        var act = () => order.AddItem(productId, productName, negativePrice, quantity);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Order_UpdateStatus_ShouldUpdateSuccessfully()
    {
        // Arrange
        var order = _fixture.Fixture.Create<Order>();
        var newStatus = "Confirmed";

        // Act
        order.UpdateStatus(newStatus);

        // Assert
        order.Status.Should().Be(newStatus);
        order.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Order_UpdateWithInvalidStatus_ShouldThrowArgumentException(string invalidStatus)
    {
        // Arrange
        var order = _fixture.Fixture.Create<Order>();

        // Act & Assert
        var act = () => order.UpdateStatus(invalidStatus);
        act.Should().Throw<ArgumentException>();
    }
}
