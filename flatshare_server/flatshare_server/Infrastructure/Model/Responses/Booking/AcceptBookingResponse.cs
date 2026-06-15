namespace flatshare_server.Infrastructure.Model.Responses.Booking;

public record AcceptBookingResponse(
    string BookingId,
    string Status,
    DateTime AcceptedAt,
    DateTime PaymentRequiredUntil
);