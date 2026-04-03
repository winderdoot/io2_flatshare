namespace flatshare_server.Infrastructure.Services;

public interface IStorageService
{
    Task<Guid> StoreFileAsync(IFormFile file);
    Task<(Stream? Content, string? ContentType)> GetFileAsync(Guid fileId);
    Task<bool> DeleteFileAsync(Guid fileId);
}