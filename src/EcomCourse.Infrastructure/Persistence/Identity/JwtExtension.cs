using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EcomCourse.Infrastructure.Persistence.Identity
{
    public static class JwtExtension
    {
        public static IServiceCollection AddJWTAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured.");
            services
           .AddAuthentication(options =>
           {
               options.DefaultAuthenticateScheme =
                   JwtBearerDefaults.AuthenticationScheme;

               options.DefaultChallengeScheme =
                   JwtBearerDefaults.AuthenticationScheme;
           })
           .AddJwtBearer(options =>
           {
               options.TokenValidationParameters =
                   new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,

                       ValidIssuer = configuration["Jwt:Issuer"],
                       ValidAudience = configuration["Jwt:Audience"],

                       IssuerSigningKey =
                           new SymmetricSecurityKey(
                               Encoding.UTF8.GetBytes(jwtKey))
                   };
               options.Events = new JwtBearerEvents
               {
                   OnChallenge = async context =>
                   {
                       context.HandleResponse();

                       context.Response.StatusCode =
                           StatusCodes.Status401Unauthorized;

                       context.Response.ContentType =
                           "application/problem+json";

                       var problem = new ProblemDetails
                       {
                           Title = "Unauthorized",
                           Detail = "Authentication is required to access this resource.",
                           Status = StatusCodes.Status401Unauthorized
                       };

                       await context.Response.WriteAsJsonAsync(problem);
                   },
                   OnForbidden = async context =>
                   {
                       context.Response.StatusCode =
                           StatusCodes.Status403Forbidden;

                       context.Response.ContentType =
                           "application/problem+json";

                       var problem = new ProblemDetails
                       {
                           Title = "Forbidden",
                           Detail = "You do not have permission to access this resource.",
                           Status = StatusCodes.Status403Forbidden
                       };

                       await context.Response.WriteAsJsonAsync(problem);
                   }
               };
           });

            return services;
        }
    }
}
