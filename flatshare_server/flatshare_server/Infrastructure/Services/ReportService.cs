using flatshare_server.Infrastructure.Model;
using flatshare_server.Infrastructure.Model.Bookings;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace flatshare_server.Infrastructure.Services;

public class ReportService
{
    private readonly FlatshareDbContext _dbContext;
    private readonly UserService _userService;
    private readonly ListingService _listingService;
    private readonly BookingService _bookingService;
    private readonly IStripeClient _stripeClient;

    public ReportService(
        FlatshareDbContext dbContext,
        UserService userService,
        ListingService listingService,
        BookingService bookingService,
        IStripeClient stripeClient)
    {
        _dbContext = dbContext;
        _userService = userService;
        _listingService = listingService;
        _bookingService = bookingService;
        _stripeClient = stripeClient;
    }


    public async Task<ViolationReportDTO> CreateReportAsync(CreateReportRequest request)
    {
        var type = Enum.Parse<ViolationReport.ReportType>(request.Type, ignoreCase: true);
        var report = ViolationReport.Create(type, request.TargetId, request.Reason, request.Details);

        _dbContext.ViolationReports.Add(report);
        await _dbContext.SaveChangesAsync();

        return MapToDTO(report);
    }

    public async Task<PageResponse<ViolationReportDTO>> GetReportsAsync(int page, int size)
    {
        var totalElements = await _dbContext.ViolationReports.CountAsync();
        var reports = await _dbContext.ViolationReports
            .OrderBy(r => r.Status == ViolationReport.ReportStatus.Open ? 0 : 1)
            .ThenBy(r => r.CreatedAt)
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        var content = reports.Select(MapToDTO).ToList();

        return new PageResponse<ViolationReportDTO>
        {
            Content = content,
            Page = new PageMetadata
            {
                Size = size,
                Number = page,
                TotalElements = totalElements,
                TotalPages = (int)Math.Ceiling(totalElements / (double)size)
            }
        };
    }

    public async Task AdminOpenCaseAsync(Guid reportId)
    {
        var report = await GetReportByIdAsync(reportId);
        report.AdminOpenCase();
        await _dbContext.SaveChangesAsync();
    }

    public async Task DismissReportAsync(Guid reportId)
    {
        var report = await GetReportByIdAsync(reportId);
        report.DismissReport();
        await _dbContext.SaveChangesAsync();
    }

    public async Task BanUserAsync(Guid reportId, Guid userId, string reason)
    {
        var report = await GetReportByIdAsync(reportId);

        // Blocking account
        var user = await _userService.GetByIdAsync(userId);
        user.Ban(reason);

        // Hiding active listings
        var userListings = await _dbContext.Listings
            .Include(l => l.Owner)
            .Where(l => l.Owner != null && l.Owner.Id == userId && l.Status == Listing.ListingStatus.Active)
            .ToListAsync();

        foreach (var listing in userListings)
        {
            listing.HideByModeration();
        }

        // Canclelling booking and returning money (Stripe Destination Charges)
        var listingIds = userListings.Select(l => l.Id).ToList();
        var activeBookings = await _dbContext.Bookings
            .Where(b => listingIds.Contains(b.ListingId) && b.Status == Booking.BookingStatus.Confirmed)
            .ToListAsync();

        var refundService = new RefundService(_stripeClient);

        foreach (var booking in activeBookings)
        {
            booking.AdminCancel();

            // Finding payment of this booking
            var payment = await _dbContext.Payments
                .Where(p => p.BookingId == booking.BookingId && p.Status == Payment.PaymentStatus.Succeeded)
                .OrderByDescending(p => p.PaymentId)
                .FirstOrDefaultAsync();

            if (payment != null && !string.IsNullOrEmpty(payment.StripePaymentIntentId))
            {
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = payment.StripePaymentIntentId,
                    ReverseTransfer = true,
                    RefundApplicationFee = false
                };

                await refundService.CreateAsync(refundOptions);
            }
        }

        report.TakeAction();

        await _dbContext.SaveChangesAsync();
    }

    private async Task<ViolationReport> GetReportByIdAsync(Guid reportId)
    {
        var report = await _dbContext.ViolationReports.FindAsync(reportId);
        if (report is null)
        {
            throw ErrorResponse.Generate("Violation Report not found", StatusCodes.Status404NotFound);
        }
        return report;
    }

    private static ViolationReportDTO MapToDTO(ViolationReport report)
    {
        return new ViolationReportDTO(
            report.Id,
            report.Type.ToString(),
            report.TargetId,
            report.Reason,
            report.Details,
            report.Status.ToString(),
            report.CreatedAt
        );
    }
}