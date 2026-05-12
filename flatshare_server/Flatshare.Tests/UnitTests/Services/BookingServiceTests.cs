using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Model;
using flatshare_server.Infrastructure.Model.Responses.Booking;
using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Bookings;
using flatshare_server.Infrastructure.Model.Requests.Booking;

namespace Flatshare.Tests.UnitTests.Services;

public class BookingServiceTests
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
            Title = "Test apartment",
            Description = "Nice place for tests",
            Price = 1500m,
            Currency = "PLN",
            AvailableSince = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            AvailableUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
            OwnerContact = "owner@test.local",
            Area = 35.0f,
            Location = new Address("TestCity", "TestDistrict", "TestStreet", "1"),
            Attributes = new ListingAttributes { Profile = ListingAttributes.TenantProfile.Student }
        };
    }

    private FlatshareDbContext SeedListingWithOwner(FlatshareDbContext ctx, out User owner, out Listing listing)
    {
        // Ensure first and last names are longer than 5 characters to satisfy User.TryCreate validation
        owner = User.TryCreate(new CreateUserRequest("OwnerFirst", "OwnerLast", "owner@test.local", "Pass123!", CreateUserRequest.Landlord));
        var req = GenerateValidListingRequest();
        listing = Listing.TryCreate(req, owner);

        ctx.Listings.Add(listing);
        ctx.SaveChanges();

        return ctx;
    }

    [Fact]
    public async Task Create_ShouldPersistBooking_WhenValid()
    {
        var ctx = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);

        SeedListingWithOwner(ctx, out var owner, out var listing);

        var listingService = new ListingService(ctx, mockUserService.Object);
        var bookingService = new BookingService(ctx, listingService, mockUserService.Object);

        var tenant = User.TryCreate(new CreateUserRequest("TenantFirst", "TenantLast", "tenant@test.local", "Pass123!", CreateUserRequest.Tenant));

        var start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var end = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(40));

        var req = new CreateBookingRequest(listing.Id, start, end);

        var resp = await bookingService.Create(req, tenant.Id);

        resp.Should().NotBeNull();
        resp.BookingId.Should().NotBeNullOrWhiteSpace();
        resp.Status.Should().Be("PendingApproval");
        resp.TotalPrice.Should().BeGreaterThan(0);

        // persisted
        var stored = ctx.Bookings.SingleOrDefault(b => b.BookingId.ToString() == resp.BookingId);
        stored.Should().NotBeNull();
        stored!.TenantId.Should().Be(tenant.Id);
        stored.ListingId.Should().Be(listing.Id);
    }

    [Fact]
    public async Task Create_ShouldThrowConflict_WhenDatesOverlapExistingConfirmedBooking()
    {
        var ctx = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);

        SeedListingWithOwner(ctx, out var owner, out var listing);

        var listingService = new ListingService(ctx, mockUserService.Object);
        var bookingService = new BookingService(ctx, listingService, mockUserService.Object);

        var existingTenant = User.TryCreate(new CreateUserRequest("TenantOneFirst", "TenantOneLast", "t1@test.local", "Pass123!", CreateUserRequest.Tenant));

        var startEx = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        var endEx = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(25));
        var money = new Money { Curr = Money.Currency.PLN, Value = 1500m };

        var existing = Booking.TryCreate(new CreateBookingRequest(listing.Id, startEx, endEx), existingTenant.Id, money);
        // mark as confirmed so it blocks new bookings
        existing.OwnerAccept();
        existing.PaymentSuccess();
        ctx.Bookings.Add(existing);
        await ctx.SaveChangesAsync();

        var newTenant = User.TryCreate(new CreateUserRequest("TenantTwoFirst", "TenantTwoLast", "t2@test.local", "Pass123!", CreateUserRequest.Tenant));
        var req = new CreateBookingRequest(listing.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)));

        var act = async () => await bookingService.Create(req, newTenant.Id);

        var ex = await act.Should().ThrowAsync<ServerResponseException>();
        ex.Which.Response.Status.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Accept_ShouldMoveToPendingPayment_WhenCalledByOwner()
    {
        var ctx = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);

        SeedListingWithOwner(ctx, out var owner, out var listing);

        var listingService = new ListingService(ctx, mockUserService.Object);
        var bookingService = new BookingService(ctx, listingService, mockUserService.Object);

        var tenant = User.TryCreate(new CreateUserRequest("TenantAFirst", "TenantALast", "tenant2@test.local", "Pass123!", CreateUserRequest.Tenant));
        var req = new CreateBookingRequest(listing.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(37)));

        var created = await bookingService.Create(req, tenant.Id);

        var bookingId = Guid.Parse(created.BookingId);
        var acceptResp = await bookingService.Accept(bookingId, owner.Id);

        acceptResp.Should().NotBeNull();
        acceptResp.Status.Should().Be("PendingPayment");

        var stored = await ctx.Bookings.FindAsync(bookingId);
        stored!.Status.ToString().Should().Be("PendingPayment");
    }

    [Fact]
    public async Task Reject_ShouldMoveToRejected_WhenCalledByOwner()
    {
        var ctx = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);

        SeedListingWithOwner(ctx, out var owner, out var listing);

        var listingService = new ListingService(ctx, mockUserService.Object);
        var bookingService = new BookingService(ctx, listingService, mockUserService.Object);

        var tenant = User.TryCreate(new CreateUserRequest("TenantBFirst", "TenantBLast", "tenant3@test.local", "Pass123!", CreateUserRequest.Tenant));
        var req = new CreateBookingRequest(listing.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(8)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(18)));

        var created = await bookingService.Create(req, tenant.Id);
        var bookingId = Guid.Parse(created.BookingId);

        var reasonReq = new RejectBookingRequest("Owner changed mind");
        var resp = await bookingService.Reject(bookingId, owner.Id, reasonReq);

        resp.Should().NotBeNull();
        resp.Status.Should().Be("Rejected");

        var stored = await ctx.Bookings.FindAsync(bookingId);
        stored!.Status.ToString().Should().Be("Rejected");
    }

    [Fact]
    public async Task Cancel_ShouldAllowTenantToCancel_BeforeStart()
    {
        var ctx = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);

        SeedListingWithOwner(ctx, out var owner, out var listing);

        var listingService = new ListingService(ctx, mockUserService.Object);
        var bookingService = new BookingService(ctx, listingService, mockUserService.Object);

        var tenant = User.TryCreate(new CreateUserRequest("TenantCFirst", "TenantCLast", "tenant4@test.local", "Pass123!", CreateUserRequest.Tenant));
        var start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20));
        var end = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(50));
        var req = new CreateBookingRequest(listing.Id, start, end);

        var created = await bookingService.Create(req, tenant.Id);
        var bookingId = Guid.Parse(created.BookingId);

        var cancelReq = new CancelBookingRequest("Change of plans");
        var resp = await bookingService.Cancel(bookingId, tenant.Id, cancelReq);

        resp.Should().NotBeNull();
        resp.Status.Should().Be("Cancelled");

        var stored = await ctx.Bookings.FindAsync(bookingId);
        stored!.Status.ToString().Should().Be("Cancelled");
    }

    [Fact]
    public async Task Cancel_ShouldThrowWhenStartAlreadyBegun()
    {
        var ctx = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);

        SeedListingWithOwner(ctx, out var owner, out var listing);

        var listingService = new ListingService(ctx, mockUserService.Object);
        var bookingService = new BookingService(ctx, listingService, mockUserService.Object);

        var tenant = User.TryCreate(new CreateUserRequest("TenantDFirst", "TenantDLast", "tenant5@test.local", "Pass123!", CreateUserRequest.Tenant));
        // start in the past
        var start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));
        var end = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var money = new Money { Curr = Money.Currency.PLN, Value = 1500m };
        var existing = Booking.TryCreate(new CreateBookingRequest(listing.Id, start, end), tenant.Id, money);
        ctx.Bookings.Add(existing);
        await ctx.SaveChangesAsync();

        var act = async () => await bookingService.Cancel(existing.BookingId, tenant.Id, new CancelBookingRequest("Too late"));

        var ex = await act.Should().ThrowAsync<ServerResponseException>();
        ex.Which.Response.Status.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task GetById_ShouldAllowOwnerOrTenant_AndDenyOthers()
    {
        var ctx = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);

        SeedListingWithOwner(ctx, out var owner, out var listing);

        var listingService = new ListingService(ctx, mockUserService.Object);
        var bookingService = new BookingService(ctx, listingService, mockUserService.Object);

        var tenant = User.TryCreate(new CreateUserRequest("TenantGFirst", "TenantGLast", "tenant8@test.local", "Pass123!", CreateUserRequest.Tenant));
        var created = await bookingService.Create(new CreateBookingRequest(listing.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))), tenant.Id);
        var bookingId = Guid.Parse(created.BookingId);

        // Owner can fetch
        var dtoByOwner = await bookingService.GetById(bookingId, owner.Id);
        dtoByOwner.Id.Should().Be(bookingId);

        // Tenant can fetch
        var dtoByTenant = await bookingService.GetById(bookingId, tenant.Id);
        dtoByTenant.Id.Should().Be(bookingId);

        // Other user cannot
        var other = User.TryCreate(new CreateUserRequest("OtherFirst", "OtherLast", "other@test.local", "Pass123!", CreateUserRequest.Tenant));
        var act = async () => await bookingService.GetById(bookingId, other.Id);
        var ex = await act.Should().ThrowAsync<ServerResponseException>();
        ex.Which.Response.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task Get_ShouldFilterByTenantAndListing()
    {
        var ctx = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var mockUserService = new Mock<UserService>(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);

        // Seed owner and first listing
        SeedListingWithOwner(ctx, out var owner, out var listing1);

        // Create a second listing owned by the same owner
        var req2 = GenerateValidListingRequest();
        var listing2 = Listing.TryCreate(req2, owner);
        ctx.Listings.Add(listing2);
        await ctx.SaveChangesAsync();

        var listingService = new ListingService(ctx, mockUserService.Object);
        var bookingService = new BookingService(ctx, listingService, mockUserService.Object);

        var tenantA = User.TryCreate(new CreateUserRequest("TenantAlpha", "TenantAlphaLast", "ta@test.local", "Pass123!", CreateUserRequest.Tenant));
        var tenantB = User.TryCreate(new CreateUserRequest("TenantBeta", "TenantBetaLast", "tb@test.local", "Pass123!", CreateUserRequest.Tenant));

        var money = new Money { Curr = Money.Currency.PLN, Value = 1500m };

        var start1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var end1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20));

        var start2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15));
        var end2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(25));

        var start3 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var end3 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60));

        var bA1 = Booking.TryCreate(new CreateBookingRequest(listing1.Id, start1, end1), tenantA.Id, money);
        bA1.OwnerAccept();
        var bB1 = Booking.TryCreate(new CreateBookingRequest(listing1.Id, start2, end2), tenantB.Id, money);
        bB1.OwnerAccept();
        var bA2 = Booking.TryCreate(new CreateBookingRequest(listing2.Id, start3, end3), tenantA.Id, money);
        bA2.OwnerAccept();

        ctx.Bookings.AddRange(bA1, bB1, bA2);
        await ctx.SaveChangesAsync();

        // Filter by tenantA
        var resultTenantA = await bookingService.Get(tenantA.Id, null);
        resultTenantA.Should().HaveCount(2);
        resultTenantA.All(r => r.TenantId == tenantA.Id).Should().BeTrue();

        // Filter by listing1
        var resultListing1 = await bookingService.Get(null, listing1.Id);
        resultListing1.Should().HaveCount(2);
        resultListing1.All(r => r.ListingId == listing1.Id).Should().BeTrue();

        // Filter by tenantA and listing1
        var resultBoth = await bookingService.Get(tenantA.Id, listing1.Id);
        resultBoth.Should().HaveCount(1);
        resultBoth.First().TenantId.Should().Be(tenantA.Id);
        resultBoth.First().ListingId.Should().Be(listing1.Id);

        // No filters => all bookings
        var resultAll = await bookingService.Get(null, null);
        resultAll.Should().HaveCount(3);
    }
}