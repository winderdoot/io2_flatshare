namespace flatshare_server.Infrastructure.Model.Responses;

public record class PageResponse<T>
{
    public required List<T> Content { get; init; }
    public required PageMetadata Page { get; init; }
}

public record class PageMetadata
{
    public required int Size { get; init; }
    public required int Number { get; init; }
    public required long TotalElements { get; init; }
    public required int TotalPages { get; init; }
}