using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Services.Listings;
using flatshare_server.Infrastructure.Services.Bookings;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Model;
using flatshare_server.Infrastructure.Model.Bookings;
using flatshare_server.Infrastructure.Model.Requests.Booking;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Responses.Booking;
using flatshare_server.Infrastructure.Model.Exceptions;
using Microsoft.Extensions.Options;
using flatshare_server.Infrastructure.Configuration;

namespace Flatshare.Tests.IntegrationTests.Services;

public class ListingUnavailabilityIntegrationTests
{
    private FlatshareDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FlatshareDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new FlatshareDbContext(options);
    }

    private CreateListingRequest GenerateValidListingRequest()
    {
        return new CreateListingRequest
        {
            Title = "Integration Test apartment",
            Description = "Integration test listing",
            Price = 1500m,
            Currency = "PLN",
            AvailableSince = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            AvailableUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
            OwnerContact = "owner@test.local",
            Area = 35.0f,
            Location = new Address("IntegrationCity", "IntegrationDistrict", "IntegrationStreet", "1"),
            Attributes = new ListingAttributes { Profile = ListingAttributes.TenantProfile.Student }
        };
    }

    private void SeedPublishedListing(FlatshareDbContext ctx, out User owner, out Listing listing)
    {
        // names must meet validation rules
        owner = User.TryCreate(new CreateUserRequest("OwnerFirst", "OwnerLast", "owner@test.local", "Pass123!", CreateUserRequest.Landlord));
        ctx.Users.Add(owner);

        var req = GenerateValidListingRequest();
        listing = Listing.TryCreate(req, owner);
        ctx.Listings.Add(listing);
        ctx.SaveChanges();

        // Bring listing to Active state via ListingService in tests below (service expects listing present in DB).
    }

    [Fact(DisplayName = "Add/Remove unavailability works when no bookings exist")]
    public async Task AddRemoveUnavailability_NoBookings_Succeeds()
    {
        var ctx = CreateInMemoryDbContext();

        // user service used by listing service; repository mocks are sufficient for this integration-style test
        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var userService = new UserService(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);

        SeedPublishedListing(ctx, out var owner, out var listing);

        var listingService = new ListingService(ctx, userService);

        // submit & approve listing so it becomes visible/published (replicates controller workflow)
        await listingService.SubmitAsync(listing.Id);
        await listingService.ApproveAsync(listing.Id); // now Active

        // define unavailability period
        var since = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var until = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20));
        var unavailability = new Unavailability
        {
            Since = since,
            Until = until,
            Message = string.Empty // or provide an appropriate message if needed
        };

        // Add unavailability
        await listingService.AddUnavailabilityAsync(listing.Id, unavailability);

        // verify persisted on listing
        var reloaded = await listingService.GetByIdAsync(listing.Id, attachOwner: true);
        reloaded.Unavailabilities.Should().ContainSingle(u => u.Since == since && u.Until == until);

        // Remove unavailability
        await listingService.RemoveUnavailabilityAsync(listing.Id, new UnavailabilityRange { Since = since, Until = until });

        // verify removed
        var afterRemove = await listingService.GetByIdAsync(listing.Id, attachOwner: true);
        afterRemove.Unavailabilities.Should().NotContain(u => u.Since == since && u.Until == until);
    }

    [Fact(DisplayName = "Adding unavailability is blocked when a pending booking exists")]
    public async Task AddUnavailability_Blocked_WhenPendingBookingExists()
    {
        var ctx = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var userService = new UserService(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);

        SeedPublishedListing(ctx, out var owner, out var listing);

        var listingService = new ListingService(ctx, userService);
        var bookingService = new BookingService(ctx, listingService);

        // publish listing
        await listingService.SubmitAsync(listing.Id);
        await listingService.ApproveAsync(listing.Id); // Active

        // create tenant and booking
        var tenant = User.TryCreate(new CreateUserRequest("TenantFirst", "TenantLast", "tenant@test.local", "Pass123!", CreateUserRequest.Tenant));
        ctx.Users.Add(tenant);
        await ctx.SaveChangesAsync();

        var start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12));
        var end = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(22));
        var createReq = new CreateBookingRequest(listing.Id, start, end);

        var bookingCreated = await bookingService.Create(createReq, tenant.Id);
        var bookingId = Guid.Parse(bookingCreated.BookingId);

        // Owner accepts -> booking moves to PENDING_PAYMENT (pending)
        await bookingService.Accept(bookingId, owner.Id);

        // attempt to add unavailability overlapping booking -> should be blocked
        var since = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var until = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(25));
        var unavailability = new Unavailability
        {
            Since = since,
            Until = until,
            Message = string.Empty // or provide an appropriate message if needed
        };

        var act = async () => await bookingService.VerifyUnavailabilityCollisionsAsync(listing.Id, unavailability.Since, unavailability.Until);

        var ex = await act.Should().ThrowAsync<ServerResponseException>();
        // expect conflict error (business rule prevents changing availability when pending booking exists)
        ex.Which.Response.Status.Should().Be(StatusCodes.Status409Conflict);
    }
}