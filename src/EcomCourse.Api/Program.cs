using EcomCourse.Api.Middleware;

using EcomCourse.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{

    builder.Services.AddInfrastructure(builder.Configuration);

}
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


app.MapGet("/", () =>
{
   
    return "Everything is okay";
})
.WithName("GetHealthCheck");

app.Run();
