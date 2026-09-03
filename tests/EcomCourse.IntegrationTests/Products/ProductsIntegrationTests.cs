using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Categories.Commands.Create;
using EcomCourse.Application.Products;
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

        var getResponse = await _client.GetAsync(
            $"/products/{productId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var product = await getResponse.Content.ReadFromJsonAsync<ProductDto>();

        Assert.NotNull(product);
        Assert.Equal(productId, product.Id);
        Assert.Equal("Integration Test Product", product.Name);
        Assert.Equal(999.99m, product.Amount);
        Assert.Equal(Currency.USD, product.Currency);
        Assert.Equal(createProductRequest.SKU, product.SKU);
        Assert.Equal(categoryId, product.CategoryId);

        var getAllResponse = await _client.GetAsync("/products");

        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);

        var products = await getAllResponse.Content
            .ReadFromJsonAsync<List<ProductDto>>();

        Assert.NotNull(products);
        Assert.Contains(products, product => product.Id == productId);

        var updateProductRequest = new
        {
            Name = "Updated Integration Test Product",
            Amount = 1299.99m,
            Currency = Currency.EUR,
            SKU = createProductRequest.SKU,
            CategoryId = categoryId
        };

        var updateResponse = await _client.PutAsJsonAsync(
            $"/products/{productId}",
            updateProductRequest);

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getUpdatedResponse = await _client.GetAsync(
            $"/products/{productId}");

        Assert.Equal(HttpStatusCode.OK, getUpdatedResponse.StatusCode);

        var updatedProduct = await getUpdatedResponse.Content
            .ReadFromJsonAsync<ProductDto>();

        Assert.NotNull(updatedProduct);
        Assert.Equal(productId, updatedProduct.Id);
        Assert.Equal("Updated Integration Test Product", updatedProduct.Name);
        Assert.Equal(1299.99m, updatedProduct.Amount);
        Assert.Equal(Currency.EUR, updatedProduct.Currency);
        Assert.Equal(createProductRequest.SKU, updatedProduct.SKU);
        Assert.Equal(categoryId, updatedProduct.CategoryId);
    }

    [Fact]
    public async Task GetProductById_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        var productId = Guid.NewGuid();

        var response = await _client.GetAsync(
            $"/products/{productId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
