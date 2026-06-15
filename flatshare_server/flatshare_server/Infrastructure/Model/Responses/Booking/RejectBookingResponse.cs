namespace flatshare_server.Infrastructure.Model.Responses.Booking;

public record RejectBookingResponse(
    string BookingId,
    string Status,
    DateTime RejectedAt,
    string Reason
);