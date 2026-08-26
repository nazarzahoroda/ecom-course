using EcomCourse.Api.Middleware;
using EcomCourse.Application;
using EcomCourse.Infrastructure;
using EcomCourse.Infrastructure.Persistence.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
//var jwtKey = builder.Configuration["Jwt:Key"]!;

//builder.Services
//    .AddAuthentication(options =>
//    {
//        options.DefaultAuthenticateScheme =
//            JwtBearerDefaults.AuthenticationScheme;

//        options.DefaultChallengeScheme =
//            JwtBearerDefaults.AuthenticationScheme;
//    })
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters =
//            new TokenValidationParameters
//            {
//                ValidateIssuer = true,
//                ValidateAudience = true,
//                ValidateLifetime = true,
//                ValidateIssuerSigningKey = true,

//                ValidIssuer =
//                    builder.Configuration["Jwt:Issuer"],

//                ValidAudience =
//                    builder.Configuration["Jwt:Audience"],

//                IssuerSigningKey =
//                    new SymmetricSecurityKey(
//                        Encoding.UTF8.GetBytes(jwtKey))
//            };
//    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
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


app.MapGet("/", () =>
{

    return "Everything is okay";
})
.WithName("GetHealthCheck");

app.MapControllers();

app.Run();
