using flatshare_server.Infrastructure.Model.Responses;
using Microsoft.AspNetCore.Http;

namespace flatshare_server.Infrastructure.Model;

public class ViolationReport
{
    public enum ReportType
    {
        LISTING,
        USER
    }

    public enum ReportStatus
    {
        Open,
        UnderReview,
        ActionTaken,
        ClosedNoAction
    }

    public Guid Id { get; init; }
    public ReportType Type { get; init; }
    public Guid TargetId { get; init; }
    public string Reason { get; init; }
    public string Details { get; init; }
    public ReportStatus Status { get; private set; }
    public DateTime CreatedAt { get; init; }

    private ViolationReport() { }

    public static ViolationReport Create(ReportType type, Guid targetId, string reason, string details)
    {
        return new ViolationReport
        {
            Id = Guid.NewGuid(),
            Type = type,
            TargetId = targetId,
            Reason = reason,
            Details = details,
            Status = ReportStatus.Open,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AdminOpenCase()
    {
        if (Status != ReportStatus.Open)
        {
            throw ErrorResponse.Generate($"Cannot open case from status '{Status}'", StatusCodes.Status400BadRequest);
        }
        Status = ReportStatus.UnderReview;
    }

    public void TakeAction()
    {
        if (Status != ReportStatus.UnderReview)
        {
            throw ErrorResponse.Generate($"Cannot take action from status '{Status}'", StatusCodes.Status400BadRequest);
        }
        Status = ReportStatus.ActionTaken;
    }

    public void DismissReport()
    {
        if (Status != ReportStatus.UnderReview)
        {
            throw ErrorResponse.Generate($"Cannot dismiss report from status '{Status}'", StatusCodes.Status400BadRequest);
        }
        Status = ReportStatus.ClosedNoAction;
    }
}