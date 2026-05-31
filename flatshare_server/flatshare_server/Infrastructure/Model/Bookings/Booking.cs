using flatshare_server.Infrastructure.Model.Requests.Booking;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Responses.Booking;

namespace flatshare_server.Infrastructure.Model.Bookings;
public class Booking
{
    public enum BookingStatus
    {
        PendingApproval,
        PendingPayment,
        Confirmed,
        Rejected,
        Expired,
        PaymentFailed,
        Cancelled
    }

    public required Guid BookingId { get; init; }
    public required Guid ListingId { get; init; }
    public required Guid TenantId { get; init; }

    private BookingStatus _status;
    public required BookingStatus Status { get => _status; init => _status = value; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public required Money TotalPrice { get; init; }
    public required DateTime CreatedAt { get; init; }
    public bool IsPendingPayment => Status == BookingStatus.PendingPayment;
    private Guid? _paymentId = null;
    public Guid? PaymentId { get => _paymentId; }
    public void SetPaymentID(Guid paymentId)
    {
        if (PaymentId != null)
            throw ErrorResponse.Generate($"Booking: Cannot set payment ID when it's already set");
        _paymentId = paymentId;
    }
    private Booking() { }

    public static Booking TryCreate(CreateBookingRequest request, Guid tenantId, Money totalPrice)
    {
        var errors = new List<FieldError>();

        if (request.EndDate <= request.StartDate)
        {
            errors.Add(new FieldError(nameof(request.EndDate), $"'{nameof(request.EndDate)}' date must be later than '{nameof(request.StartDate)}'"));
            errors.Add(new FieldError(nameof(request.StartDate), $"'{nameof(request.StartDate)}' date must be earlier than '{nameof(request.EndDate)}'"));
        }

        if (errors.Any())
        {
            throw ErrorResponse.Generate("Booking Error", StatusCodes.Status400BadRequest, errors);
        }

        var booking = new Booking
        {
            BookingId = Guid.NewGuid(),
            ListingId = request.ListingId,
            TenantId = tenantId,
            Status = BookingStatus.PendingApproval,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalPrice = totalPrice,
            CreatedAt = DateTime.UtcNow
        };

        return booking;
    }

    public void TimeoutNoResponse()
    {
        if (Status != BookingStatus.PendingApproval)
            throw ErrorResponse.Generate($"Booking: Cannot timeout from status {Status}");
        _status = BookingStatus.Expired;
    }

    public void OwnerReject()
    {
        if (Status != BookingStatus.PendingApproval)
            throw ErrorResponse.Generate($"Booking: Cannot reject from status {Status}");
        _status = BookingStatus.Rejected;
    }

    public void OwnerAccept()
    {
        if (Status != BookingStatus.PendingApproval)
            throw ErrorResponse.Generate($"Booking: Cannot accept from status {Status}");
        _status = BookingStatus.PendingPayment;
    }

    public void PaymentTimeout()
    {
        if (Status != BookingStatus.PendingPayment)
            throw ErrorResponse.Generate($"Booking: Cannot timeout payment from status {Status}");
        _status = BookingStatus.Expired;
    }

    public void PaymentSuccess()
    {
        if (Status != BookingStatus.PendingPayment)
            throw ErrorResponse.Generate($"Booking: Cannot confirm payment from status {Status}");
        _status = BookingStatus.Confirmed;
    }

    public void PaymentFailure()
    {
        if (Status != BookingStatus.PendingPayment)
            throw ErrorResponse.Generate($"Booking: Cannot fail payment from status {Status}");
        _status = BookingStatus.PaymentFailed;
    }

    public void RetryPayment()
    {
        if (Status != BookingStatus.PaymentFailed)
            throw ErrorResponse.Generate($"Booking: Cannot retry payment from status {Status}");
        _status = BookingStatus.PendingPayment;
    }

    public void CancelAfterFailure()
    {
        if (Status != BookingStatus.PaymentFailed)
            throw ErrorResponse.Generate($"Booking: Cannot cancel after failure from status {Status}");
        _status = BookingStatus.Cancelled;
    }

    public void AvailabilityConflict()
    {
        if (Status != BookingStatus.PendingPayment && Status != BookingStatus.Confirmed)
            throw ErrorResponse.Generate($"Booking: Cannot resolve conflict from status {Status}");
        _status = BookingStatus.Cancelled;
    }

    public void TenantCancel()
    {
        if (Status != BookingStatus.PendingApproval &&
            Status != BookingStatus.PendingPayment &&
            Status != BookingStatus.Confirmed)
        {
            throw ErrorResponse.Generate($"Booking: Cannot cancel from status {Status}");
        }
        _status = BookingStatus.Cancelled;
    }

    public void AdminCancel()
    {
        _status = BookingStatus.Cancelled;
    }

    public BookingDTO IntoDTO()
    {
        return new BookingDTO
        {
            Id = BookingId,
            ListingId = ListingId,
            TenantId = TenantId,
            StartDate = StartDate,
            EndDate = EndDate,
            TotalPrice = TotalPrice.Value,
            Currency = TotalPrice.CurrencyStr(),
            Status = Status.ToString(),
            PaymentId = PaymentId?.ToString() ?? ""
        };
    }
}