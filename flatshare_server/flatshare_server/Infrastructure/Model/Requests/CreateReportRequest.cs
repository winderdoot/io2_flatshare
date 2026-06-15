namespace flatshare_server.Infrastructure.Model.Requests;

public record class CreateReportRequest(
    string Type,
    Guid TargetId,
    string Reason,
    string Details
);