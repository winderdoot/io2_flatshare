using flatshare_server.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace Flatshare.Tests.Utils;

public class FakeStorageService : IStorageService
{
    private readonly ConcurrentDictionary<Guid, (byte[] Content, string ContentType)> _files = new();

    public async Task<Guid> StoreFileAsync(IFormFile file)
    {
        var id = Guid.NewGuid();

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        _files[id] = (ms.ToArray(), file.ContentType);

        return id;
    }

    public Task<(Stream? Content, string? ContentType)> GetFileAsync(Guid fileId)
    {
        if (_files.TryGetValue(fileId, out var fileData))
        {
            var stream = new MemoryStream(fileData.Content);
            return Task.FromResult<(Stream?, string?)>((stream, fileData.ContentType));
        }

        return Task.FromResult<(Stream?, string?)>((null, null));
    }

    public Task<bool> DeleteFileAsync(Guid fileId)
    {
        return Task.FromResult(_files.TryRemove(fileId, out _));
    }
}