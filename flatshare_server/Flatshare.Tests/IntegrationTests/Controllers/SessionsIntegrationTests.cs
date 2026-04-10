using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Flatshare.Tests.IntegrationTests.Controllers;

public class SessionsIntegrationTests : IClassFixture<FlatshareApiFactory>
{
    private readonly HttpClient _client;
    private readonly FlatshareApiFactory _factory;
    private const string TestEmail = "integration@test.pl";
    private const string TestPassword = "Password123!";

    public SessionsIntegrationTests(FlatshareApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task SeedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var user = User.TryCreate(new CreateUserRequest("Test", "User", TestEmail, TestPassword, "TENANT"));
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task LogIn_ShouldReturn401_WhenPasswordIsInvalid()
    {
        // Arrange
        await SeedUserAsync();
        var loginRequest = new LoginRequest(TestEmail, "WrongPassword");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/sessions", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_ShouldReturnSessionData_WhenAuthenticated()
    {
        // Arrange
        await SeedUserAsync();

        // 1. Logujemy się, aby dostać token i ID sesji
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/sessions", new LoginRequest(TestEmail, TestPassword));
        var authData = await loginResponse.Content.ReadFromJsonAsync<LoggedInResponse>();

        // 2. Ustawiamy nagłówek Bearer dla klienta
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authData!.Token);

        // Act
        var response = await _client.GetAsync($"/api/v1/sessions/{authData.SessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<SessionDTO>();
        session!.SessionId.Should().Be(authData.SessionId);
    }

    [Fact]
    public async Task SessionRefresh_ShouldReturnNewToken_WhenSessionIsValid()
    {
        // Arrange
        await SeedUserAsync();
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/sessions", new LoginRequest(TestEmail, TestPassword));
        var authData = await loginResponse.Content.ReadFromJsonAsync<LoggedInResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authData!.Token);

        // Act
        var response = await _client.PatchAsync($"/api/v1/sessions/{authData.SessionId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var newData = await response.Content.ReadFromJsonAsync<LoggedInResponse>();
        newData!.Token.Should().NotBe(authData.Token);
        newData.SessionId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_ShouldReturn401_WhenTokenIsMissing()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/sessions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenSessionDoesNotExistInDb()
    {
        // Arrange
        await SeedUserAsync();
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/sessions", new LoginRequest(TestEmail, TestPassword));
        var authData = await loginResponse.Content.ReadFromJsonAsync<LoggedInResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authData!.Token);

        // Act
        var response = await _client.GetAsync($"/api/v1/sessions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}