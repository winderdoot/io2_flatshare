using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Requests.Matches;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Repositories;
using Xunit;

namespace Flatshare.Tests.IntegrationTests.Controllers;

public class MatchesIntegrationTests : IClassFixture<FlatshareApiFactory>
{
    private readonly HttpClient _client;
    private readonly FlatshareApiFactory _factory;

    public MatchesIntegrationTests(FlatshareApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private CreateListingRequest GenerateValidRequest(string city = "Warsaw", string district = "Wola", decimal price = 1500m, bool petsAllowed = false)
    {
        return new CreateListingRequest
        {
            Title = "Integration test listing",
            Description = "Listing used in integration tests",
            Price = price,
            Currency = "PLN",
            AvailableSince = DateOnly.FromDateTime(DateTime.UtcNow),
            AvailableUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            OwnerContact = "owner@test.local",
            Area = 45f,
            Location = new Address(city, district, "Main", "1"),
            Attributes = new ListingAttributes
            {
                Profile = ListingAttributes.TenantProfile.Student,
                PetsAllowed = petsAllowed,
                NonSmokingOnly = false,
                CloseToShops = true
            }
        };
    }

    // Helper: seed a landlord and a published listing, return listing id
    private async Task<Guid> SeedPublishedListingAsync(string city = "Warsaw", string district = "Wola", decimal price = 1500m, bool petsAllowed = false)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();

        var owner = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Owner",
            LastName = "Landlord",
            Email = $"owner+{Guid.NewGuid()}@test.local",
            PassHash = "hashed",
            Status = null!,
            Role = null!
        };

        db.Users.Add(owner);

        var req = GenerateValidRequest(city, district, price, petsAllowed);
        var listing = Listing.TryCreate(req, owner);

        // Mark as published/active so ApplyMatchesFilter will include it
        // Unit tests in the repo use .Publish() before saving; follow same approach.
        listing.Publish();

        db.Listings.Add(listing);
        await db.SaveChangesAsync();

