using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using SimpleInventoryManagementSystem.Application.Contracts.Responses;

namespace SimpleInventoryManagementSystem.Tests.Integration.Endpoints;

public sealed class OrdersEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    // Fixed GUIDs from DataSeeder — Alice is US (location multiplier 1.0x)
    private static readonly Guid AliceId = new("11111111-0000-0000-0000-000000000001");

    public OrdersEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateProductAsync(int stock, decimal price)
    {
        var body = new { name = $"Test Product {Guid.NewGuid():N}", description = "Test item", price, stock };
        var response = await _client.PostAsJsonAsync("/products", body);
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        return product!.Id;
    }

    [Fact]
    public async Task PostOrders_ValidRequest_Returns201WithOrderDto()
    {
        var productId = await CreateProductAsync(stock: 100, price: 40m);
        var body = new
        {
            customerId = AliceId,
            items = new[] { new { productId, quantity = 1 } }
        };

        var response = await _client.PostAsJsonAsync("/orders", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        order.Should().NotBeNull();
        order!.CustomerId.Should().Be(AliceId);
        order.Items.Should().HaveCount(1);
        order.Items[0].Quantity.Should().Be(1);
        order.TotalAmount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PostOrders_UnknownCustomerId_Returns404()
    {
        var productId = await CreateProductAsync(stock: 10, price: 10m);
        var body = new
        {
            customerId = Guid.NewGuid(),
            items = new[] { new { productId, quantity = 1 } }
        };

        var response = await _client.PostAsJsonAsync("/orders", body);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostOrders_InsufficientStock_Returns409()
    {
        var productId = await CreateProductAsync(stock: 2, price: 10m);
        var body = new
        {
            customerId = AliceId,
            items = new[] { new { productId, quantity = 100 } }
        };

        var response = await _client.PostAsJsonAsync("/orders", body);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostOrders_Qty5_Returns201WithVolumeDiscountApplied()
    {
        // Volume savings ($10*5*0.10=$5) > max holiday savings ($10*0.15=$1.50)
        // Volume discount always wins regardless of the current date
        var productId = await CreateProductAsync(stock: 100, price: 10m);
        var body = new
        {
            customerId = AliceId,
            items = new[] { new { productId, quantity = 5 } }
        };

        var response = await _client.PostAsJsonAsync("/orders", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        order.Should().NotBeNull();
        // 10% volume discount on $10 = $9/unit; US location 1.0x; 5 * $9 = $45
        order!.TotalAmount.Should().Be(45m);
        order.Items[0].FinalUnitPrice.Should().Be(9m);
    }
}
