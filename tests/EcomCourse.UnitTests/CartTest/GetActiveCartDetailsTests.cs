using EcomCourse.Application.Interfaces;
using EcomCourse.Domain.Carts;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Products;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EcomCourse.UnitTests.CartTest
{
    public class GetActiveCartDetailsTests
    {
        private readonly Mock<IUserContext> _currentUserServiceMock = new();
        private readonly DbContextOptions<EcomCourseDbContext> _dbOptions;

        public GetActiveCartDetailsTests()
        {
            _dbOptions = new DbContextOptionsBuilder<EcomCourseDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetActiveCartDetailsAsync_WhenNoActiveCart_ReturnsEmptyCartDetails()
        {
            var customerId = Guid.NewGuid();
            _currentUserServiceMock.Setup(x => x.CustomerId).Returns(customerId);

            await using var context = new EcomCourseDbContext(_dbOptions);
            var service = new CartService(context, _currentUserServiceMock.Object);

            var result = await service.GetActiveCartDetailsAsync(CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Guid.Empty, result.Value!.CartId);
            Assert.Empty(result.Value.Items);
            Assert.Equal(0m, result.Value.TotalAmount);
        }

        [Fact]
        public async Task GetActiveCartDetailsAsync_WhenCartIsEmpty_ReturnsEmptyCartDetails()
        {
            var customerId = Guid.NewGuid();
            _currentUserServiceMock.Setup(x => x.CustomerId).Returns(customerId);

            await using (var setupContext = new EcomCourseDbContext(_dbOptions))
            {
                var emptyCart = new Cart(Guid.NewGuid(), customerId);
                setupContext.Carts.Add(emptyCart);
                await setupContext.SaveChangesAsync();
            }

            await using var context = new EcomCourseDbContext(_dbOptions);
            var service = new CartService(context, _currentUserServiceMock.Object);

            var result = await service.GetActiveCartDetailsAsync(CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Guid.Empty, result.Value!.CartId);
            Assert.Empty(result.Value.Items);
            Assert.Equal(0m, result.Value.TotalAmount);
        }

        [Fact]
        public async Task GetActiveCartDetailsAsync_HappyPath_ReturnsEnrichedCartWithTotal()
        {
            var customerId = Guid.NewGuid();
            _currentUserServiceMock.Setup(x => x.CustomerId).Returns(customerId);

            var category = Category.Create("Electronics").Value!;
            var price = Price.Create(50.00m, Currency.USD).Value!;
            var sku = SKU.Create("SKU-1001").Value!;
            var product = Product
                .Create("Product 1", price.Amount, price.Currency, sku.Value, category.Id)
                .Value!;

            var cart = new Cart(Guid.NewGuid(), customerId);
            cart.AddItem(product.Id, 2);

            await using (var setupContext = new EcomCourseDbContext(_dbOptions))
            {
                setupContext.Categories.Add(category);
                setupContext.Products.Add(product);
                setupContext.Carts.Add(cart);
                await setupContext.SaveChangesAsync();
            }

            await using var context = new EcomCourseDbContext(_dbOptions);
            var service = new CartService(context, _currentUserServiceMock.Object);

            var result = await service.GetActiveCartDetailsAsync(CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(cart.Id, result.Value!.CartId);
            Assert.Single(result.Value.Items);

            var item = result.Value.Items.First();
            Assert.Equal(product.Id, item.ProductId);
            Assert.Equal("Product 1", item.Name);
            Assert.Equal("SKU-1001", item.Sku);
            Assert.Equal(50.00m, item.UnitPrice);
            Assert.Equal(2, item.Quantity);
            Assert.Equal(100.00m, result.Value.TotalAmount);
        }

        [Fact]
        public async Task GetActiveCartDetailsAsync_WhenProductMissingInCatalog_ReturnsFailure()
        {
            var customerId = Guid.NewGuid();
            _currentUserServiceMock.Setup(x => x.CustomerId).Returns(customerId);

            var category = Category.Create("Clothes").Value;
            Assert.NotNull(category);

            var price = Price.Create(30.00m, Currency.USD).Value;
            Assert.NotNull(price);

            var sku = SKU.Create($"SKU-{Random.Shared.Next(1000, 9999)}").Value;
            Assert.NotNull(sku);

            var activeProduct = Product
                .Create("Existing Product", price.Amount, price.Currency, sku.Value, category.Id)
                .Value;
            Assert.NotNull(activeProduct);

            var deletedProductId = Guid.NewGuid();

            var cart = new Cart(Guid.NewGuid(), customerId);
            cart.AddItem(activeProduct.Id, 1);
            cart.AddItem(deletedProductId, 3);

            await using (var setupContext = new EcomCourseDbContext(_dbOptions))
            {
                setupContext.Categories.Add(category);
                setupContext.Products.Add(activeProduct);
                setupContext.Carts.Add(cart);
                await setupContext.SaveChangesAsync();
            }

            await using var context = new EcomCourseDbContext(_dbOptions);
            var service = new CartService(context, _currentUserServiceMock.Object);

            var result = await service.GetActiveCartDetailsAsync(CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ProductErrors.Unavailable.Code, result.Error.Code);
        }
    }
}
