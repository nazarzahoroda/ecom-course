using EcomCourse.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{

    builder.Services.AddInfrastructure(builder.Configuration);

}

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapGet("/", () =>
{
   
    return "Everything is okay";
})
.WithName("GetHealthCheck");

app.Run();