        return listing.Id;
    }

    [Fact]
    public async Task GetMatches_ReturnsPublishedListings_ForRegisteredTenant()
    {
        // Arrange
        var listingId = await SeedPublishedListingAsync(city: "Warsaw", district: "Wola", price: 1300m);
        // seed another published listing
        var listingId2 = await SeedPublishedListingAsync(city: "Warsaw", district: "Wola", price: 2000m);

        // Register tenant user via API
        var tenantRequest = new CreateUserRequest("Tenant", "User", $"tenant+{Guid.NewGuid()}@test.local", "Pass123!", CreateUserRequest.Tenant);
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/users", tenantRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await registerResponse.Content.ReadFromJsonAsync<UserCreatedResponse>();
        created.Should().NotBeNull();
        var tenantId = created!.User.Id;

        // Use TestAuthHandler by setting headers to simulate logged-in tenant
        _client.DefaultRequestHeaders.Remove("X-Test-User-Id");
        _client.DefaultRequestHeaders.Remove("X-Test-User-Role");
        _client.DefaultRequestHeaders.Add("X-Test-User-Id", tenantId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-User-Role", CreateUserRequest.Tenant);

        // Act
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        var response = await _client.GetAsync("/api/v1/matches");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PageResponse<MatchDTO>>(jsonOptions);

        // Assert
        page.Should().NotBeNull();
        page!.Content.Should().NotBeNull();
        page.Content.Count.Should().BeGreaterThanOrEqualTo(2);
        var returnedIds = page.Content.Select(c => c.Listing.Id).ToList();
        returnedIds.Should().Contain(listingId);
        returnedIds.Should().Contain(listingId2);
    }

    [Fact]
    public async Task GetMatches_WithCityFilter_ReturnsOnlyMatchingCity()
    {
        // Arrange
        var warsawListing = await SeedPublishedListingAsync(city: "Warsaw", district: "Wola", price: 1200m);
        var krakowListing = await SeedPublishedListingAsync(city: "Krakow", district: "StareMiasto", price: 900m);

        // Register tenant
        var tenantRequest = new CreateUserRequest("Tenant", "Filter", $"tenantfilter+{Guid.NewGuid()}@test.local", "Pass123!", CreateUserRequest.Tenant);
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/users", tenantRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await registerResponse.Content.ReadFromJsonAsync<UserCreatedResponse>();
        var tenantId = created!.User.Id;

        // Authenticate as tenant via test headers
        _client.DefaultRequestHeaders.Remove("X-Test-User-Id");
        _client.DefaultRequestHeaders.Remove("X-Test-User-Role");
        _client.DefaultRequestHeaders.Add("X-Test-User-Id", tenantId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-User-Role", CreateUserRequest.Tenant);

        // Act: request filtered by city=Krakow
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        var response = await _client.GetAsync("/api/v1/matches?City=Krakow");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PageResponse<MatchDTO>>(jsonOptions);

        // Assert
        page.Should().NotBeNull();
        page!.Content.Should().NotBeNull();
        // Use Count-based assertion (avoids ambiguous/fluid overload issues)
        page.Content.Count.Should().BeGreaterThanOrEqualTo(1);
        page.Content.All(c => string.Equals(c.Listing.Location.City, "Krakow", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
        var returnedIds = page.Content.Select(c => c.Listing.Id).ToList();
        returnedIds.Should().Contain(krakowListing);
        returnedIds.Should().NotContain(warsawListing);
    }

    [Fact]
    public async Task TenantPreferences_AreApplied_WhenFilterParametersAreNotProvided()
    {
        // Arrange
        var listingId = await SeedPublishedListingAsync(city: "Warsaw", district: "Wola", price: 1300m, petsAllowed: true);
        // seed another published listing
        var listingId2 = await SeedPublishedListingAsync(city: "Warsaw", district: "Wola", price: 2500m, petsAllowed: false);

        var expectedMatchingListingId = listingId;

        // Register tenant via API
        var tenantRequest = new CreateUserRequest("Ewa", "Nowak", $"ewa+{Guid.NewGuid()}@test.local", "Pass123!", CreateUserRequest.Tenant);
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/users", tenantRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await registerResponse.Content.ReadFromJsonAsync<UserCreatedResponse>();
        created.Should().NotBeNull();
        var tenantId = created!.User.Id;

        // Authenticate subsequent requests as the tenant using TestAuthHandler
        _client.DefaultRequestHeaders.Remove("X-Test-User-Id");
        _client.DefaultRequestHeaders.Remove("X-Test-User-Role");
        _client.DefaultRequestHeaders.Add("X-Test-User-Id", tenantId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-User-Role", CreateUserRequest.Tenant);

        // Update tenant preferences via API: MaxPrice = 2000, PetsAllowed = true
        var prefDto = new TenantPreferencesDTO(
            MaxPrice: 2000m,
            Currency: "PLN",
            SmokingAllowed: null,
            PetsAllowed: true,
            PreferredDistricts: new List<string> { "Wola" }
        );

        var prefUpdateResponse = await _client.PutAsJsonAsync("/api/v1/users/me/preferences", prefDto);
        prefUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act: request matches WITHOUT supplying price/pets filters - preferences should be applied
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        var response = await _client.GetAsync("/api/v1/matches");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PageResponse<MatchDTO>>(jsonOptions);

        // Assert: only the listing that matches both price == MaxPrice and pets allowed is returned
        page.Should().NotBeNull();
        page!.Content.Should().NotBeNull();
        page.Content.Count.Should().Be(1);

        var returned = page.Content.Single();
        returned.Listing.Id.Should().Be(expectedMatchingListingId);
        returned.Listing.Price.Should().Be(1300m);
        returned.Listing.Attributes.PetsAllowed.Should().BeTrue();
    }
}
