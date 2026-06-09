using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using SimpleInventoryManagementSystem.Application.Contracts.Responses;

namespace SimpleInventoryManagementSystem.Tests.Integration.Endpoints;

public sealed class ProductsEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProductsEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_AfterSeeding_Returns200WithSeededProducts()
    {
        var response = await _client.GetAsync("/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>(JsonOptions);
        paged.Should().NotBeNull();
        paged!.Items.Count.Should().BeGreaterThanOrEqualTo(5);
        paged.PageNumber.Should().Be(1);
        paged.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetProducts_PageNumberZero_Returns400()
    {
        var response = await _client.GetAsync("/products?pageNumber=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProducts_PageSizeExceedsMaximum_Returns400()
    {
        var response = await _client.GetAsync("/products?pageSize=101");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostProducts_ValidRequest_Returns201WithProductDto()
    {
        var body = new { name = "Integration Test Mug", description = "Test description", price = 9.99m, stock = 10 };

        var response = await _client.PostAsJsonAsync("/products", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        product.Should().NotBeNull();
        product!.Name.Should().Be("Integration Test Mug");
        product.Price.Should().Be(9.99m);
        product.Stock.Should().Be(10);
        product.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PostProducts_EmptyName_Returns400WithNameValidationError()
    {
        var body = new { name = "", description = "Description", price = 9.99m, stock = 5 };

        var response = await _client.PostAsJsonAsync("/products", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("Name");
    }

    [Fact]
    public async Task PostProducts_NameTooLong_Returns400()
    {
        var body = new { name = new string('a', 51), description = "Valid description", price = 9.99m, stock = 5 };

        var response = await _client.PostAsJsonAsync("/products", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostProducts_DescriptionTooLong_Returns400()
    {
        var body = new { name = "Valid Name", description = new string('d', 51), price = 9.99m, stock = 5 };

        var response = await _client.PostAsJsonAsync("/products", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostProducts_ZeroPrice_Returns400()
    {
        var body = new { name = "Valid Name", description = "Valid description", price = 0m, stock = 5 };

        var response = await _client.PostAsJsonAsync("/products", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostProducts_NegativeStock_Returns400()
    {
        var body = new { name = "Valid Name", description = "Valid description", price = 9.99m, stock = -1 };

        var response = await _client.PostAsJsonAsync("/products", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
