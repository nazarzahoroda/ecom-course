using System.Security.Claims;
using EcomCourse.Application.Interfaces;
using EcomCourse.Infrastructure.Authorization.Extensions;
using Microsoft.AspNetCore.Http;

namespace EcomCourse.Infrastructure.Services
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId =>
            _httpContextAccessor.HttpContext?.User.GetUserId()
            ?? throw new UnauthorizedAccessException("User context is unavailable");

        public Guid CustomerId =>
            _httpContextAccessor.HttpContext?.User.GetCustomerId()
            ?? throw new UnauthorizedAccessException("User context is unavailable");
        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated
            ?? throw new UnauthorizedAccessException("User context is unavailable");

    }
}
