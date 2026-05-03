using Moq;
using FluentAssertions;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Configuration;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Model;
using flatshare_server.Infrastructure.Model.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using flatshare_server.Infrastructure.Utils;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Flatshare.Tests.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ISessionRepository> _sessionRepoMock;
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _sessionRepoMock = new Mock<ISessionRepository>();

        _jwtOptions = Options.Create(new JwtOptions
        {
            Secret = "super_secret_key_123_super_secret_key_123",
            Issuer = "test_issuer",
            Audience = "test_audience",
            ExpirationTimeInMinutes = 60
        });

        _authService = new AuthService(
            _userRepoMock.Object,
            _sessionRepoMock.Object,
            _jwtOptions
        );
    }

    [Fact]
    public async Task Authenticate_ShouldThrow401_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmail(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _authService.Authenticate("wrong@test.pl", "pass");

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Authenticate_ShouldCreateSession_WhenCredentialsAreValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var password = "CorrectPassword123";
        var hash = PasswordEncoder.Encrypt(password, userId);

        var user = new User
        {
            Id = userId,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.pl",
            PassHash = hash,
            Status = new AccountStatus { },
            Role = new TenantRole { 
                TenantPreferences = new TenantPreferences { }
            }
        };

        _userRepoMock.Setup(r => r.GetByEmail(user.Email)).ReturnsAsync(user);

        // Act
        var result = await _authService.Authenticate(user.Email, password);

        // Assert
        result.Item1.Should().NotBeNullOrEmpty(); // JWT Token
        _sessionRepoMock.Verify(r => r.SaveNew(result.Item2, userId), Times.Once);
    }

    [Fact]
    public async Task Refresh_ShouldReturnNewToken_ForExistingSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = new UserSession { Id = sessionId, UserId = userId };

        var user = new User
        {
            Id = userId,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.pl",
            PassHash = "anyHash",
            Status = new AccountStatus { },
            Role = new TenantRole
            {
                TenantPreferences = new TenantPreferences { }
            }
        };

        _sessionRepoMock.Setup(r => r.GetBySessionId(sessionId)).ReturnsAsync(session);
        _userRepoMock.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        // Act
        var result = await _authService.Refresh(sessionId);

        // Assert
        result.Item1.Should().NotBeNullOrEmpty();
        result.Item4.Should().Contain("TENANT");
    }

    [Fact]
    public async Task GetUserFromSession_ShouldReturnUserId()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _sessionRepoMock.Setup(r => r.GetBySessionId(sessionId))
            .ReturnsAsync(new UserSession { Id = sessionId, UserId = userId });

        // Act
        var result = await _authService.GetUserFromSession(sessionId);

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public async Task Authenticate_ShouldThrow401_WhenPasswordIsInvalid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var correctPassword = "CorrectPassword123";
        var hash = PasswordEncoder.Encrypt(correctPassword, userId);

        var user = new User
        {
            Id = userId,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.pl",
            PassHash = hash,
            Status = new AccountStatus { },
            Role = new TenantRole
            {
                TenantPreferences = new TenantPreferences { }
            }
        };

        _userRepoMock.Setup(r => r.GetByEmail(user.Email)).ReturnsAsync(user);

        // Act
        var act = async () => await _authService.Authenticate(user.Email, "WrongPassword");

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Authenticate_ShouldReturn_EMPTYRole_WhenUserRoleToStringThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var password = "Secret123!";
        var hash = PasswordEncoder.Encrypt(password, userId);

        var user = new User
        {
            Id = userId,
            FirstName = "Err",
            LastName = "Role",
            Email = "errrole@test.pl",
            PassHash = hash,
            Status = new AccountStatus { },
            Role = new ThrowingUserRole()
        };

        _userRepoMock.Setup(r => r.GetByEmail(user.Email)).ReturnsAsync(user);

        // Act
        var result = await _authService.Authenticate(user.Email, password);

        // Assert
        result.Item4.Should().Be("EMPTY");
    }

    [Fact]
    public void GetUserId_ShouldReturnUserId_WhenClaimPresent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = _authService.GetUserId(principal);

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserId_ShouldThrow401_WhenClaimMissing()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var act = () => _authService.GetUserId(principal);

        // Assert
        var exception = act.Should().Throw<ServerResponseException>().Which;
        exception.Response.Status.Should().Be(StatusCodes.Status401Unauthorized);
    }

    private class ThrowingUserRole : UserRole
    {
        protected override string GetStringRepresentation()
        {
            throw new Exception("Role string generation failed");
        }
    }
}