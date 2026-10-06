using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Abstractions;
using EcomCourse.Application.Categories;
using EcomCourse.Application.Products;
using EcomCourse.Domain.Categories;
using EcomCourse.Domain.Common;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Infrastructure.Persistence.Identity;
using EcomCourse.IntegrationTests.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace EcomCourse.IntegrationTests.Categories
{
    public class CategoryImagesIntegrationTests : IAsyncLifetime
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly FakeBlobStorageService _fakeStorage = new();

        private Guid _categoryId;

        public CategoryImagesIntegrationTests()
        {
            var dbName = $"InMemoryCategoryImageDb_{Guid.NewGuid()}";

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

                    var inMemoryServiceProvider = new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider();

                    services.RemoveAll<DbContextOptions<EcomCourseDbContext>>();
                    services.RemoveAll<EcomCourseDbContext>();

                    services.AddDbContext<EcomCourseDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(dbName);
                        options.UseInternalServiceProvider(inMemoryServiceProvider);
                    });

                    services.RemoveAll<DbContextOptions<IdentityDbContext>>();
                    services.RemoveAll<IdentityDbContext>();

                    services.AddDbContext<IdentityDbContext>(options =>
                    {
                        options.UseInMemoryDatabase($"Identity_{dbName}");
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

            var category = Category.Create("Books").Value!;
            _categoryId = category.Id;

            dbContext.Categories.Add(category);
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
                $"/api/CategoryImages/{_categoryId}/initiate-upload",
                role: "Admin"
            );
            request.Content = JsonContent.Create(
                new InitiateUploadRequest("icon.png", "image/png")
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
                $"/api/CategoryImages/{_categoryId}/initiate-upload",
                role: "Customer"
            );
            request.Content = JsonContent.Create(
                new InitiateUploadRequest("icon.png", "image/png")
            );

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ConfirmUpload_ShouldPersistImageInDatabase_WhenBlobExists()
        {
            var blobName = $"{Guid.NewGuid()}-category.jpg";
            _fakeStorage.RegisterBlob(blobName);

            var request = BuildRequest(
                HttpMethod.Post,
                $"/api/CategoryImages/{_categoryId}/confirm-upload",
                role: "Admin"
            );
            request.Content = JsonContent.Create(new ConfirmCategoryImageUploadRequest(blobName));

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var getResponse = await _client.GetAsync($"/api/Categories/{_categoryId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var category = await getResponse.Content.ReadFromJsonAsync<CategoryDto>();
            Assert.NotNull(category);
            Assert.NotNull(category.ImageUrl);
            Assert.Contains(blobName, category.ImageUrl);
        }

        [Fact]
        public async Task ConfirmUpload_ShouldReturnError_WhenBlobNotFoundInStorage()
        {
            var nonExistentBlob = "missing-category-image.jpg";

            var request = BuildRequest(
                HttpMethod.Post,
                $"/api/CategoryImages/{_categoryId}/confirm-upload",
                role: "Admin"
            );
            request.Content = JsonContent.Create(
                new ConfirmCategoryImageUploadRequest(nonExistentBlob)
            );

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteImage_ShouldRemoveImageFromDbAndStorage_WhenAdmin()
        {
            var blobName = $"{Guid.NewGuid()}-category.webp";
            _fakeStorage.RegisterBlob(blobName);

            var confirmRequest = BuildRequest(
                HttpMethod.Post,
                $"/api/CategoryImages/{_categoryId}/confirm-upload",
                role: "Admin"
            );
            confirmRequest.Content = JsonContent.Create(
                new ConfirmCategoryImageUploadRequest(blobName)
            );
            var confirmResponse = await _client.SendAsync(confirmRequest);
            Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

            var deleteRequest = BuildRequest(
                HttpMethod.Delete,
                $"/api/CategoryImages/{_categoryId}?productId={_categoryId}",
                role: "Admin"
            );
            var deleteResponse = await _client.SendAsync(deleteRequest);

            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            Assert.False(await _fakeStorage.ExistsAsync(blobName));

            var getResponse = await _client.GetAsync($"/api/Categories/{_categoryId}");
            var category = await getResponse.Content.ReadFromJsonAsync<CategoryDto>();
            Assert.NotNull(category);
            Assert.Null(category.ImageUrl);
        }

        [Fact]
        public async Task InitiateUpload_ShouldReturnNotFound_WhenCategoryDoesNotExist()
        {
            var nonExistentCategoryId = Guid.NewGuid();

            var request = BuildRequest(
                HttpMethod.Post,
                $"/api/CategoryImages/{nonExistentCategoryId}/initiate-upload",
                role: "Admin"
            );
            request.Content = JsonContent.Create(
                new InitiateUploadRequest("icon.png", "image/png")
            );

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("executable.exe", "application/x-msdownload")]
        [InlineData("script.sh", "application/x-sh")]
        [InlineData("document.pdf", "application/pdf")]
        public async Task InitiateUpload_ShouldReturnBadRequest_WhenExtensionIsInvalid(
            string fileName,
            string contentType
        )
        {
            var request = BuildRequest(
                HttpMethod.Post,
                $"/api/CategoryImages/{_categoryId}/initiate-upload",
                role: "Admin"
            );
            request.Content = JsonContent.Create(new InitiateUploadRequest(fileName, contentType));

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
