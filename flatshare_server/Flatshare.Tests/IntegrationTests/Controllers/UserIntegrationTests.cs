using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Flatshare.Tests.IntegrationTests.Controllers;

public class UsersIntegrationTests : IClassFixture<FlatshareApiFactory>
{
    private readonly HttpClient _client;
    private readonly FlatshareApiFactory _factory;

    public UsersIntegrationTests(FlatshareApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldCreateUserInDatabase_AndReturn201()
    {
        // Arrange
        var request = new CreateUserRequest(
            "Robert",
            "Lewandowski",
            "robert@test.pl",
            "Password123!",
            CreateUserRequest.Tenant
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<UserCreatedResponse>();
        body!.User.Email.Should().Be(request.Email);
        body.User.Id.Should().NotBeEmpty();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();
        var userInDb = await db.Users.FindAsync(body.User.Id);

        userInDb.Should().NotBeNull();
        userInDb!.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Register_ShouldReturn400_WhenEmailIsInvalid()
    {
        // Arrange
        var request = new CreateUserRequest(
            "Jan", "Kowalski", "zly-email", "Pass123!", CreateUserRequest.Tenant
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Error.Should().Be("Register Error");
        error.FieldErrors.Should().Contain(e => e.Field == "Email");
    }

    [Fact]
    public async Task GetById_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        Guid userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();
            var request = new CreateUserRequest("Anna", "Nowak", "anna@test.pl", "Pass123!", CreateUserRequest.Landlord);
            var user = flatshare_server.Infrastructure.Model.Users.User.TryCreate(request);
            userId = user.Id;
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        // Provide test auth header so the request is treated as authenticated
        _client.DefaultRequestHeaders.Remove("X-Test-User-Id");
        _client.DefaultRequestHeaders.Add("X-Test-User-Id", userId.ToString());

        // Act
        var response = await _client.GetAsync($"/api/v1/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var userDto = await response.Content.ReadFromJsonAsync<UserDTO>();
        userDto!.Id.Should().Be(userId);
        userDto.Email.Should().Be("anna@test.pl");
    }

    [Fact]
    public async Task GetById_ShouldReturn403_WhenUserAttemptsToAccessOtherUser()
    {
        // Arrange
        // Authenticate as one user but request data for a different user should be forbidden
        var authenticatedUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        _client.DefaultRequestHeaders.Remove("X-Test-User-Id");
        _client.DefaultRequestHeaders.Add("X-Test-User-Id", authenticatedUserId.ToString());

        // Act
        var response = await _client.GetAsync($"/api/v1/users/{targetUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PutPreferences_ShouldUpdateDatabase_AndReturn200_WhenUserIsTenant()
    {
        // Arrange
        Guid userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();
            var request = new CreateUserRequest("Jan", "Kowalski", "tenant@pref.pl", "Pass123!", CreateUserRequest.Tenant);
            var user = flatshare_server.Infrastructure.Model.Users.User.TryCreate(request);
            userId = user.Id;
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Remove("X-Test-User-Id");
        _client.DefaultRequestHeaders.Add("X-Test-User-Id", userId.ToString());
        _client.DefaultRequestHeaders.Remove("X-Test-User-Role");
        _client.DefaultRequestHeaders.Add("X-Test-User-Role", CreateUserRequest.Tenant);

        var dto = new TenantPreferencesDTO(2500m, "PLN", false, true, new List<string> { "Wola", "Mokotów" });

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/users/me/preferences", dto);

        // Assert HTTP Response
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultDto = await response.Content.ReadFromJsonAsync<TenantPreferencesDTO>();
        resultDto!.MaxPrice.Should().Be(2500m);
        resultDto.PreferredDistricts.Should().BeEquivalentTo("Wola", "Mokotów");

        // Assert Database State
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();
            var userInDb = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);

            var tenantRole = userInDb!.Role as TenantRole;
            tenantRole.Should().NotBeNull();
            tenantRole!.TenantPreferences.MaxPrice.Should().Be(2500m);
            tenantRole.TenantPreferences.PreferredDistricts.Should().BeEquivalentTo("Wola", "Mokotów");
        }
    }

    [Fact]
    public async Task GetPreferences_ShouldReturn403_WhenUserIsLandlord()
    {
        // Arrange
        Guid userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();
            var request = new CreateUserRequest("Adam", "Nowak", "landlord@pref.pl", "Pass123!", CreateUserRequest.Landlord);
            var user = flatshare_server.Infrastructure.Model.Users.User.TryCreate(request);
            userId = user.Id;
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        _client.DefaultRequestHeaders.Remove("X-Test-User-Id");
        _client.DefaultRequestHeaders.Add("X-Test-User-Id", userId.ToString());
        _client.DefaultRequestHeaders.Remove("X-Test-User-Role");
        _client.DefaultRequestHeaders.Add("X-Test-User-Role", CreateUserRequest.Landlord);

        // Act
        var response = await _client.GetAsync("/api/v1/users/me/preferences");

        // Assert
        // The [Authorize(Roles = "TENANT")] should block this before it even hits the controller logic
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}