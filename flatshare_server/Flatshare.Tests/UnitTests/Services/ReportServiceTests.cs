using flatshare_server.Infrastructure.Model;
using flatshare_server.Infrastructure.Model.Bookings;
using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Stripe;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using flatshare_server.Infrastructure.Model.Requests.Booking;

namespace Flatshare.Tests.UnitTests.Services;

public class ReportServiceTests : IDisposable
{
    private readonly FlatshareDbContext _dbContext;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IResetCodesRepository> _resetCodesRepoMock;
    private readonly Mock<ISessionRepository> _sessionRepoMock;
    private readonly UserService _userService;
    private readonly ListingService _listingService;
    private readonly BookingService _bookingService;
    private readonly Mock<IStripeClient> _stripeClientMock;
    private readonly ReportService _reportService;

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<FlatshareDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new FlatshareDbContext(options);

        _userRepoMock = new Mock<IUserRepository>();
        _resetCodesRepoMock = new Mock<IResetCodesRepository>();
        _sessionRepoMock = new Mock<ISessionRepository>();

        _userService = new UserService(_userRepoMock.Object, _resetCodesRepoMock.Object, _sessionRepoMock.Object);
        _listingService = new ListingService(_dbContext, _userService);
        _bookingService = new BookingService(_dbContext, _listingService);
        _stripeClientMock = new Mock<IStripeClient>();

        _reportService = new ReportService(
            _dbContext,
            _userService,
            _listingService,
            _bookingService,
            _stripeClientMock.Object
        );
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task CreateReportAsync_ShouldPersistReport_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateReportRequest(
            "Listing",
            Guid.NewGuid(),
            "Inappropriate content",
            "The user uploaded duplicate promotional text."
        );

        // Act
        var result = await _reportService.CreateReportAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Reason.Should().Be(request.Reason);
        result.Details.Should().Be(request.Details);
        result.Status.Should().Be("Open");

        var stored = await _dbContext.ViolationReports.FindAsync(result.Id);
        stored.Should().NotBeNull();
        stored!.TargetId.Should().Be(request.TargetId);
    }

    [Fact]
    public async Task GetReportsAsync_ShouldReturnPaginatedReportsOrderedByOpenStatusAndCreationDate()
    {
        // Arrange
        var reportOpenOld = ViolationReport.Create(ViolationReport.ReportType.LISTING, Guid.NewGuid(), "Old Open", "Details");
        var reportOpenNew = ViolationReport.Create(ViolationReport.ReportType.LISTING, Guid.NewGuid(), "New Open", "Details");
        var reportDismissed = ViolationReport.Create(ViolationReport.ReportType.LISTING, Guid.NewGuid(), "Dismissed", "Details");

        reportDismissed.AdminOpenCase();
        reportDismissed.DismissReport();

        _dbContext.ViolationReports.AddRange(reportDismissed, reportOpenOld, reportOpenNew);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _reportService.GetReportsAsync(page: 0, size: 3);

        // Assert
        result.Content.Should().HaveCount(3);
        result.Page.TotalElements.Should().Be(3);

        result.Content.ElementAt(0).Reason.Should().Be("Old Open");
        result.Content.ElementAt(1).Reason.Should().Be("New Open");
        result.Content.ElementAt(2).Reason.Should().Be("Dismissed");
    }

    [Fact]
    public async Task AdminOpenCaseAsync_ShouldTransitionStatusToOpen_WhenReportExists()
    {
        // Arrange
        var report = ViolationReport.Create(ViolationReport.ReportType.LISTING, Guid.NewGuid(), "Reason", "Details");
        _dbContext.ViolationReports.Add(report);
        await _dbContext.SaveChangesAsync();

        // Act
        await _reportService.AdminOpenCaseAsync(report.Id);

        // Assert
        var stored = await _dbContext.ViolationReports.FindAsync(report.Id);
        stored!.Status.Should().Be(ViolationReport.ReportStatus.UnderReview);
    }

    [Fact]
    public async Task DismissReportAsync_ShouldTransitionStatusToDismissed_WhenReportExists()
    {
        // Arrange
        var report = ViolationReport.Create(ViolationReport.ReportType.LISTING, Guid.NewGuid(), "Reason", "Details");
        _dbContext.ViolationReports.Add(report);
        await _dbContext.SaveChangesAsync();
        report.AdminOpenCase();

        // Act
        await _reportService.DismissReportAsync(report.Id);

        // Assert
        var stored = await _dbContext.ViolationReports.FindAsync(report.Id);
        stored!.Status.Should().Be(ViolationReport.ReportStatus.ClosedNoAction);
    }

    [Fact]
    public async Task GetReportByIdAsync_ShouldThrow404_WhenReportDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = async () => await _reportService.AdminOpenCaseAsync(nonExistentId);

        // Assert
        var exception = await act.Should().ThrowAsync<ServerResponseException>();
        exception.Which.Response.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task BanUserAsync_ShouldExecuteFullModerationWorkflow_WhenCalledWithValidData()
    {
        // Arrange
        var targetUser = User.TryCreate(new CreateUserRequest("Malicious", "User", "badactor@test.local", "Pass123!", CreateUserRequest.Tenant));
        _dbContext.Users.Add(targetUser);

        var report = ViolationReport.Create(ViolationReport.ReportType.USER, targetUser.Id, "Fraudulent Behavior", "Details");
        report.AdminOpenCase();
        _dbContext.ViolationReports.Add(report);

        _userRepoMock.Setup(r => r.GetById(targetUser.Id)).ReturnsAsync(targetUser);

        var listingReq = new CreateListingRequest
        {
            Title = "title",
            Description = "description",
            Price = 10,
            Currency = "PLN",
            AvailableSince = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            AvailableUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            OwnerContact = "contact",
            Area = 10,
            Location = new flatshare_server.Infrastructure.Model.Listings.Address("city", "distr", "street", "number"),
            Attributes = new ListingAttributes { Profile = ListingAttributes.TenantProfile.None }
        };
        var listing = Listing.TryCreate(listingReq, targetUser);
        listing.SubmitForReview();
        listing.Approve();
        _dbContext.Listings.Add(listing);
        var moneyAmount = new Money { Value = 3000, Curr = Money.Currency.PLN };

        var bookingReq = new CreateBookingRequest(listing.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)));
        var booking = Booking.TryCreate(bookingReq, targetUser.Id, moneyAmount);
        booking.OwnerAccept();
        booking.PaymentSuccess();
        _dbContext.Bookings.Add(booking);

        var payment = new Payment(booking.BookingId, moneyAmount);
        _dbContext.Payments.Add(payment);

        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        _stripeClientMock
            .Setup(c => c.RequestAsync<Refund>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<BaseOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Refund());

        // Act
        var banReason = "Violated Terms of Service";
        await _reportService.BanUserAsync(report.Id, targetUser.Id, banReason);

        // Assert
        targetUser!.Status.Value.Should().Be(AccountStatus.Type.Blocked);
        targetUser.Status.Reason.Should().Be(banReason);

        var storedListing = await _dbContext.Listings.FindAsync(listing.Id);
        storedListing!.Status.Should().Be(Listing.ListingStatus.HiddenByModeration);

        var storedBooking = await _dbContext.Bookings.FindAsync(booking.BookingId);
        storedBooking!.Status.Should().Be(Booking.BookingStatus.Cancelled);

        var storedReport = await _dbContext.ViolationReports.FindAsync(report.Id);
        storedReport!.Status.Should().Be(ViolationReport.ReportStatus.ActionTaken);
    }
}