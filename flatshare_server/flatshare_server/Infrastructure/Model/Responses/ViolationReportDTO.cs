namespace flatshare_server.Infrastructure.Model.Responses;

public record class ViolationReportDTO(
    Guid Id,
    string Type,
    Guid TargetId,
    string Reason,
    string Details,
    string Status,
    DateTime CreatedAt
);