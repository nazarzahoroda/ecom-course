using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Api.Middleware;
using EcomCourse.Application;
using EcomCourse.Infrastructure;

using EcomCourse.Application.Orders.Commands.CreateOrder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
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

app.MapControllers();

app.MapGet("/", () =>
{

    return "Everything is okay";
})
.WithName("GetHealthCheck");

app.MapControllers();

app.Run();
