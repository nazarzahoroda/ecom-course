using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Carts.Commands.AddItemToCartCommand;
using EcomCourse.Application.Carts.DTOs;
using EcomCourse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCourse.IntegrationTests.Carts
{
    public class CartIntegrationTests: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _httpClient;
        private readonly EcomCourseDbContext _context;

        public CartIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _httpClient = factory.CreateClient();
            var scope = factory.Services.CreateScope();
            _context = scope.ServiceProvider
                .GetRequiredService<EcomCourseDbContext>();
        }
        [Fact]
        public async Task CartFlow_ShouldAddUpdateAndRemoveItems()
        {
            var customerId = Guid.NewGuid();
            var product1Id = Guid.NewGuid();
            var product2Id = Guid.NewGuid();

            var addItem1 = new AddItemToCartDto(product1Id, 2);

            var response1 = await _httpClient.PostAsJsonAsync($"/api/Cart/items?customerId={customerId}", addItem1);

            response1.EnsureSuccessStatusCode();

            var addItem2 = new AddItemToCartDto(product2Id, 3);

            var response2 = await _httpClient.PostAsJsonAsync($"/api/Cart/items?customerId={customerId}", addItem2);

            response2.EnsureSuccessStatusCode();

            var updateCommand = new AddItemToCartDto(product1Id, 5);

            var updateResponse = await _httpClient.PostAsJsonAsync($"/api/Cart/items?customerId={customerId}", updateCommand);

            updateResponse.EnsureSuccessStatusCode();

            var cartItem = await _context.CartItems.SingleOrDefaultAsync(x => x.ProductId == product2Id
            && x.Cart.CustomerId == customerId);

            var removeResponse = await _httpClient.DeleteAsync($"/api/Cart/items/{cartItem!.Id}");

            removeResponse.EnsureSuccessStatusCode();
        }
    }
}
