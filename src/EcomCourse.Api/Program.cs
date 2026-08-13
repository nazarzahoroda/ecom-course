using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Api.Middleware;

using EcomCourse.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();
builder.Services.AddPersistence(builder.Configuration);
//builder.Services.AddApplication(); waiting for PR

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

app.Run();
