using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using EcomCourse.Application.Authentication.DTOs;
using EcomCourse.Application.Abstractions.Authentication;
using EcomCourse.Infrastructure.Persistence.Identity;
using EcomCourse.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EcomCourse.UnitTests
{
    public class IdentityProviderTests : IDisposable
    {
        private readonly IdentityDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<ITokenIssuer> _tokenIssuerMock = new();
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly IdentityProvider _sut;

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }

        public IdentityProviderTests()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new IdentityDbContext(options);

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!
            );

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var userClaimsPrincipalFactoryMock =
                new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                contextAccessorMock.Object,
                userClaimsPrincipalFactoryMock.Object,
                null!,
                null!,
                null!,
                null!
            );

            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x.Value).Returns("7");
            _configurationMock
                .Setup(c => c.GetSection("Jwt:RefreshTokenDays"))
                .Returns(configSectionMock.Object);
            _configurationMock.Setup(c => c["Jwt:RefreshTokenDays"]).Returns("7");

            _sut = new IdentityProvider(
                _userManagerMock.Object,
                _context,
                _signInManagerMock.Object,
                _tokenIssuerMock.Object,
                _configurationMock.Object
            );
        }

        [Fact]
        public async Task CheckRefreshToken_WhenTokenValid_ShouldRotateAndLinkOldTokenWithNewHash()
        {
            var rawOldToken = "valid-old-token";
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@test.com" };

            var oldTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                TokenHash = HashToken(rawOldToken),
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                RevokedAt = null,
                ReplacedByTokenHash = null,
            };

            _context.RefreshTokens.Add(oldTokenEntity);
            await _context.SaveChangesAsync();

            var newRawToken = "new-fresh-token";
            _tokenIssuerMock.Setup(j => j.GenerateRefreshToken()).Returns(newRawToken);
            _tokenIssuerMock
                .Setup(j => j.GenerateAccessToken(It.IsAny<UserTokenDetails>()))
                .Returns("new-access-token");
            _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string>());

            var result = await _sut.CheckRefreshToken(rawOldToken, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(newRawToken, result.Value.RefreshToken);
            Assert.Equal("new-access-token", result.Value.AccessToken);

            var oldEntityInDb = await _context.RefreshTokens.FirstAsync(t =>
                t.Id == oldTokenEntity.Id
            );
            Assert.True(oldEntityInDb.IsRevoked);
            Assert.NotNull(oldEntityInDb.RevokedAt);
            Assert.Equal(HashToken(newRawToken), oldEntityInDb.ReplacedByTokenHash);

            var newEntityInDb = await _context.RefreshTokens.FirstAsync(t =>
                t.TokenHash == HashToken(newRawToken)
            );
            Assert.Equal(user.Id, newEntityInDb.UserId);
            Assert.True(newEntityInDb.IsActive);
            Assert.Null(newEntityInDb.ReplacedByTokenHash);
        }

        [Fact]
        public async Task CheckRefreshToken_WhenRotatedTokenReused_ShouldDetectTheftAndRevokeAllSessions()
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, Email = "user@test.com" };
            var reusedRawToken = "already-used-raw-token";

            var compromisedToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                User = user,
                TokenHash = HashToken(reusedRawToken),
                RevokedAt = DateTime.UtcNow.AddMinutes(-10),
                ReplacedByTokenHash = "previous-replacement-token-hash",
                ExpiresAt = DateTime.UtcNow.AddDays(5),
            };

            var otherActiveToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                User = user,
                TokenHash = "other-session-token-hash",
                RevokedAt = null,
                ExpiresAt = DateTime.UtcNow.AddDays(5),
            };

            _context.RefreshTokens.AddRange(compromisedToken, otherActiveToken);
            await _context.SaveChangesAsync();

            var result = await _sut.CheckRefreshToken(reusedRawToken, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal("Identity.RefreshTokenCompromised", result.Error.Code);

            var activeTokens = await _context
                .RefreshTokens.Where(t => t.UserId == userId && t.RevokedAt == null)
                .ToListAsync();

            Assert.Empty(activeTokens);
        }

        [Fact]
        public async Task CheckRefreshToken_WhenTokenRevokedViaLogout_ShouldNotRevokeOtherSessions()
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, Email = "user@test.com" };
            var logoutRawToken = "logout-raw-token";

            var loggedOutToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                User = user,
                TokenHash = HashToken(logoutRawToken),
                RevokedAt = DateTime.UtcNow.AddMinutes(-5),
                ReplacedByTokenHash = null,
                ExpiresAt = DateTime.UtcNow.AddDays(5),
            };

            var otherSessionToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                User = user,
                TokenHash = "other-active-session-hash",
                RevokedAt = null,
                ExpiresAt = DateTime.UtcNow.AddDays(5),
            };

            _context.RefreshTokens.AddRange(loggedOutToken, otherSessionToken);
            await _context.SaveChangesAsync();

            var result = await _sut.CheckRefreshToken(logoutRawToken, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal("Identity.RefreshTokenRevoked", result.Error.Code);

            var sessionInDb = await _context.RefreshTokens.FirstAsync(t =>
                t.TokenHash == "other-active-session-hash"
            );
            Assert.True(sessionInDb.IsActive);
        }

        [Fact]
        public async Task CheckRefreshToken_WhenTokenExpired_ShouldReturnExpiredFailure()
        {
            var rawToken = "expired-raw-token";
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@test.com" };

            var expiredToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                TokenHash = HashToken(rawToken),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ExpiresAt = DateTime.UtcNow.AddMinutes(-5),
                RevokedAt = null,
            };

            _context.RefreshTokens.Add(expiredToken);
            await _context.SaveChangesAsync();

            var result = await _sut.CheckRefreshToken(rawToken, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal("Refresh token expired", result.Error.Description);

            var count = await _context.RefreshTokens.CountAsync();
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CheckRefreshToken_ShouldPersistOnlySha256HashNotRawToken()
        {
            var rawOldToken = "plain-text-secret-token";
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@test.com" };

            var oldTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                TokenHash = HashToken(rawOldToken),
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            };

            _context.RefreshTokens.Add(oldTokenEntity);
            await _context.SaveChangesAsync();

            var newRawToken = "another-plain-secret-token";
            _tokenIssuerMock.Setup(j => j.GenerateRefreshToken()).Returns(newRawToken);
            _tokenIssuerMock
                .Setup(j => j.GenerateAccessToken(It.IsAny<UserTokenDetails>()))
                .Returns("access-token");
            _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string>());

            await _sut.CheckRefreshToken(rawOldToken, CancellationToken.None);

            var allTokens = await _context.RefreshTokens.ToListAsync();
            foreach (var t in allTokens)
            {
                Assert.NotEqual(rawOldToken, t.TokenHash);
                Assert.NotEqual(newRawToken, t.TokenHash);
                Assert.Equal(64, t.TokenHash.Length);
            }
        }

        [Fact]
        public async Task RevokeRefreshToken_WhenTokenExists_ShouldRevokeSingleTokenOnly()
        {
            var rawToken = "token-to-revoke";
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@test.com" };

            var tokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                TokenHash = HashToken(rawToken),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ExpiresAt = DateTime.UtcNow.AddDays(6),
                RevokedAt = null,
                ReplacedByTokenHash = null,
            };

            _context.RefreshTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();

            var result = await _sut.RevokeRefreshToken(rawToken, CancellationToken.None);

            Assert.True(
                result.IsSuccess,
                result.IsFailure ? $"{result.Error.Code}: {result.Error.Description}" : string.Empty
            );

            var entityInDb = await _context.RefreshTokens.FirstAsync(t => t.Id == tokenEntity.Id);
            Assert.True(entityInDb.IsRevoked);
            Assert.NotNull(entityInDb.RevokedAt);
            Assert.Null(entityInDb.ReplacedByTokenHash);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
