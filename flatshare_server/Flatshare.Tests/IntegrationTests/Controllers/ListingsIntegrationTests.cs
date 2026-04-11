using System.Net;
using System.Net.Http.Json;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Model.Listings;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flatshare.Tests.IntegrationTests.Controllers;

public class ListingsIntegrationTests : IClassFixture<FlatshareApiFactory>
{
    private readonly HttpClient _client;
    private readonly FlatshareApiFactory _factory;

    public ListingsIntegrationTests(FlatshareApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private CreateListingRequest GenerateValidRequest()
    {
        return new CreateListingRequest
        {
            Title = "Przytulna kawalerka",
            Description = "Œwietna lokalizacja, blisko stacji metra.",
            Price = 2500m,
            Currency = "PLN",
            AvailableSince = DateOnly.FromDateTime(DateTime.Now),
            AvailableUntil = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
            OwnerContact = "123-456-789",
            Area = 35.5f,
            Location = new Address("Warszawa", "Œródmieœcie", "Z³ota", "44"),
            Attributes = new ListingAttributes { Profile = ListingAttributes.TenantProfile.Student }
        };
    }

    [Fact]
    public async Task CreateNew_ShouldReturn201_WhenUserIsLandlord()
    {
        // Arrange
        Guid landlordId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();
            var userRequest = new CreateUserRequest("Jan", "Kowalski", "landlord@test.pl", "Pass123!", CreateUserRequest.Landlord);
            var user = flatshare_server.Infrastructure.Model.Users.User.TryCreate(userRequest);
            landlordId = user.Id;
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        var request = GenerateValidRequest();

        _client.DefaultRequestHeaders.Add("X-Test-User-Id", landlordId.ToString());

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/listings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        var body = await response.Content.ReadFromJsonAsync<ListingCreatedResponse>(jsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateNew_ShouldReturn401_WhenUserIsNotAuthenticated()
    {
        var request = GenerateValidRequest();

        var response = await _client.PostAsJsonAsync("/api/v1/listings", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}