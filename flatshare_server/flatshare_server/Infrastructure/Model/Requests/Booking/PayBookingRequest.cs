namespace flatshare_server.Infrastructure.Model.Requests.Booking;

public record PayBookingRequest(
    string PaymentMethod,
    string ReturnUrl,
    string CancelUrl
);