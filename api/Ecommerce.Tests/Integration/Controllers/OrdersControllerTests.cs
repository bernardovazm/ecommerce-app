using System.Net;
using System.Text;
using System.Text.Json;
using Ecommerce.Api;
using Ecommerce.Tests.Common;

namespace Ecommerce.Tests.Integration.Controllers;

[Collection("Test Collection")]
public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly TestFixture _fixture;

    public OrdersControllerTests(CustomWebApplicationFactory<Program> factory, TestFixture fixture)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var category = _fixture.Fixture.Create<Category>();
        context.Categories.Add(category);

        var product = _fixture.Fixture.Build<Product>()
            .With(p => p.CategoryId, category.Id)
            .Create();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var createOrderRequest = new CreateOrderRequest
        {
            CustomerEmail = "test@example.com",
            ShippingAddress = "123 Test Street",
            Items = new List<CreateOrderItemRequest>
            {
                new()
                {
                    ProductId = product.Id,
                    Quantity = 2
                }
            }
        };

        var json = JsonSerializer.Serialize(createOrderRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdOrder = JsonSerializer.Deserialize<OrderDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        createdOrder.Should().NotBeNull();
        createdOrder!.Id.Should().NotBeEmpty();
        createdOrder.CustomerEmail.Should().Be(createOrderRequest.CustomerEmail);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var createOrderRequest = new CreateOrderRequest
        {
            CustomerEmail = "",
            ShippingAddress = "123 Test Street",
            Items = new List<CreateOrderItemRequest>()
        };

        var json = JsonSerializer.Serialize(createOrderRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrderById_WithValidId_ShouldReturnOrder()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var order = _fixture.Fixture.Create<Order>();
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/orders/{order.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var returnedOrder = JsonSerializer.Deserialize<OrderDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        returnedOrder.Should().NotBeNull();
        returnedOrder!.Id.Should().Be(order.Id);
    }
}

// Request/Response DTOs for tests
public class CreateOrderRequest
{
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
