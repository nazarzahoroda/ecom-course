using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Products;
using EcomCourse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace EcomCourse.IntegrationTests.Carts;

public class CartCheckoutIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public CartCheckoutIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Checkout_WhenCartIsEmpty_ReturnsBadRequestDomainError()
    {
        var customerId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            GenerateToken(customerId)
        );

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();

            var cart = new Cart(Guid.NewGuid(), customerId);
            db.Carts.Add(cart);
            await db.SaveChangesAsync();
        }

        var response = await _client.PostAsync("/api/Cart/checkout", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_FullFlow_Success_UpdatesCartAndSetsJwtCustomerId()
    {
        var customerId = Guid.NewGuid();

        var categoryResult = Category.Create("Test Category");
        var category = categoryResult.Value!;

        var priceResult = Price.Create(100.00m, Currency.USD);
        Assert.True(priceResult.IsSuccess);

        var randomSku = $"PRD-{Random.Shared.Next(1000, 9999)}";

        var skuResult = SKU.Create(randomSku);
        Assert.True(skuResult.IsSuccess);

        var productResult = Product.Create(
            "Sample Product",
            priceResult.Value!.Amount,
            priceResult.Value!.Currency,
            skuResult.Value!.Value,
            category.Id
        );
        Assert.True(productResult.IsSuccess);
        var product = productResult.Value!;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();
            db.Categories.Add(category);
            db.Products.Add(product);
            await db.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            GenerateToken(customerId)
        );

        var addRes = await _client.PostAsJsonAsync(
            "/api/Cart/items",
            new AddItemToCartDto { ProductId = product.Id, Quantity = 2 }
        );
        Assert.Equal(HttpStatusCode.OK, addRes.StatusCode);

        var checkoutRes = await _client.PostAsync("/api/Cart/checkout", null);
        Assert.Equal(HttpStatusCode.OK, checkoutRes.StatusCode);

        var orderId = await checkoutRes.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, orderId);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();

            var cart = await db
                .Carts.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            Assert.NotNull(cart);
            Assert.Equal(CartStatus.CheckedOut, cart.Status);

            var order = await db
                .Orders.Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            Assert.NotNull(order);
            Assert.Equal(customerId, order.CustomerId);
            Assert.Single(order.Lines);
            Assert.Equal(product.Id, order.Lines.First().ProductId);
            Assert.Equal(2, order.Lines.First().Quantity);
            Assert.Equal(100.00m, order.Lines.First().UnitPrice);
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
                    new Claim(ClaimTypes.Role, "Customer"),
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
