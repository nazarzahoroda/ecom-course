using EcomCourse.Api.Middleware;
using EcomCourse.Application;
using EcomCourse.Infrastructure;
using EcomCourse.Infrastructure.Persistence.Identity;
using EcomCourse.Infrastructure.Persistence.Identity.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

const string ClientCorsPolicy = "Client";

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "SameCustomerOrAdmin",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.AddRequirements(
                new SameCustomerOrAdminRequirement());
        });
});

builder.Services.AddScoped<IAuthorizationHandler, SameCustomerOrAdminHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy(ClientCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token."
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();
await app.SeedIdentityAsync();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(ClientCorsPolicy);

app.MapGet("/", () =>
{
    return "Everything is okay";
})
.WithName("GetHealthCheck");

app.MapControllers();

app.Run();

