using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Flatshare.Tests.UnitTests.Services;

public class ListingPhotoServiceTests
{
    private readonly DbContextOptions<FlatshareDbContext> _dbOptions;
    private readonly Mock<IStorageService> _storageMock;

    public ListingPhotoServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<FlatshareDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _storageMock = new Mock<IStorageService>();
    }

    private FlatshareDbContext CreateDbContext() => new FlatshareDbContext(_dbOptions);

    private async Task<Listing> SeedListingAsync(FlatshareDbContext context)
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@user.com",
            PassHash = "hash",
            Status = null!,
            Role = null!
        };
        context.Users.Add(owner);

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
        context.Listings.Add(listing);
        await context.SaveChangesAsync();

        return listing;
    }

    /* ---------------- UPLOAD PHOTO TESTS ---------------- */

    [Fact]
    public async Task UploadPhotoAsync_WhenFileIsEmpty_ThrowsServerResponseException()
    {
        // Arrange
        var context = CreateDbContext();
        var service = new ListingPhotoService(context, _storageMock.Object, null!);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        // Act
        var act = async () => await service.UploadPhotoAsync(Guid.NewGuid(), fileMock.Object);

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status400BadRequest);
        exception.Which.Response.Error.Should().Be("No photo provided");
    }

    [Fact]
    public async Task UploadPhotoAsync_WhenListingDoesNotExist_ThrowsServerResponseException()
    {
        // Arrange
        var context = CreateDbContext();
        var service = new ListingPhotoService(context, _storageMock.Object, null!);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("photo.jpg");

        // Act
        var act = async () => await service.UploadPhotoAsync(Guid.NewGuid(), fileMock.Object);

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status400BadRequest);
        exception.Which.Response.Error.Should().Be("Listing not found");
    }

    [Fact]
    public async Task UploadPhotoAsync_WhenInvalidExtension_ThrowsServerResponseException()
    {
        // Arrange
        var context = CreateDbContext();
        var listing = await SeedListingAsync(context);
        var service = new ListingPhotoService(context, _storageMock.Object, null!);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("document.pdf");

        // Act
        var act = async () => await service.UploadPhotoAsync(listing.Id, fileMock.Object);

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status400BadRequest);
        exception.Which.Response.Error.Should().Be("Invalid file type");
    }

    [Fact]
    public async Task UploadPhotoAsync_WhenValidImage_StoresFileAndUpdatesListing()
    {
        // Arrange
        var context = CreateDbContext();
        var listing = await SeedListingAsync(context);
        var service = new ListingPhotoService(context, _storageMock.Object, null!);

        var expectedFileId = Guid.NewGuid();
        _storageMock.Setup(s => s.StoreFileAsync(It.IsAny<IFormFile>())).ReturnsAsync(expectedFileId);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("image.PNG"); // Uppercase

        // Act
        var resultId = await service.UploadPhotoAsync(listing.Id, fileMock.Object);

        // Assert
        resultId.Should().Be(expectedFileId);
        _storageMock.Verify(s => s.StoreFileAsync(fileMock.Object), Times.Once);

        var dbListing = await context.Listings.FindAsync(listing.Id);
        dbListing!.Photos.Should().Contain(expectedFileId);
    }

    /* ---------------- GET PHOTOS TESTS ---------------- */

    [Fact]
    public async Task GetPhotosAsync_WhenListingExists_ReturnsPhotosList()
    {
        // Arrange
        var context = CreateDbContext();
        var listing = await SeedListingAsync(context);

        var photoId = Guid.NewGuid();
        listing.Photos.Add(photoId);
        await context.SaveChangesAsync();

        var service = new ListingPhotoService(context, _storageMock.Object, null!);

        // Act
        var response = await service.GetPhotosAsync(listing.Id);

        // Assert
        response.ListingId.Should().Be(listing.Id);
        response.Photos.Should().ContainSingle(p => p == photoId);
    }

    [Fact]
    public async Task GetPhotosAsync_WhenListingDoesNotExist_ThrowsServerResponseException()
    {
        // Arrange
        var context = CreateDbContext();
        var service = new ListingPhotoService(context, _storageMock.Object, null!);

        // Act
        var act = async () => await service.GetPhotosAsync(Guid.NewGuid());

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status404NotFound);
        exception.Which.Response.Error.Should().Be("Listing not found");
    }

    /* ---------------- DELETE PHOTO TESTS ---------------- */

    [Fact]
    public async Task DeleteAsync_WhenListingDoesNotExist_ThrowsServerResponseException()
    {
        // Arrange
        var context = CreateDbContext();
        var service = new ListingPhotoService(context, _storageMock.Object, null!);

        // Act
        var act = async () => await service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status404NotFound);
        exception.Which.Response.Error.Should().Be("Listing not found");
    }

    [Fact]
    public async Task DeleteAsync_WhenPhotoNotInListing_ThrowsServerResponseException()
    {
        // Arrange
        var context = CreateDbContext();
        var listing = await SeedListingAsync(context);
        var service = new ListingPhotoService(context, _storageMock.Object, null!);

        // Act
        var act = async () => await service.DeleteAsync(listing.Id, Guid.NewGuid());

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status404NotFound);
        exception.Which.Response.Error.Should().Be("Photo not found");
    }

    [Fact]
    public async Task DeleteAsync_WhenSuccess_RemovesFromListingAndCallsStorage()
    {
        // Arrange
        var context = CreateDbContext();
        var listing = await SeedListingAsync(context);

        var photoId = Guid.NewGuid();
        listing.Photos.Add(photoId);
        await context.SaveChangesAsync();

        _storageMock.Setup(s => s.DeleteFileAsync(photoId)).ReturnsAsync(true);
        var service = new ListingPhotoService(context, _storageMock.Object, null!);

        // Act
        var result = await service.DeleteAsync(listing.Id, photoId);

        // Assert
        result.Should().BeTrue();

        var dbListing = await context.Listings.FindAsync(listing.Id);
        dbListing!.Photos.Should().BeEmpty();

        _storageMock.Verify(s => s.DeleteFileAsync(photoId), Times.Once);
    }
}