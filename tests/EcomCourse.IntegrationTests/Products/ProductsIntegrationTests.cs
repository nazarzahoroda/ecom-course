using System.Net;
using System.Net.Http.Json;
using EcomCourse.Api.Categories;
using EcomCourse.Api.Products;
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
        var createCategoryRequest =
            new CreateCategoryRequest("Product Test Category");

        var categoryResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            createCategoryRequest);

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var categoryId = await categoryResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, categoryId);

        var createProductRequest = new CreateProductRequest(
            "Integration Test Product",
            999.99m,
            Currency.USD,
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}{(char)Random.Shared.Next('A', 'Z' + 1)}{(char)Random.Shared.Next('A', 'Z' + 1)}-{Random.Shared.Next(10000):D4}",
            categoryId);

        var productResponse = await _client.PostAsJsonAsync(
            "/api/products",
            createProductRequest);

        Assert.Equal(HttpStatusCode.Created, productResponse.StatusCode);

        var productId = await productResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, productId);

        var getResponse = await _client.GetAsync(
            $"/api/products/{productId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var product = await getResponse.Content.ReadFromJsonAsync<ProductDto>();

        Assert.NotNull(product);
        Assert.Equal(productId, product.Id);
        Assert.Equal("Integration Test Product", product.Name);
        Assert.Equal(999.99m, product.Amount);
        Assert.Equal(Currency.USD, product.Currency);
        Assert.Equal(createProductRequest.SKU, product.SKU);
        Assert.Equal(categoryId, product.CategoryId);

        var getAllResponse = await _client.GetAsync(
            "/api/products");

        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);

        var products = await getAllResponse.Content
            .ReadFromJsonAsync<List<ProductDto>>();

        Assert.NotNull(products);
        Assert.Contains(products, product => product.Id == productId);

        var updateProductRequest = new UpdateProductRequest(
            "Updated Integration Test Product",
            1299.99m,
            Currency.EUR,
            createProductRequest.SKU,
            categoryId);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/products/{productId}",
            updateProductRequest);

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getUpdatedResponse = await _client.GetAsync(
            $"/api/products/{productId}");

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

        var deleteResponse = await _client.DeleteAsync(
            $"/api/products/{productId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getDeletedResponse = await _client.GetAsync(
            $"/api/products/{productId}");

        Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);
    }

    [Fact]
    public async Task GetProductById_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        var productId = Guid.NewGuid();

        var response = await _client.GetAsync(
            $"/api/products/{productId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        var productId = Guid.NewGuid();

        var updateProductRequest = new UpdateProductRequest(
            "Updated Product",
            1299.99m,
            Currency.EUR,
            "UPD-0001",
            Guid.NewGuid());

        var response = await _client.PutAsJsonAsync(
            $"/api/products/{productId}",
            updateProductRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        var productId = Guid.NewGuid();

        var response = await _client.DeleteAsync(
            $"/api/products/{productId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
