namespace flatshare_server.Infrastructure.Model.Responses.Booking;

public record class BookingDTO
{
    public required Guid Id { get; init; }
    public required Guid ListingId { get; init; }
    public required Guid TenantId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public required decimal TotalPrice { get; init; }
    public required string Currency { get; init; }
    public required string Status { get; init; }
    public required string PaymentId{ get; init; }
}
