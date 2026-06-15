namespace flatshare_server.Infrastructure.Model.Requests.Booking;

public record CreateBookingRequest(
    Guid ListingId,
    DateOnly StartDate,
    DateOnly EndDate
);