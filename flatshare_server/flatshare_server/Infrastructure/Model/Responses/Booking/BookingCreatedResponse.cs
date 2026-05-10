namespace flatshare_server.Infrastructure.Model.Responses.Booking;

public record BookingCreatedResponse(
    string BookingId,
    string Status,
    DateTime CreatedAt,
    decimal TotalPrice,
    string Currency,
    string ResourceLink
);
