using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Interfaces;
using EcomCourse.Application.Products;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Products;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.IntegrationTests.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EcomCourse.IntegrationTests.Products
{
    public class ProductImagesIntegrationTests : IAsyncLifetime
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly FakeBlobStorageService _fakeStorage = new();

        private Guid _productId;

        public ProductImagesIntegrationTests()
        {
            var dbName = $"InMemoryImageDb_{Guid.NewGuid()}";

            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services
                        .AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme =
                                TestAuthenticationHandler.AuthenticationScheme;
                            options.DefaultChallengeScheme =
                                TestAuthenticationHandler.AuthenticationScheme;
                        })
                        .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                            TestAuthenticationHandler.AuthenticationScheme,
                            _ => { }
                        );

                    services.RemoveAll<DbContextOptions<EcomCourseDbContext>>();
                    services.RemoveAll<EcomCourseDbContext>();

                    var inMemoryServiceProvider = new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider();

                    services.AddDbContext<EcomCourseDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(dbName);
                        options.UseInternalServiceProvider(inMemoryServiceProvider);
                    });

                    services.RemoveAll<IBlobStorageService>();
                    services.AddSingleton<IBlobStorageService>(_fakeStorage);
                });
            });

            _client = _factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EcomCourseDbContext>();

            var category = Category.Create("Electronics").Value!;
            dbContext.Categories.Add(category);

            var product = Product
                .Create("Test Phone", 500m, Currency.USD, "PFH-0001", category.Id)
                .Value!;
            _productId = product.Id;
            dbContext.Products.Add(product);

            await dbContext.SaveChangesAsync();
        }

        public Task DisposeAsync()
        {
            _client.Dispose();
            _factory.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task InitiateUpload_ShouldReturnSasUrl_WhenAdmin()
        {
            var request = BuildRequest(
                HttpMethod.Post,
                $"/api/ProductImages/{_productId}/initiate-upload",
                role: "Admin"
            );
            request.Content = JsonContent.Create(
                new InitiateUploadRequest("photo.png", "image/png")
            );

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<InitiateUploadResponse>();
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.UploadUrl));
            Assert.False(string.IsNullOrWhiteSpace(result.BlobName));
            Assert.Contains(".png", result.BlobName);
        }

        [Fact]
        public async Task InitiateUpload_ShouldReturn403_WhenNotAdmin()
        {
            var request = BuildRequest(
                HttpMethod.Post,
                $"/api/ProductImages/{_productId}/initiate-upload",
                role: "Customer"
            );
            request.Content = JsonContent.Create(
                new InitiateUploadRequest("photo.png", "image/png")
            );

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ConfirmUpload_ShouldPersistImageInDatabase_WhenBlobExists()
        {
            var blobName = $"{Guid.NewGuid()}-main.jpg";
            _fakeStorage.RegisterBlob(blobName);

            var request = BuildRequest(
                HttpMethod.Post,
                $"/api/ProductImages/{_productId}/confirm-upload",
                role: "Admin"
            );
            request.Content = JsonContent.Create(
                new ConfirmUploadRequest(blobName, "image/jpeg", IsMain: true)
            );

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<ConfirmResponseDto>();
            Assert.NotNull(body);
            Assert.NotEqual(Guid.Empty, body.ImageId);

            var getResponse = await _client.GetAsync($"/api/ProductImages/{_productId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var images = await getResponse.Content.ReadFromJsonAsync<List<ProductImageDto>>();
            Assert.NotNull(images);
            Assert.Single(images);
            Assert.Equal(body.ImageId, images[0].Id);
            Assert.True(images[0].IsMain);
        }

        [Fact]
        public async Task ConfirmUpload_ShouldReturnError_WhenBlobNotFoundInStorage()
        {
            var nonExistentBlob = "missing-file.jpg";

            var request = BuildRequest(
                HttpMethod.Post,
                $"/api/ProductImages/{_productId}/confirm-upload",
                role: "Admin"
            );
            request.Content = JsonContent.Create(
                new ConfirmUploadRequest(nonExistentBlob, "image/jpeg", IsMain: false)
            );

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteImage_ShouldRemoveImageFromDbAndStorage_WhenAdmin()
        {
            var blobName = $"{Guid.NewGuid()}-delete.webp";
            _fakeStorage.RegisterBlob(blobName);

            var confirmRequest = BuildRequest(
                HttpMethod.Post,
                $"/api/ProductImages/{_productId}/confirm-upload",
                role: "Admin"
            );
            confirmRequest.Content = JsonContent.Create(
                new ConfirmUploadRequest(blobName, "image/webp", IsMain: false)
            );
            var confirmResponse = await _client.SendAsync(confirmRequest);
            var body = await confirmResponse.Content.ReadFromJsonAsync<ConfirmResponseDto>();
            var imageId = body!.ImageId;

            var deleteRequest = BuildRequest(
                HttpMethod.Delete,
                $"/api/ProductImages/{imageId}?productId={_productId}",
                role: "Admin"
            );
            var deleteResponse = await _client.SendAsync(deleteRequest);

            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            Assert.False(await _fakeStorage.ExistsAsync(blobName));

            var getResponse = await _client.GetAsync($"/api/ProductImages/{_productId}");
            var images = await getResponse.Content.ReadFromJsonAsync<List<ProductImageDto>>();
            Assert.NotNull(images);
            Assert.Empty(images);
        }

        private static HttpRequestMessage BuildRequest(
            HttpMethod method,
            string url,
            bool anonymous = false,
            string? role = null
        )
        {
            var request = new HttpRequestMessage(method, url);

            if (anonymous)
            {
                request.Headers.Add("X-Test-Anonymous", "true");
                return request;
            }

            if (role is not null)
            {
                request.Headers.Add("X-Test-Role", role);
            }

            return request;
        }

        private sealed record ConfirmResponseDto(Guid ImageId);

        private sealed class FakeBlobStorageService : IBlobStorageService
        {
            private readonly HashSet<string> _blobs = [];

            public void RegisterBlob(string blobName) => _blobs.Add(blobName);

            public Result<string> GenerateWriteSasUri(
                string blobName,
                string contentType,
                TimeSpan expiresIn
            )
            {
                return Result.Success(
                    $"https://fake.storage.local/{blobName}?sas=dummy_write_token"
                );
            }

            public Result<string> GenerateReadSasUri(string blobName, TimeSpan expiresIn)
            {
                return Result.Success(
                    $"https://fake.storage.local/{blobName}?sas=dummy_read_token"
                );
            }

            public Task<bool> ExistsAsync(
                string blobName,
                CancellationToken cancellationToken = default
            )
            {
                return Task.FromResult(_blobs.Contains(blobName));
            }

            public Task<Result> DeleteAsync(
                string blobName,
                CancellationToken cancellationToken = default
            )
            {
                _blobs.Remove(blobName);
                return Task.FromResult(Result.Success());
            }
        }
    }
}
