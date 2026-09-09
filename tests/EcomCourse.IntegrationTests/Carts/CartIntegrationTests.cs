using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Products;
using EcomCourse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace EcomCourse.IntegrationTests.Carts;

public class CartIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _httpClient;
    private readonly WebApplicationFactory<Program> _factory;

    public CartIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task CartFlow_ShouldAddUpdateAndRemoveItems()
    {
        var customerId = Guid.NewGuid();

        var categoryResult = Category.Create("Test Category");
        Assert.True(categoryResult.IsSuccess);
        var category = categoryResult.Value;
        Assert.NotNull(category);

        var price1 = Price.Create(100.00m, Currency.USD).Value;
        var sku1 = SKU.Create($"PRD-{Random.Shared.Next(1000, 9999)}").Value;
        Assert.NotNull(price1);
        Assert.NotNull(sku1);

        var product1Result = Product.Create(
            "Product 1",
            price1.Amount,
            price1.Currency,
            sku1.Value,
            category.Id
        );
        Assert.True(product1Result.IsSuccess);
        var product1 = product1Result.Value;
        Assert.NotNull(product1);

        var price2 = Price.Create(200.00m, Currency.USD).Value;
        var sku2 = SKU.Create($"PRD-{Random.Shared.Next(1000, 9999)}").Value;
        Assert.NotNull(price2);
        Assert.NotNull(sku2);

        var product2Result = Product.Create(
            "Product 2",
            price2.Amount,
            price2.Currency,
            sku2.Value,
            category.Id
        );
        Assert.True(product2Result.IsSuccess);
        var product2 = product2Result.Value;
        Assert.NotNull(product2);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();
            db.Categories.Add(category);
            db.Products.AddRange(product1, product2);
            await db.SaveChangesAsync();
        }

        var token = GenerateToken(customerId);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        var addRes1 = await _httpClient.PostAsJsonAsync(
            "/api/Cart/items",
            new AddItemToCartDto { ProductId = product1.Id, Quantity = 2 }
        );
        Assert.Equal(HttpStatusCode.OK, addRes1.StatusCode);

        var addRes2 = await _httpClient.PostAsJsonAsync(
            "/api/Cart/items",
            new AddItemToCartDto { ProductId = product2.Id, Quantity = 3 }
        );
        Assert.Equal(HttpStatusCode.OK, addRes2.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();

            var item1 = await db.CartItems.FirstOrDefaultAsync(x =>
                x.ProductId == product1.Id && x.Cart.CustomerId == customerId
            );
            var item2 = await db.CartItems.FirstOrDefaultAsync(x =>
                x.ProductId == product2.Id && x.Cart.CustomerId == customerId
            );

            Assert.NotNull(item1);
            Assert.NotNull(item2);

            var updateRes = await _httpClient.PutAsJsonAsync(
                $"/api/Cart/items",
                new UpdateCartItemQuantityDto { ProductId = item1.ProductId, Quantity = 5 }
            );
            Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);

            var deleteRes = await _httpClient.DeleteAsync($"/api/Cart/items/{item2.Id}");
            Assert.Equal(HttpStatusCode.OK, deleteRes.StatusCode);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();

            var remainingItems = await db
                .CartItems.Where(x => x.Cart.CustomerId == customerId)
                .ToListAsync();

            Assert.Single(remainingItems);
            Assert.Equal(product1.Id, remainingItems[0].ProductId);
            Assert.Equal(5, remainingItems[0].Quantity);
        }
    }

    private static string GenerateToken(Guid customerId)
    {
        var key = Encoding.UTF8.GetBytes("Very_Super_Puper_Secret_Key123!321");
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new[]
                {
                    new Claim("CustomerId", customerId.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                }
            ),
            Issuer = "EcomCourse",
            Audience = "EcomCourseClient",
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
