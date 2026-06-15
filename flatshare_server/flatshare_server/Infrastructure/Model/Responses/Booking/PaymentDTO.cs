using flatshare_server.Infrastructure.Model.Bookings;

namespace flatshare_server.Infrastructure.Model.Responses.Booking;

public record class PaymentDTO
{
    public required Guid PaymentId { get; init; }
    public required Guid BookingId { get; init; }
    public required Payment.PaymentStatus Status { get; init; }
    public required decimal TotalValue { get; init; }
    public required string Currency { get; init; }
}
