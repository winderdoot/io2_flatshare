using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Listings;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;

namespace Flatshare.Tests.UnitTests.Services;

public class ListingServiceTests
{
    private FlatshareDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FlatshareDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new FlatshareDbContext(options);
    }

    private CreateListingRequest GenerateValidRequest()
    {
        return new CreateListingRequest
        {
            Title = "Unit test apartment",
            Description = "Testing the service logic",
            Price = 1500m,
            Currency = "EUR",
            AvailableSince = DateOnly.FromDateTime(DateTime.Now),
            AvailableUntil = DateOnly.FromDateTime(DateTime.Now.AddMonths(6)),
            OwnerContact = "test@owner.com",
            Area = 40.0f,
            Location = new Address("Kraków", "Kazimierz", "Szeroka", "1"),
            Attributes = new ListingAttributes { Profile = ListingAttributes.TenantProfile.Student }
        };
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenListingExists()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var mockUserService = new Mock<UserService>(null!);
        var service = new ListingService(context, mockUserService.Object);

        var user = flatshare_server.Infrastructure.Model.Users.User.TryCreate(
            new CreateUserRequest("Test", "User", "test@test.pl", "Pass123!", CreateUserRequest.Landlord));

        var request = GenerateValidRequest();
        var listing = flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(request, user);

        context.Listings.Add(listing);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetByIdAsync(listing.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(listing.Id);
        result.Title.Should().Be(request.Title);
    }
    [Fact]
    public async Task GetByFilterAsync_ShouldReturnOnlyListingsFromSpecifiedCity()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var mockUserService = new Mock<UserService>(null!);
        var service = new ListingService(context, mockUserService.Object);

        var user = flatshare_server.Infrastructure.Model.Users.User.TryCreate(
            new CreateUserRequest("Test", "User", "test@test.pl", "Pass123!", CreateUserRequest.Landlord));

        var baseRequest = GenerateValidRequest();

        var krakowRequest1 = baseRequest with { Location = new Address("Kraków", "Kazimierz", "Szeroka", "1") };
        var krakowRequest2 = baseRequest with { Location = new Address("Kraków", "Podgórze", "Lwowska", "2") };
        var warsawRequest = baseRequest with { Location = new Address("Warszawa", "Wola", "Prosta", "5") };

        context.Listings.Add(flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(krakowRequest1, user));
        context.Listings.Add(flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(krakowRequest2, user));
        context.Listings.Add(flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(warsawRequest, user));
        await context.SaveChangesAsync();

        var filter = new ListingFilter { City = "Kraków" };

        // Act
        var results = await service.GetByFilterAsync(filter);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results.All(r => r.Location.City == "Kraków").Should().BeTrue();
    }
    [Fact]
    public async Task GetByFilterAsync_ShouldFilterByCityAndDistrict()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var mockUserService = new Mock<UserService>(null!);
        var service = new ListingService(context, mockUserService.Object);

        var user = flatshare_server.Infrastructure.Model.Users.User.TryCreate(
            new CreateUserRequest("Test", "User", "test@test.pl", "Pass123!", CreateUserRequest.Landlord));

        var baseRequest = GenerateValidRequest();

        var req1 = baseRequest with { Location = new Address("Kraków", "Stare Miasto", "Floriañska", "1") };
        var req2 = baseRequest with { Location = new Address("Kraków", "Kazimierz", "Szeroka", "2") };
        var req3 = baseRequest with { Location = new Address("Warszawa", "Œródmieœcie", "Z³ota", "44") };

        context.Listings.Add(flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(req1, user));
        context.Listings.Add(flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(req2, user));
        context.Listings.Add(flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(req3, user));
        await context.SaveChangesAsync();

        var filter = new ListingFilter { City = "Kraków", District = "Kazimierz" };

        // Act
        var results = await service.GetByFilterAsync(filter);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        results.First().Location.District.Should().Be("Kazimierz");
    }

    [Fact]
    public async Task GetByFilterAsync_ShouldIgnoreDistrict_WhenCityIsEmpty()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var mockUserService = new Mock<UserService>(null!);
        var service = new ListingService(context, mockUserService.Object);

        var user = flatshare_server.Infrastructure.Model.Users.User.TryCreate(
            new CreateUserRequest("Test", "User", "test@test.pl", "Pass123!", CreateUserRequest.Landlord));

        var baseRequest = GenerateValidRequest();
        var req1 = baseRequest with { Location = new Address("Kraków", "Kazimierz", "Szeroka", "2") };
        var req2 = baseRequest with { Location = new Address("Warszawa", "Kazimierz", "InnaUlica", "5") };

        context.Listings.Add(flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(req1, user));
        context.Listings.Add(flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(req2, user));
        await context.SaveChangesAsync();


        var filter = new ListingFilter { District = "Kazimierz" };

        // Act
        var results = await service.GetByFilterAsync(filter);

        // Assert
        results.Should().HaveCount(2);
    }
}