using Flatshare.Tests.Utils;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Flatshare.Tests.IntegrationTests.Controllers;

public class ListingPhotosIntegrationTests : IClassFixture<FlatshareApiFactory>
{
    private readonly HttpClient _client;
    private readonly FlatshareApiFactory _factory;

    // Structure of the response when uploading a photo
    private record UploadPhotoResponse(Guid id);

    public ListingPhotosIntegrationTests(FlatshareApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Set the header for TestAuthHandler to bypass standard authorization
        _client.DefaultRequestHeaders.Add("X-Test-User-Id", Guid.NewGuid().ToString());
    }

    // Helper method: seeds a valid listing to the In-Memory database and returns its ID
    private async Task<Guid> SeedListingAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FlatshareDbContext>();

        var owner = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@user.com",
            PassHash = "hashedpassword",
            Status = null!,
            Role = null!
        };
        dbContext.Users.Add(owner);

        var request = new CreateListingRequest
        {
            Title = "Test listing",
            Description = "Description of the test listing",
            OwnerContact = "123456789",
            Currency = "PLN",
            Price = 1500m,
            Area = 50f,
            AvailableSince = DateOnly.FromDateTime(DateTime.UtcNow),
            AvailableUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),

            Location = new Address("Warsaw", "Wola", "Prosta", "1"),
            Attributes = new ListingAttributes
            {
                PetsAllowed = true,
                NonSmokingOnly = false,
                CloseToShops = true,
                Profile = ListingAttributes.TenantProfile.Student
            }
        };

        var listing = Listing.TryCreate(request, owner);

        dbContext.Listings.Add(listing);
        await dbContext.SaveChangesAsync();

        return listing.Id;
    }

    [Fact]
    public async Task CreateNew_WithValidImage_ReturnsCreatedAndUploadsFile()
    {
        // Arrange
        var listingId = await SeedListingAsync();

        // Create a fake file in memory
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5 });
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "test_image.jpg");

        // Act
        var response = await _client.PostAsync($"/api/v1/listings/{listingId}/photos", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateNew_WithInvalidExtension_ReturnsBadRequest()
    {
        // Arrange
        var listingId = await SeedListingAsync();

        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");

        using var formData = new MultipartFormDataContent();

        // Attempt to upload a .txt file (the service verifies extensions)
        formData.Add(fileContent, "file", "document.txt");

        // Act
        var response = await _client.PostAsync($"/api/v1/listings/{listingId}/photos", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ExistingPhoto_ReturnsFileStream()
    {
        // Arrange
        var listingId = await SeedListingAsync();

        var fileContent = new ByteArrayContent(new byte[] { 0xAA, 0xBB, 0xCC });
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "photo.png");

        var uploadResponse = await _client.PostAsync($"/api/v1/listings/{listingId}/photos", formData);

        var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<UploadPhotoResponse>();
        var photoId = uploadResult!.id;

        // Act
        var getResponse = await _client.GetAsync($"/api/v1/listings/{listingId}/photos/{photoId}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getResponse.Content.Headers.ContentType?.MediaType.Should().Be("image/png");

        var bytes = await getResponse.Content.ReadAsByteArrayAsync();
        bytes.Should().BeEquivalentTo(new byte[] { 0xAA, 0xBB, 0xCC });
    }

    [Fact]
    public async Task Delete_ExistingPhoto_ReturnsNoContent()
    {
        // Arrange
        var listingId = await SeedListingAsync();

        var fileContent = new ByteArrayContent(new byte[] { 1 });
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "to_delete.jpg");

        var uploadResponse = await _client.PostAsync($"/api/v1/listings/{listingId}/photos", formData);

        var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<UploadPhotoResponse>();
        var photoId = uploadResult!.id;

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/v1/listings/{listingId}/photos/{photoId}");
        var getResponse = await _client.GetAsync($"/api/v1/listings/{listingId}/photos/{photoId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound); // The file should no longer exist
    }
}