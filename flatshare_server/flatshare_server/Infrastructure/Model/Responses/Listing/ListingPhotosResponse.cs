namespace flatshare_server.Infrastructure.Model.Responses;

public record ListingPhotosResponse(Guid ListingId, List<Guid> Photos);
