using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Api.Middleware;

using EcomCourse.Infrastructure;

using EcomCourse.Application.Orders.Commands.CreateOrder;
using EcomCourse.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/", () =>
{

    return "Everything is okay";
})
.WithName("GetHealthCheck");

app.MapControllers();

app.Run();
