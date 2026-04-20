namespace flatshare_server.Infrastructure.Model.Responses;

public class PageResponse<T>
{
    public required List<T> Content { get; init; }
    public required PageMetadata Page { get; init; }
}

public class PageMetadata
{
    public int Size { get; init; }
    public int Number { get; init; }
    public long TotalElements { get; init; }
    public int TotalPages { get; init; }
}