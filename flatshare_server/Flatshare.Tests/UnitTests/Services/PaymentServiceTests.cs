using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Stripe;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services.Bookings;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Model;
using flatshare_server.Infrastructure.Model.Bookings;
using flatshare_server.Infrastructure.Model.Requests.Booking;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses.Booking;
using flatshare_server.Infrastructure.Model.Exceptions;

namespace Flatshare.Tests.UnitTests.Services;

public class PaymentServiceTests
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
            Location = new flatshare_server.Infrastructure.Model.Listings.Address("TestCity", "TestDistrict", "TestStreet", "1"),
            Attributes = new ListingAttributes { Profile = ListingAttributes.TenantProfile.Student }
        };
    }

    private void SeedListingWithOwner(FlatshareDbContext ctx, out User owner, out Listing listing)
    {
        // Names must be >= 3 chars, keep them longer to satisfy validation
        owner = User.TryCreate(new CreateUserRequest("OwnerFirst", "OwnerLast", "owner@test.local", "Pass123!", CreateUserRequest.Landlord));
        ctx.Users.Add(owner);

        var req = GenerateValidListingRequest();
        listing = Listing.TryCreate(req, owner);
        ctx.Listings.Add(listing);

        ctx.SaveChanges();
    }

    [Fact]
    public async Task GetById_AllowsTenantOwnerAndAdmin_BlocksOthers()
    {
        var ctx = CreateInMemoryDbContext();

        // repositories for UserService
        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var userService = new UserService(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);

        SeedListingWithOwner(ctx, out var owner, out var listing);

        // Create booking + payment
        var tenant = User.TryCreate(new CreateUserRequest("TenantFirst", "TenantLast", "tenant@test.local", "Pass123!", CreateUserRequest.Tenant));
        ctx.Users.Add(tenant);

        var start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var end = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(37));
        var money = new Money { Curr = Money.Currency.PLN, Value = 1500m };

        var booking = Booking.TryCreate(new CreateBookingRequest(listing.Id, start, end), tenant.Id, money);
        ctx.Bookings.Add(booking);

        var payment = new Payment(booking.BookingId, booking.TotalPrice);
        payment.RedirectToGateway();
        ctx.Payments.Add(payment);

        await ctx.SaveChangesAsync();

        var listingService = new ListingService(ctx, userService);

        // PaymentService needs a stripe client but we won't call payment initiation here
        var stripeClientMock = new Mock<IStripeClient>();
        var paymentService = new PaymentService(ctx, stripeClientMock.Object, listingService, userService);

        // --- Tenant should be able to fetch ---
        var dtoByTenant = await paymentService.GetById(payment.PaymentId, tenant.Id);
        dtoByTenant.Should().NotBeNull();
        dtoByTenant.PaymentId.Should().Be(payment.PaymentId);
        dtoByTenant.BookingId.Should().Be(booking.BookingId);

        // --- Owner should be able to fetch ---
        var dtoByOwner = await paymentService.GetById(payment.PaymentId, owner.Id);
        dtoByOwner.Should().NotBeNull();
        dtoByOwner.PaymentId.Should().Be(payment.PaymentId);

        // --- Admin should be able to fetch (via UserService lookup) ---
        var admin = User.TryCreate(new CreateUserRequest("AdminFirst", "AdminLast", "admin@test.local", "Pass123!", CreateUserRequest.Admin));
        mockUserRepo.Setup(r => r.GetById(admin.Id)).ReturnsAsync(admin);

        var dtoByAdmin = await paymentService.GetById(payment.PaymentId, admin.Id);
        dtoByAdmin.Should().NotBeNull();
        dtoByAdmin.PaymentId.Should().Be(payment.PaymentId);

        // --- Other user should be blocked ---
        var other = User.TryCreate(new CreateUserRequest("OtherFirst", "OtherLast", "other@test.local", "Pass123!", CreateUserRequest.Tenant));
        mockUserRepo.Setup(r => r.GetById(other.Id)).ReturnsAsync(other);

        var act = async () => await paymentService.GetById(payment.PaymentId, other.Id);
        var ex = await act.Should().ThrowAsync<ServerResponseException>();
        ex.Which.Response.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task GetByBookingId_AllowsTenantOwnerAndAdmin_BlocksOthers()
    {
        var ctx = CreateInMemoryDbContext();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockResetRepo = new Mock<IResetCodesRepository>();
        var mockSessionRepo = new Mock<ISessionRepository>();
        var userService = new UserService(mockUserRepo.Object, mockResetRepo.Object, mockSessionRepo.Object);

        SeedListingWithOwner(ctx, out var owner, out var listing);

        // Create two tenants and payments to ensure GetByBookingId picks correct payment
        var tenantA = User.TryCreate(new CreateUserRequest("TenantAFirst", "TenantALast", "ta@test.local", "Pass123!", CreateUserRequest.Tenant));
        var tenantB = User.TryCreate(new CreateUserRequest("TenantBFirst", "TenantBLast", "tb@test.local", "Pass123!", CreateUserRequest.Tenant));
        ctx.Users.AddRange(tenantA, tenantB);

        var bA = Booking.TryCreate(new CreateBookingRequest(listing.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20))), tenantA.Id, new Money { Curr = Money.Currency.PLN, Value = 1200m });
        var bB = Booking.TryCreate(new CreateBookingRequest(listing.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60))), tenantB.Id, new Money { Curr = Money.Currency.PLN, Value = 1200m });

        ctx.Bookings.AddRange(bA, bB);

        var pA = new Payment(bA.BookingId, (Money)bA.TotalPrice.Clone());
        pA.RedirectToGateway();
        var pB = new Payment(bB.BookingId, (Money)bB.TotalPrice.Clone());
        pB.RedirectToGateway();

        ctx.Payments.AddRange(pA, pB);

        await ctx.SaveChangesAsync();

        var listingService = new ListingService(ctx, userService);
        var stripeClientMock = new Mock<IStripeClient>();
        var paymentService = new PaymentService(ctx, stripeClientMock.Object, listingService, userService);

        var dtoTenantA = await paymentService.GetByBookingId(bA.BookingId, tenantA.Id);
        dtoTenantA.PaymentId.Should().Be(pA.PaymentId);
        dtoTenantA.BookingId.Should().Be(bA.BookingId);

        // Owner can access
        var dtoOwner = await paymentService.GetByBookingId(bA.BookingId, owner.Id);
        dtoOwner.PaymentId.Should().Be(pA.PaymentId);

        // Admin can access (setup repo)
        var admin = User.TryCreate(new CreateUserRequest("AdminFirst", "AdminLast", "admin2@test.local", "Pass123!", CreateUserRequest.Admin));
        mockUserRepo.Setup(r => r.GetById(admin.Id)).ReturnsAsync(admin);
        var dtoAdmin = await paymentService.GetByBookingId(bA.BookingId, admin.Id);
        dtoAdmin.PaymentId.Should().Be(pA.PaymentId);

        // Other user should be blocked
        var other = User.TryCreate(new CreateUserRequest("OtherFirst", "OtherLast", "other2@test.local", "Pass123!", CreateUserRequest.Tenant));
        mockUserRepo.Setup(r => r.GetById(other.Id)).ReturnsAsync(other);

        var act = async () => await paymentService.GetByBookingId(bA.BookingId, other.Id);
        var ex = await act.Should().ThrowAsync<ServerResponseException>();
        ex.Which.Response.Status.Should().Be(StatusCodes.Status403Forbidden);
    }
}