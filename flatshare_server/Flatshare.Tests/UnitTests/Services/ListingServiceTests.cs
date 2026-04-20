using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Model;
using flatshare_server.Infrastructure.Model.Exceptions;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Http;

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

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();

        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);
        var service = new ListingService(context, mockUserService.Object);

        var user = flatshare_server.Infrastructure.Model.Users.User.TryCreate(
            new CreateUserRequest("Test", "User", "test@test.pl", "Pass123!", CreateUserRequest.Landlord));

        var request = GenerateValidRequest();
        var listing = flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(request, user);

        context.Listings.Add(listing);
        await context.SaveChangesAsync();

        // Act
        var result = (await service.GetByIdAsync(listing.Id)).IntoDTO();

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

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();

        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);
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

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();

        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);
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

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();

        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);
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

    [Fact]
    public async Task UpdateAsync_ShouldModifyFields_WhenRequestIsValid()
    {
        // Arrange
        var context = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);
        var service = new ListingService(context, mockUserService.Object);

        var owner = flatshare_server.Infrastructure.Model.Users.User.TryCreate(
            new CreateUserRequest("Owner", "Landlord", "owner@test.pl", "Pass123!", CreateUserRequest.Landlord));

        var createReq = GenerateValidRequest();
        var listing = flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(createReq, owner);

        context.Listings.Add(listing);
        await context.SaveChangesAsync();

        var update = new UpdateListingRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Price = 2000m,
            Currency = "EUR",
            OwnerContact = "owner+updated@test.pl",
            Area = 55.5f,
            Attributes = new ListingAttributes { Profile = ListingAttributes.TenantProfile.Tourist, PetsAllowed = true },
            Location = new Address("Wroclaw", "OldTown", "Main", "2")
        };

        // Act
        var updated = await service.UpdateAsync(listing.Id, update);

        // Assert
        updated.Title.Should().Be(update.Title);
        updated.Description.Should().Be(update.Description);
        updated.Price.Value.Should().Be(update.Price!.Value);
        updated.Price.CurrencyStr().Should().Be(update.Currency);
        updated.OwnerContact.Should().Be(update.OwnerContact);
        updated.AreaMeterSq.Should().Be(update.Area!.Value);
        updated.Attributes.Profile.Should().Be(update.Attributes!.Profile);
        updated.Attributes.PetsAllowed.Should().BeTrue();
        updated.Address.City.Should().Be("Wroclaw");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow400_WhenPriceProvidedWithoutCurrency()
    {
        // Arrange
        var context = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);
        var service = new ListingService(context, mockUserService.Object);

        var owner = flatshare_server.Infrastructure.Model.Users.User.TryCreate(
            new CreateUserRequest("Owner", "Landlord", "owner@test.pl", "Pass123!", CreateUserRequest.Landlord));
        var createReq = GenerateValidRequest();
        var listing = flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(createReq, owner);

        context.Listings.Add(listing);
        await context.SaveChangesAsync();

        var update = new UpdateListingRequest
        {
            Price = 999m,
            Currency = null
        };

        // Act
        var act = async () => await service.UpdateAsync(listing.Id, update);

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow400_WhenDatesInvalid()
    {
        // Arrange
        var context = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);
        var service = new ListingService(context, mockUserService.Object);

        var owner = flatshare_server.Infrastructure.Model.Users.User.TryCreate(
            new CreateUserRequest("Owner", "Landlord", "owner@test.pl", "Pass123!", CreateUserRequest.Landlord));
        var createReq = GenerateValidRequest();
        var listing = flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(createReq, owner);

        context.Listings.Add(listing);
        await context.SaveChangesAsync();

        var since = DateOnly.FromDateTime(DateTime.Now.AddDays(10));
        var until = DateOnly.FromDateTime(DateTime.Now.AddDays(5)); // invalid: until <= since

        var update = new UpdateListingRequest
        {
            AvailableSince = since,
            AvailableUntil = until
        };

        // Act
        var act = async () => await service.UpdateAsync(listing.Id, update);

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    // ----------------------
    // New tests: State transitions
    // ----------------------

    [Fact]
    public async Task StateTransitions_ShouldFollowExpectedLifecycle()
    {
        // Arrange
        var context = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);
        var service = new ListingService(context, mockUserService.Object);

        var owner = flatshare_server.Infrastructure.Model.Users.User.TryCreate(
            new CreateUserRequest("Owner", "Landlord", "owner@test.pl", "Pass123!", CreateUserRequest.Landlord));
        var createReq = GenerateValidRequest();
        var listing = flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(createReq, owner);

        context.Listings.Add(listing);
        await context.SaveChangesAsync();

        // Submit -> UnderReview
        await service.SubmitAsync(listing.Id);
        (await service.GetByIdAsync(listing.Id)).Status.Should().Be(Listing.ListingStatus.UnderReview);

        // Approve -> Active
        await service.ApproveAsync(listing.Id);
        (await service.GetByIdAsync(listing.Id)).Status.Should().Be(Listing.ListingStatus.Active);

        // Hide -> Hidden
        await service.HideAsync(listing.Id);
        (await service.GetByIdAsync(listing.Id)).Status.Should().Be(Listing.ListingStatus.Hidden);

        // Publish -> Active
        await service.PublishAsync(listing.Id);
        (await service.GetByIdAsync(listing.Id)).Status.Should().Be(Listing.ListingStatus.Active);

        // Archive -> Archived
        await service.ArchiveAsync(listing.Id);
        (await service.GetByIdAsync(listing.Id)).Status.Should().Be(Listing.ListingStatus.Archived);
    }

    [Fact]
    public async Task RequestFixes_ShouldRevertToDraft_WhenUnderReview()
    {
        // Arrange
        var context = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);
        var service = new ListingService(context, mockUserService.Object);

        var owner = flatshare_server.Infrastructure.Model.Users.User.TryCreate(
            new CreateUserRequest("Owner", "Landlord", "owner@test.pl", "Pass123!", CreateUserRequest.Landlord));
        var createReq = GenerateValidRequest();
        var listing = flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(createReq, owner);

        context.Listings.Add(listing);
        await context.SaveChangesAsync();

        await service.SubmitAsync(listing.Id);
        (await service.GetByIdAsync(listing.Id)).Status.Should().Be(Listing.ListingStatus.UnderReview);

        await service.RequestFixesAsync(listing.Id);
        (await service.GetByIdAsync(listing.Id)).Status.Should().Be(Listing.ListingStatus.Draft);
    }

    [Fact]
    public async Task ApproveAsync_ShouldThrow400_WhenNotUnderReview()
    {
        // Arrange
        var context = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);
        var service = new ListingService(context, mockUserService.Object);

        var owner = flatshare_server.Infrastructure.Model.Users.User.TryCreate(
            new CreateUserRequest("Owner", "Landlord", "owner@test.pl", "Pass123!", CreateUserRequest.Landlord));
        var createReq = GenerateValidRequest();
        var listing = flatshare_server.Infrastructure.Model.Listings.Listing.TryCreate(createReq, owner);

        context.Listings.Add(listing);
        await context.SaveChangesAsync();

        // Act
        var act = async () => await service.ApproveAsync(listing.Id);

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status400BadRequest);
    }
}