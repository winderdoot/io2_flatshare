using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;

namespace flatshare_server.Infrastructure.Services;

public class BlobStorageService : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "photos";

    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<Guid> StoreFileAsync(IFormFile file)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync();

        var fileId = Guid.NewGuid();
        var blobClient = containerClient.GetBlobClient($"{fileId}");

        using (var stream = file.OpenReadStream())
        {
            var blobUploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
            };

            await blobClient.UploadAsync(stream, blobUploadOptions);
        }

        return fileId;
    }

    public async Task<(Stream? Content, string? ContentType)> GetFileAsync(Guid fileId)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            var blobClient = containerClient.GetBlobClient($"{fileId}");

            var response = await blobClient.DownloadStreamingAsync();

            return (response.Value.Content, response.Value.Details.ContentType);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return (null, null);
        }
    }

    public async Task<bool> DeleteFileAsync(Guid fileId)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient($"{fileId}");

        var response = await blobClient.DeleteIfExistsAsync();

        return response?.Value ?? false;
    }
}
