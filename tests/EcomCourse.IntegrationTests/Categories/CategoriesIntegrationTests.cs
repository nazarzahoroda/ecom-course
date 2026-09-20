using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using EcomCourse.Application.Categories;
using EcomCourse.Application.Categories.Commands.Create;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace EcomCourse.IntegrationTests.Categories;

public class CategoriesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>  
{
    private const string _testAuthenticationScheme = "TestScheme";

    private readonly HttpClient _client;

    public CategoriesIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var authenticatedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = _testAuthenticationScheme;
                        options.DefaultChallengeScheme = _testAuthenticationScheme;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                        _testAuthenticationScheme,
                        options => { });
            });
        });

        _client = authenticatedFactory.CreateClient();
    }

    [Fact]
    public async Task Category_CRUD_HappyPath_ShouldWork()
    {
        // Arrange
        var categoryName = $"Electronics-{Guid.NewGuid()}";
        var createCommand = new CreateCategoryCommand(categoryName);

        // CREATE
        var createResponse = await _client.PostAsJsonAsync("/api/categories", createCommand);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var categoryId = await createResponse.Content.ReadFromJsonAsync<Guid>();
            
        Assert.NotEqual(Guid.Empty, categoryId);

        // READ BY ID
        var getResponse = await _client.GetAsync($"/api/categories/{categoryId}");
            
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var category = await getResponse.Content.ReadFromJsonAsync<CategoryDto>();

        Assert.NotNull(category);

        Assert.Equal(categoryName, category.Name);

        // UPDATE
        var updatedCategoryName = $"Smartphones-{Guid.NewGuid()}";

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/categories/{categoryId}",
            new { name = updatedCategoryName });

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // VERIFY UPDATE
        var updatedResponse = await _client.GetAsync($"/api/categories/{categoryId}");

        Assert.Equal(HttpStatusCode.OK, updatedResponse.StatusCode);
            
        var updatedCategory = await updatedResponse.Content.ReadFromJsonAsync<CategoryDto>();   

        Assert.NotNull(updatedCategory);

        Assert.Equal(updatedCategoryName, updatedCategory.Name);

        // DELETE
        var deleteResponse = await _client.DeleteAsync($"/api/categories/{categoryId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // VERIFY DELETE
        var deletedResponse = await _client.GetAsync($"/api/categories/{categoryId}");

        Assert.Equal(HttpStatusCode.NotFound, deletedResponse.StatusCode);   
    }

    [Fact]
    public async Task GetCategoryById_WhenCategoryDoesNotExist_ShouldReturnNotFound()
    {
        var categoryId = Guid.NewGuid();

        var response = await _client.GetAsync(
            $"/api/categories/{categoryId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_WhenCategoryDoesNotExist_ShouldReturnNotFound()
    {
        var categoryId = Guid.NewGuid();

        var response = await _client.PutAsJsonAsync(
            $"/api/categories/{categoryId}",
            new { name = "Electronics" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WhenCategoryDoesNotExist_ShouldReturnNotFound()
    {
        var categoryId = Guid.NewGuid();

        var response = await _client.DeleteAsync(
            $"/api/categories/{categoryId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTopCategories_ShouldReturnAtMostFourCategories()
    {
        var response = await _client.GetAsync(
            "/api/categories/top");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content
            .ReadFromJsonAsync<List<CategoryDto>>();

        Assert.NotNull(categories);
        Assert.True(categories.Count <= 4);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateName_ShouldReturnBadRequest()
    {
        var categoryName = $"Duplicate-{Guid.NewGuid()}";

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryCommand(categoryName));

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        var secondResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryCommand(categoryName));

        Assert.Equal(
            HttpStatusCode.BadRequest,
            secondResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_WithDuplicateName_ShouldReturnBadRequest()
    {
        var firstName = $"Category-{Guid.NewGuid()}";
        var secondName = $"Category-{Guid.NewGuid()}";

        var firstCreateResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryCommand(firstName));

        Assert.Equal(
            HttpStatusCode.Created,
            firstCreateResponse.StatusCode);

        var secondCreateResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryCommand(secondName));

        Assert.Equal(
            HttpStatusCode.Created,
            secondCreateResponse.StatusCode);

        var secondCategoryId = await secondCreateResponse.Content
            .ReadFromJsonAsync<Guid>();

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/categories/{secondCategoryId}",
            new { name = firstName });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            updateResponse.StatusCode);
    }

    private sealed class TestAuthenticationHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
            new Claim(
                ClaimTypes.NameIdentifier,
                Guid.NewGuid().ToString()),
            new Claim(
                ClaimTypes.Role,
                "Admin"),
        };

            var identity = new ClaimsIdentity(
                claims,
                _testAuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(
                principal,
                _testAuthenticationScheme);

            return Task.FromResult(
                AuthenticateResult.Success(ticket));
        }
    }
}
