using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using EcomCourse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Categories;
using EcomCourse.Application.Categories.Commands.Create;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EcomCourse.IntegrationTests.Categories;

public class CategoriesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>  
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CategoriesIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            GenerateAdminToken()
        );
    }

    [Fact]
    public async Task Category_CRUD_HappyPath_ShouldWork()
    {
        // Arrange
        var createCommand = new CreateCategoryCommand("Electronics");
            
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

        Assert.Equal("Electronics", category.Name);

        // UPDATE
        var updateResponse = await _client.PutAsJsonAsync($"/api/categories/{categoryId}", new {name = "Smartphones"});

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // VERIFY UPDATE
        var updatedResponse = await _client.GetAsync($"/api/categories/{categoryId}");

        Assert.Equal(HttpStatusCode.OK, updatedResponse.StatusCode);
            
        var updatedCategory = await updatedResponse.Content.ReadFromJsonAsync<CategoryDto>();   

        Assert.NotNull(updatedCategory);

        Assert.Equal("Smartphones", updatedCategory.Name);

        // DELETE
        var deleteResponse = await _client.DeleteAsync($"/api/categories/{categoryId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // VERIFY DELETE
        var deletedResponse = await _client.GetAsync($"/api/categories/{categoryId}");

        Assert.Equal(HttpStatusCode.NotFound, deletedResponse.StatusCode);   
    }

    [Fact]
    public async Task CreateCategory_WithParentId_ShouldCreateChildCategory()
    {
        // Arrange - create root category
        var rootResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryCommand("Electronics"));

        Assert.Equal(HttpStatusCode.Created, rootResponse.StatusCode);

        var rootId = await rootResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, rootId);

        // Act - create child category
        var childResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryCommand(
                "Smartphones",
                rootId));

        Assert.Equal(HttpStatusCode.Created, childResponse.StatusCode);

        var childId = await childResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, childId);

        // Assert - verify relationship in database
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<EcomCourseDbContext>();

        var childCategory = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == childId);

        Assert.NotNull(childCategory);
        Assert.Equal(rootId, childCategory.ParentId);
    }

    [Fact]
    public async Task CreateCategory_WithNonExistingParentId_ShouldReturnBadRequest()
    {
        // Arrange
        var nonExistingParentId = Guid.NewGuid();

        var command = new CreateCategoryCommand(
            "Smartphones",
            nonExistingParentId);

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/categories",
            command);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_WhenNewParentIsDescendant_ShouldRejectCyclicReference()
    {
        // Arrange - create root category A
        var rootResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryCommand("Electronics"));

        Assert.Equal(HttpStatusCode.Created, rootResponse.StatusCode);

        var rootId = await rootResponse.Content.ReadFromJsonAsync<Guid>();

        // Create child category B
        var childResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryCommand(
                "Phones",
                rootId));

        Assert.Equal(HttpStatusCode.Created, childResponse.StatusCode);

        var childId = await childResponse.Content.ReadFromJsonAsync<Guid>();

        // Create grandchild category C
        var grandchildResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryCommand(
                "Smartphones",
                childId));

        Assert.Equal(HttpStatusCode.Created, grandchildResponse.StatusCode);

        var grandchildId = await grandchildResponse.Content.ReadFromJsonAsync<Guid>();

        // Act - try to make C the parent of A
        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/categories/{rootId}",
            new
            {
                name = "Electronics",
                parentId = grandchildId
            });

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            updateResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<EcomCourseDbContext>();

        var rootCategory = await dbContext.Categories
            .AsNoTracking()
            .FirstAsync(category => category.Id == rootId);

        Assert.Null(rootCategory.ParentId);
    }

    [Fact]
    public async Task UpdateCategory_WithValidParentId_ShouldUpdateParent()
    {
        // Arrange - create root category A
        var parentResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryCommand("Electronics"));

        Assert.Equal(HttpStatusCode.Created, parentResponse.StatusCode);

        var parentId = await parentResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, parentId);

        // Create another root category B
        var categoryResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryCommand("Phones"));

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var categoryId = await categoryResponse.Content.ReadFromJsonAsync<Guid>();

        Assert.NotEqual(Guid.Empty, categoryId);

        // Act - make A the parent of B
        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/categories/{categoryId}",
            new
            {
                name = "Phones",
                parentId
            });

        Assert.Equal(
            HttpStatusCode.NoContent,
            updateResponse.StatusCode);

        // Assert - verify relationship in database
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<EcomCourseDbContext>();

        var updatedCategory = await dbContext.Categories
            .AsNoTracking()
            .FirstAsync(category => category.Id == categoryId);

        Assert.Equal(parentId, updatedCategory.ParentId);
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
    public async Task GetCategoryTree_ShouldReturnNestedHierarchy()
    {
        // Arrange - create root
        var rootResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryCommand("Tree Test Electronics"));

        Assert.Equal(HttpStatusCode.Created, rootResponse.StatusCode);

        var rootId = await rootResponse.Content.ReadFromJsonAsync<Guid>();

        // Create child
        var childResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryCommand(
                "Tree Test Phones",
                rootId));

        Assert.Equal(HttpStatusCode.Created, childResponse.StatusCode);

        var childId = await childResponse.Content.ReadFromJsonAsync<Guid>();

        // Create grandchild
        var grandchildResponse = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryCommand(
                "Tree Test Smartphones",
                childId));

        Assert.Equal(HttpStatusCode.Created, grandchildResponse.StatusCode);

        var grandchildId = await grandchildResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await _client.GetAsync(
            "/api/categories/tree");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tree = await response.Content
            .ReadFromJsonAsync<List<CategoryTreeDto>>();

        Assert.NotNull(tree);

        var root = tree.Single(
            category => category.Id == rootId);

        Assert.Equal("Tree Test Electronics", root.Name);

        var child = root.Children.Single(
            category => category.Id == childId);

        Assert.Equal("Tree Test Phones", child.Name);

        var grandchild = child.Children.Single(
            category => category.Id == grandchildId);

        Assert.Equal("Tree Test Smartphones", grandchild.Name);
        Assert.Empty(grandchild.Children);

    }

    private static string GenerateAdminToken()
    {
        var key = Encoding.UTF8.GetBytes(
            "Very_Super_Puper_Secret_Key123!321");

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new[]
                {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    Guid.NewGuid().ToString()),
                new Claim(
                    ClaimTypes.Role,
                    "Admin"),
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

        return handler.WriteToken(
            handler.CreateToken(descriptor));
    }

}
