using System.Net;
using System.Net.Http.Json;
using EcomCourse.Api.Categories;
using EcomCourse.Api.Products;
using EcomCourse.Application.Products;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Products;
using EcomCourse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCourse.IntegrationTests.Products;

public class ProductsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private readonly WebApplicationFactory<Program> _factory;

    public ProductsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
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
    public async Task CreateProduct_WhenCategoryDoesNotExist_ShouldReturnBadRequest()
    {
        var createProductRequest = new CreateProductRequest(
            "Product Without Category",
            100m,
            Currency.USD,
            "CAT-0001",
            Guid.NewGuid());

        var response = await _client.PostAsJsonAsync(
            "/api/products",
            createProductRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WhenSKUAlreadyExists_ShouldReturnBadRequest()
    {
        var createCategoryRequest =
            new CreateCategoryRequest("Duplicate SKU Test Category");

        var categoryResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            createCategoryRequest);

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var categoryId = await categoryResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, categoryId);

        var sku = $"{(char)Random.Shared.Next('A', 'Z' + 1)}{(char)Random.Shared.Next('A', 'Z' + 1)}{(char)Random.Shared.Next('A', 'Z' + 1)}-{Random.Shared.Next(10000):D4}";

        var firstProductRequest = new CreateProductRequest(
            "First Product",
            100m,
            Currency.USD,
            sku,
            categoryId);

        var firstProductResponse = await _client.PostAsJsonAsync(
            "/api/products",
            firstProductRequest);

        Assert.Equal(HttpStatusCode.Created, firstProductResponse.StatusCode);

        var secondProductRequest = new CreateProductRequest(
            "Second Product",
            200m,
            Currency.EUR,
            sku,
            categoryId);

        var secondProductResponse = await _client.PostAsJsonAsync(
            "/api/products",
            secondProductRequest);

        Assert.Equal(HttpStatusCode.BadRequest, secondProductResponse.StatusCode);
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
    public async Task UpdateProduct_WhenCategoryDoesNotExist_ShouldReturnBadRequest()
    {
        var createCategoryRequest =
            new CreateCategoryRequest("Update Missing Category Test");

        var categoryResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            createCategoryRequest);

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var categoryId = await categoryResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, categoryId);

        var sku = $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
                  $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
                  $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
                  $"-{Random.Shared.Next(10000):D4}";

        var createProductRequest = new CreateProductRequest(
            "Product To Update",
            100m,
            Currency.USD,
            sku,
            categoryId);

        var createProductResponse = await _client.PostAsJsonAsync(
            "/api/products",
            createProductRequest);

        Assert.Equal(HttpStatusCode.Created, createProductResponse.StatusCode);

        var productId = await createProductResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, productId);

        var updateProductRequest = new UpdateProductRequest(
            "Updated Product",
            150m,
            Currency.EUR,
            sku,
            Guid.NewGuid());

        var response = await _client.PutAsJsonAsync(
            $"/api/products/{productId}",
            updateProductRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
    public async Task UpdateProduct_WhenSKUAlreadyExists_ShouldReturnBadRequest()
    {
        var createCategoryRequest =
            new CreateCategoryRequest("Update Duplicate SKU Test Category");

        var categoryResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            createCategoryRequest);

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var categoryId = await categoryResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, categoryId);

        var firstSku = $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
                       $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
                       $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
                       $"-{Random.Shared.Next(10000):D4}";

        var secondSku = $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
                        $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
                        $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
                        $"-{Random.Shared.Next(10000):D4}";

        var firstProductRequest = new CreateProductRequest(
            "First Product",
            100m,
            Currency.USD,
            firstSku,
            categoryId);

        var firstProductResponse = await _client.PostAsJsonAsync(
            "/api/products",
            firstProductRequest);

        Assert.Equal(HttpStatusCode.Created, firstProductResponse.StatusCode);

        var secondProductRequest = new CreateProductRequest(
            "Second Product",
            200m,
            Currency.EUR,
            secondSku,
            categoryId);

        var secondProductResponse = await _client.PostAsJsonAsync(
            "/api/products",
            secondProductRequest);

        Assert.Equal(HttpStatusCode.Created, secondProductResponse.StatusCode);

        var secondProductId =
            await secondProductResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, secondProductId);

        var updateProductRequest = new UpdateProductRequest(
            "Updated Second Product",
            250m,
            Currency.USD,
            firstSku,
            categoryId);

        var response = await _client.PutAsJsonAsync(
            $"/api/products/{secondProductId}",
            updateProductRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        var productId = Guid.NewGuid();

        var response = await _client.DeleteAsync(
            $"/api/products/{productId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTopProducts_ShouldReturnAtMostFourProducts()
    {
        var response = await _client.GetAsync(
            "/api/products/top");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content
            .ReadFromJsonAsync<List<ProductDto>>();

        Assert.NotNull(products);
        Assert.True(products.Count <= 4);
    }

    [Fact]
    public async Task GetProducts_WithNameFilter_ShouldReturnMatchingProducts()
    {
        var uniquePart = Guid.NewGuid().ToString("N");

        var categoryResult = Category.Create(
            $"Search Test Category {uniquePart}");

        Assert.True(categoryResult.IsSuccess);

        var category = categoryResult.Value!;

        var sku =
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"-{Random.Shared.Next(10000):D4}";

        var productResult = Product.Create(
            $"Search Product {uniquePart}",
            500m,
            Currency.USD,
            sku,
            category.Id);

        Assert.True(productResult.IsSuccess);

        var product = productResult.Value!;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext =
                scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();

            dbContext.Categories.Add(category);
            dbContext.Products.Add(product);

            await dbContext.SaveChangesAsync();
        }

        var response = await _client.GetAsync(
            $"/api/products?name={uniquePart}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content
            .ReadFromJsonAsync<List<ProductDto>>();

        Assert.NotNull(products);
        Assert.Contains(
            products,
            productDto => productDto.Name == product.Name);
    }

    [Fact]
    public async Task GetProducts_WithCategoryIdFilter_ShouldReturnProductsFromCategory()
    {
        var uniquePart = Guid.NewGuid().ToString("N");

        var categoryAResult = Category.Create(
            $"Category A Filter Test {uniquePart}");

        Assert.True(categoryAResult.IsSuccess);

        var categoryA = categoryAResult.Value!;

        var categoryBResult = Category.Create(
            $"Category B Filter Test {uniquePart}");

        Assert.True(categoryBResult.IsSuccess);

        var categoryB = categoryBResult.Value!;

        var skuA =
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"-{Random.Shared.Next(10000):D4}";

        var skuB =
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"-{Random.Shared.Next(10000):D4}";

        var productAResult = Product.Create(
            $"Product A Filter Test {uniquePart}",
            100m,
            Currency.USD,
            skuA,
            categoryA.Id);

        Assert.True(productAResult.IsSuccess);

        var productA = productAResult.Value!;

        var productBResult = Product.Create(
            $"Product B Filter Test {uniquePart}",
            200m,
            Currency.USD,
            skuB,
            categoryB.Id);

        Assert.True(productBResult.IsSuccess);

        var productB = productBResult.Value!;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext =
                scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();
            dbContext.Categories.AddRange(categoryA, categoryB);
            dbContext.Products.AddRange(productA, productB);
            await dbContext.SaveChangesAsync();
        }

        var response = await _client.GetAsync(
            $"/api/products?categoryId={categoryA.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content
            .ReadFromJsonAsync<List<ProductDto>>();

        Assert.NotNull(products);

        Assert.Contains(
            products,
            productDto => productDto.Id == productA.Id);

        Assert.DoesNotContain(
            products,
            productDto => productDto.Id == productB.Id);
    }

    [Fact]
    public async Task GetProducts_WithMinPriceFilter_ShouldReturnProductsAtOrAboveMinimumPrice()
    {
        var uniquePart = Guid.NewGuid().ToString("N");

        var categoryResult = Category.Create(
            $"Min Price Filter Test {uniquePart}");

        Assert.True(categoryResult.IsSuccess);

        var category = categoryResult.Value!;

        var skuCheap =
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"-{Random.Shared.Next(10000):D4}";

        var skuExpensive =
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"-{Random.Shared.Next(10000):D4}";

        var cheapProductResult = Product.Create(
            $"Cheap Product Filter Test {uniquePart}",
            100m,
            Currency.USD,
            skuCheap,
            category.Id);

        Assert.True(cheapProductResult.IsSuccess);

        var cheapProduct = cheapProductResult.Value!;

        var expensiveProductResult = Product.Create(
            $"Expensive Product Filter Test {uniquePart}",
            500m,
            Currency.USD,
            skuExpensive,
            category.Id);

        Assert.True(expensiveProductResult.IsSuccess);

        var expensiveProduct = expensiveProductResult.Value!;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext =
                scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();
            dbContext.Categories.Add(category);
            dbContext.Products.AddRange(cheapProduct, expensiveProduct);
            await dbContext.SaveChangesAsync();
        }

        var response = await _client.GetAsync(
            "/api/products?minPrice=300");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content
            .ReadFromJsonAsync<List<ProductDto>>();

        Assert.NotNull(products);

        Assert.Contains(
            products,
            productDto => productDto.Id == expensiveProduct.Id);

        Assert.DoesNotContain(
            products,
            productDto => productDto.Id == cheapProduct.Id);
    }

    [Fact]
    public async Task GetProducts_WithMaxPriceFilter_ShouldReturnProductsAtOrBelowMaximumPrice()
    {
        var uniquePart = Guid.NewGuid().ToString("N");

        var categoryResult = Category.Create(
            $"Max Price Filter Test {uniquePart}");

        Assert.True(categoryResult.IsSuccess);

        var category = categoryResult.Value!;

        var skuCheap =
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"-{Random.Shared.Next(10000):D4}";

        var skuExpensive =
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"-{Random.Shared.Next(10000):D4}";

        var cheapProductResult = Product.Create(
            $"Cheap Product Filter Test {uniquePart}",
            100m,
            Currency.USD,
            skuCheap,
            category.Id);

        Assert.True(cheapProductResult.IsSuccess);

        var cheapProduct = cheapProductResult.Value!;

        var expensiveProductResult = Product.Create(
            $"Expensive Product Filter Test {uniquePart}",
            500m,
            Currency.USD,
            skuExpensive,
            category.Id);

        Assert.True(expensiveProductResult.IsSuccess);

        var expensiveProduct = expensiveProductResult.Value!;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext =
                scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();
            dbContext.Categories.Add(category);
            dbContext.Products.AddRange(cheapProduct, expensiveProduct);
            await dbContext.SaveChangesAsync();
        }

        var response = await _client.GetAsync(
            "/api/products?maxPrice=300");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content
            .ReadFromJsonAsync<List<ProductDto>>();

        Assert.NotNull(products);

        Assert.Contains(
            products,
            productDto => productDto.Id == cheapProduct.Id);

        Assert.DoesNotContain(
            products,
            productDto => productDto.Id == expensiveProduct.Id);
    }

    [Fact]
    public async Task GetProducts_WithPriceRange_ShouldReturnProductsWithinInclusiveRange()
    {
        var uniquePart = Guid.NewGuid().ToString("N");

        var categoryResult = Category.Create(
            $"Price Range Filter Test {uniquePart}");

        Assert.True(categoryResult.IsSuccess);

        var category = categoryResult.Value!;

        var skuCheap =
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"-{Random.Shared.Next(10000):D4}";

        var skuMiddle =
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"-{Random.Shared.Next(10000):D4}";

        var skuExpensive =
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"-{Random.Shared.Next(10000):D4}";

        var cheapProductResult = Product.Create(
            $"Cheap Product Filter Test {uniquePart}",
            100m,
            Currency.USD,
            skuCheap,
            category.Id);

        Assert.True(cheapProductResult.IsSuccess);

        var cheapProduct = cheapProductResult.Value!;

        var middleProductResult = Product.Create(
            $"Middle Product Filter Test {uniquePart}",
            300m,
            Currency.USD,
            skuMiddle,
            category.Id);

        Assert.True(middleProductResult.IsSuccess);

        var middleProduct = middleProductResult.Value!;

        var expensiveProductResult = Product.Create(
            $"Expensive Product Filter Test {uniquePart}",
            500m,
            Currency.USD,
            skuExpensive,
            category.Id);

        Assert.True(expensiveProductResult.IsSuccess);

        var expensiveProduct = expensiveProductResult.Value!;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext =
                scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();
            dbContext.Categories.Add(category);
            dbContext.Products.AddRange(cheapProduct, middleProduct, expensiveProduct);
            await dbContext.SaveChangesAsync();
        }

        var response = await _client.GetAsync(
            "/api/products?minPrice=100&maxPrice=300");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content
            .ReadFromJsonAsync<List<ProductDto>>();

        Assert.NotNull(products);

        Assert.Contains(
            products,
            p => p.Id == cheapProduct.Id);

        Assert.Contains(
            products,
            p => p.Id == middleProduct.Id);

        Assert.DoesNotContain(
            products,
            p => p.Id == expensiveProduct.Id);
    }

    [Fact]
    public async Task GetProducts_WithCombinedFilters_ShouldReturnOnlyMatchingProducts()
    {
        var uniquePart = Guid.NewGuid().ToString("N");

        var categoryAResult = Category.Create(
            $"Combined Filter Category A {uniquePart}");

        Assert.True(categoryAResult.IsSuccess);

        var categoryA = categoryAResult.Value!;

        var categoryBResult = Category.Create(
            $"Combined Filter Category B {uniquePart}");

        Assert.True(categoryBResult.IsSuccess);

        var categoryB = categoryBResult.Value!;

        var skuTarget =
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"-{Random.Shared.Next(10000):D4}";

        var skuWrongName =
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"-{Random.Shared.Next(10000):D4}";

        var skuWrongCategory =
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"-{Random.Shared.Next(10000):D4}";

        var skuWrongPrice =
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"{(char)Random.Shared.Next('A', 'Z' + 1)}" +
            $"-{Random.Shared.Next(10000):D4}";

        var targetProductResult = Product.Create(
            $"Target Product {uniquePart}",
            300m,
            Currency.USD,
            skuTarget,
            categoryA.Id);

        Assert.True(targetProductResult.IsSuccess);

        var targetProduct = targetProductResult.Value!;

        var wrongNameProductResult = Product.Create(
            $"Other Product {uniquePart}",
            300m,
            Currency.USD,
            skuWrongName,
            categoryA.Id);

        Assert.True(wrongNameProductResult.IsSuccess);

        var wrongNameProduct = wrongNameProductResult.Value!;

        var wrongCategoryProductResult = Product.Create(
            $"Target Product {uniquePart}",
            300m,
            Currency.USD,
            skuWrongCategory,
            categoryB.Id);

        Assert.True(wrongCategoryProductResult.IsSuccess);

        var wrongCategoryProduct = wrongCategoryProductResult.Value!;

        var wrongPriceProductResult = Product.Create(
            $"Target Product {uniquePart}",
            700m,
            Currency.USD,
            skuWrongPrice,
            categoryA.Id);

        Assert.True(wrongPriceProductResult.IsSuccess);

        var wrongPriceProduct = wrongPriceProductResult.Value!;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext =
                scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();
            dbContext.Categories.AddRange(categoryA, categoryB);
            dbContext.Products.AddRange(
                targetProduct,
                wrongNameProduct,
                wrongCategoryProduct,
                wrongPriceProduct);
            await dbContext.SaveChangesAsync();
        }

        var response = await _client.GetAsync(
            $"/api/products?name=Target%20Product&categoryId={categoryA.Id}&minPrice=200&maxPrice=400");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content
            .ReadFromJsonAsync<List<ProductDto>>();

        Assert.NotNull(products);

        Assert.Contains(
            products,
            p => p.Id == targetProduct.Id);

        Assert.DoesNotContain(
            products,
            p => p.Id == wrongNameProduct.Id);
        Assert.DoesNotContain(
            products,
            p => p.Id == wrongCategoryProduct.Id);
        Assert.DoesNotContain(
            products,
            p => p.Id == wrongPriceProduct.Id);
    }
}
