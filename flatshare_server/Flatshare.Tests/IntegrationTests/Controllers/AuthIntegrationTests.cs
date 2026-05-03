using flatshare_server.Infrastructure.Model;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using flatshare_server.Infrastructure.Utils;

namespace Flatshare.Tests.IntegrationTests.Controllers;

public class AuthIntegrationTests : IClassFixture<FlatshareApiFactory>
{
    private readonly HttpClient _client;
    private readonly FlatshareApiFactory _factory;

    public AuthIntegrationTests(FlatshareApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<User> SeedUserAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();

        var id = Guid.NewGuid();
        var user = User.TryCreate(new CreateUserRequest("Auth", "Test", email, "old_pass", "TENANT"));

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task RequestPasswordReset_ForExistingUser_ReturnsAcceptedAndPersistsCode()
    {
        // Arrange
        var email = "user@example.com";
        var user = await SeedUserAsync(email);
        var request = new PasswordResetRequest(email);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/password-reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();

        var resetEntry = await dbContext.PasswordResetEntries
            .FirstOrDefaultAsync(r => r.UserId == user.Id);

        resetEntry.Should().NotBeNull();
        resetEntry!.ResetCode.Should().HaveLength(10);
    }

    [Fact]
    public async Task RequestPasswordReset_NonExistentUser_ReturnsAccepted()
    {
        // Arrange
        var request = new PasswordResetRequest("ghost@example.com");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/password-reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task ConfirmPasswordReset_ValidCode_ReturnsOk()
    {
        // Arrange
        var email = "confirm@example.com";
        var user = await SeedUserAsync(email);
        var code = CodeGenerator.GenerateResetCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();
            dbContext.PasswordResetEntries.Add(new PasswordResetEntry
            {
                UserId = user.Id,
                ResetCode = code,
                CreatedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
        }

        var newPass = "NewSecurePassword123!";
        var request = new ConfirmPasswordResetRequest(code, email, newPass);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/password-reset/confirm", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();

            var updatedUser = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            updatedUser.Should().NotBeNull();

            updatedUser!.PassHash.Should().NotBe(user.PassHash);
            updatedUser.PassHash.Should().NotBeNullOrWhiteSpace();
            updatedUser.PassHash.Should().Be(PasswordEncoder.Encrypt(newPass, user.Id));
        }
    }

    [Fact]
    public async Task ConfirmPasswordReset_InvalidCode_ReturnsBadRequest()
    {
        // Arrange
        var email = "wrongcode@example.com";
        await SeedUserAsync(email);

        var request = new ConfirmPasswordResetRequest("WRONG_CODE", email, "SomePassword123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/password-reset/confirm", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConfirmPasswordReset_ExpiredOrMissingRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new ConfirmPasswordResetRequest("ANYCODE", "no-request@example.com", "SomePassword123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/password-reset/confirm", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}