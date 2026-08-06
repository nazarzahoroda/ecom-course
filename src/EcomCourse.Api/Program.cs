using EcomCourse.Api;

var userId = new CreateUserCommand("Ivan", "ivan@example.com");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapGet("/", () =>
{

    return userId;
})
.WithName("GetHealthCheck");

app.Run();
