using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Categories.Commands.Create;
using EcomCourse.Domain.Products;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EcomCourse.IntegrationTests.Products;

public class ProductsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturnCreated()
    {
        var createCategoryCommand = new CreateCategoryCommand("Product Test Category");

        var categoryResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            createCategoryCommand);

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var categoryId = await categoryResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, categoryId);

        var createProductRequest = new
        {
            Name = "Integration Test Product",
            Amount = 999.99m,
            Currency = Currency.USD,
            SKU = $"{(char)Random.Shared.Next('A', 'Z' + 1)}{(char)Random.Shared.Next('A', 'Z' + 1)}{(char)Random.Shared.Next('A', 'Z' + 1)}-{Random.Shared.Next(10000):D4}",
            CategoryId = categoryId
        };

        var productResponse = await _client.PostAsJsonAsync(
            "/products",
            createProductRequest);

        Assert.Equal(HttpStatusCode.Created, productResponse.StatusCode);

        var productId = await productResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, productId);
    }
}
