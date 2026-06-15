namespace flatshare_server.Infrastructure.Model.Responses.Booking;

public record CancelBookingResponse(
    string BookingId,
    string Status,
    DateTime CancelledAt,
    string RefundStatus
);