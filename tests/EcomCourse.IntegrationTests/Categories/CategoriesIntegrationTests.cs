using System.Net;
using System.Net.Http.Json;
using EcomCourse.Application.Categories;
using EcomCourse.Application.Categories.Commands.Create;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EcomCourse.IntegrationTests.Categories;

public class CategoriesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>  
{
    private readonly HttpClient _client;

    public CategoriesIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
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
}
