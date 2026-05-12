namespace flatshare_server.Infrastructure.Model.Bookings;

public class Payment
{
    public enum PaymentStatus
    {
        Initiated,
        Redirected,
        Succeeded,
        Failed,
        Cancelled
    }
    public Guid PaymentId { get; init; }
    public Guid BookingId { get; init; }
    public Money Amount { get; init; }
    private PaymentStatus _status = PaymentStatus.Initiated;
    public PaymentStatus Status { get => _status; init => _status = value; }

    public Payment(Guid bookingId, Money amount)
    {
        PaymentId = Guid.NewGuid();
        BookingId = bookingId;
        Amount = amount;
        Status = PaymentStatus.Initiated;
    }

    public void RedirectToGateway()
    {
        if (Status != PaymentStatus.Initiated)
            throw new InvalidOperationException($"Cannot redirect from status {Status}");
        _status = PaymentStatus.Redirected;
    }

    public void GatewayConfirmed()
    {
        if (Status != PaymentStatus.Redirected)
            throw new InvalidOperationException($"Cannot confirm gateway from status {Status}");
        _status = PaymentStatus.Succeeded;
    }

    public void GatewayFailed()
    {
        if (Status != PaymentStatus.Redirected)
            throw new InvalidOperationException($"Cannot fail gateway from status {Status}");
        _status = PaymentStatus.Failed;
    }

    public void UserAborted()
    {
        if (Status != PaymentStatus.Redirected)
            throw new InvalidOperationException($"Cannot abort from status {Status}");
        _status = PaymentStatus.Cancelled;
    }

    public void Retry()
    {
        if (Status != PaymentStatus.Failed)
            throw new InvalidOperationException($"Cannot retry from status {Status}");
        _status = PaymentStatus.Initiated;
    }
}