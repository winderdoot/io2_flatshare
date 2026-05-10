namespace flatshare_server.Infrastructure.Model.Responses.Booking;

public record PaymentInitiatedResponse(
    string PaymentId,
    string BookingId,
    string Status,
    string RedirectUrl,
    decimal Amount,
    string Currency
);