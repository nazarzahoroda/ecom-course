using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using EcomCourse.Application.Carts.DTOs;
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
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();

        var token = GenerateToken(customerId);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        var addRes1 = await _httpClient.PostAsJsonAsync(
            "/api/Cart/items",
            new AddItemToCartDto { ProductId = product1Id, Quantity = 2 }
        );
        Assert.Equal(HttpStatusCode.OK, addRes1.StatusCode);

        var addRes2 = await _httpClient.PostAsJsonAsync(
            "/api/Cart/items",
            new AddItemToCartDto { ProductId = product2Id, Quantity = 3 }
        );
        Assert.Equal(HttpStatusCode.OK, addRes2.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();

            var item1 = await db.CartItems.FirstOrDefaultAsync(x =>
                x.ProductId == product1Id && x.Cart.CustomerId == customerId
            );
            var item2 = await db.CartItems.FirstOrDefaultAsync(x =>
                x.ProductId == product2Id && x.Cart.CustomerId == customerId
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
            Assert.Equal(product1Id, remainingItems[0].ProductId);
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
