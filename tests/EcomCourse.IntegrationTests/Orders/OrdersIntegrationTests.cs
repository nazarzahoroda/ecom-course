using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Orders.Commands.CreateOrder;
using EcomCourse.Application.Orders.Queries.GetOrderWithLines;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EcomCourse.IntegrationTests.Orders;

// Використовуємо WebApplicationFactory для запуску API в тестовому середовищі.
// Примітка: замість Program може бути ваш IAssemblyMarker, або CustomWebApplicationFactory,
// якщо ви використовуєте Testcontainers для підняття реальної БД у тестах.
public class OrdersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_And_GetOrderWithLines_ShouldReturnCorrectData()
    {
        // ==========================================
        // 1. Arrange (Підготовка даних)
        // ==========================================
        var customerId = Guid.NewGuid();
        var command = new CreateOrderCommand(
            customerId,
            new List<OrderLineItemRequest>
            {
                new(Guid.NewGuid(), 2, 100m), // Сума: 200
                new(Guid.NewGuid(), 1, 50m)   // Сума: 50
            });

        // ==========================================
        // 2. Act - Створення замовлення (POST)
        // ==========================================
        var createResponse = await _client.PostAsJsonAsync("/api/orders", command);

        var responseBody = await createResponse.Content.ReadAsStringAsync();

        Console.WriteLine($"STATUS: {createResponse.StatusCode}");
        Console.WriteLine($"BODY: {responseBody}");

        // ==========================================
        // 3. Assert - Перевірка створення
        // ==========================================
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var orderId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, orderId);

        // ==========================================
        // 4. Act - Отримання замовлення (GET)
        // ==========================================
        var getResponse = await _client.GetAsync($"/api/orders/{orderId}");

        // ==========================================
        // 5. Assert - Перевірка отриманих даних
        // ==========================================
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var orderDetails = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(orderDetails);
        Assert.Equal(orderId, orderDetails.Id);
        Assert.Equal(customerId, orderDetails.CustomerId);

        // Перевіряємо обчислювану властивість Total (200 + 50 = 250)
        Assert.Equal(250m, orderDetails.Total);

        // Перевіряємо лінії
        Assert.Equal(2, orderDetails.Lines.Count);

        var firstLine = orderDetails.Lines.First(l => l.Quantity == 2);
        Assert.Equal(100m, firstLine.UnitPrice);
        Assert.Equal(200m, firstLine.LineTotal); // 2 * 100
    }
}