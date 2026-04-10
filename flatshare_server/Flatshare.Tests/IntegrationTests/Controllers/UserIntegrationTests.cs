using System.Net;
using System.Net.Http.Json;
using flatshare_server.Infrastructure.Model.Responses;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Model.Requests;

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

        // Act
        var response = await _client.GetAsync($"/api/v1/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var userDto = await response.Content.ReadFromJsonAsync<UserDTO>();
        userDto!.Id.Should().Be(userId);
        userDto.Email.Should().Be("anna@test.pl");
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenUserDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}