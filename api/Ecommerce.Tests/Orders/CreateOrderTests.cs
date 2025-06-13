using Ecommerce.Application.Orders.Commands;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Tests.Orders;

[Collection("Test Collection")]
public class CreateOrderTests
{
    private readonly TestFixture _fixture;

    public CreateOrderTests(TestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateOrder()
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

        var command = new CreateOrderCommand
        {
            CustomerEmail = _fixture.Fixture.Create<string>(),
            ShippingAddress = _fixture.Fixture.Create<string>(),
            Items = new List<CreateOrderItemCommand>
            {
                new()
                {
                    ProductId = product.Id,
                    Quantity = 2
                }
            }
        };

        var handler = new CreateOrderCommandHandler(context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == result);

        order.Should().NotBeNull();
        order!.CustomerEmail.Should().Be(command.CustomerEmail);
        order.ShippingAddress.Should().Be(command.ShippingAddress);
        order.Items.Should().HaveCount(1);
        order.Items.First().ProductId.Should().Be(product.Id);
        order.Items.First().Quantity.Should().Be(2);
        order.TotalAmount.Should().Be(product.Price * 2);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldThrowException()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext();

        var command = new CreateOrderCommand
        {
            CustomerEmail = _fixture.Fixture.Create<string>(),
            ShippingAddress = _fixture.Fixture.Create<string>(),
            Items = new List<CreateOrderItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1
                }
            }
        };

        var handler = new CreateOrderCommandHandler(context);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WithMultipleItems_ShouldCreateOrderWithCorrectTotal()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateInMemoryContext();

        var category = _fixture.Fixture.Create<Category>();
        context.Categories.Add(category);

        var product1 = _fixture.Fixture.Build<Product>()
            .With(p => p.CategoryId, category.Id)
            .With(p => p.Price, 50m)
            .Create();

        var product2 = _fixture.Fixture.Build<Product>()
            .With(p => p.CategoryId, category.Id)
            .With(p => p.Price, 30m)
            .Create();

        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        var command = new CreateOrderCommand
        {
            CustomerEmail = _fixture.Fixture.Create<string>(),
            ShippingAddress = _fixture.Fixture.Create<string>(),
            Items = new List<CreateOrderItemCommand>
            {
                new() { ProductId = product1.Id, Quantity = 2 },
                new() { ProductId = product2.Id, Quantity = 3 }
            }
        };

        var handler = new CreateOrderCommandHandler(context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == result);

        order.Should().NotBeNull();
        order!.Items.Should().HaveCount(2);
        // calc (50*2) + (30*3)
        order.TotalAmount.Should().Be(190m);
    }
}
