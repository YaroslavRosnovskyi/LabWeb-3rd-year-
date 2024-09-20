using LabWeb.Services.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;


namespace LabWeb.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly IConfiguration _configuration;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName = "az-lab-web-container";

    public BlobStorageService(IConfiguration configuration, BlobServiceClient blobServiceClient)
    {
        _configuration = configuration;
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadBlob(IFormFile formFile, string imageName, string? originalBlobName = null)
    {
        var blobName = $"{imageName}{Path.GetExtension(formFile.FileName)}";
        var container = await GetBlobContainerClient();

        if (!string.IsNullOrEmpty(originalBlobName))
        {
            await RemoveBlob(originalBlobName);
        }

        using var memoryStream = new MemoryStream();
        formFile.CopyTo(memoryStream);
        memoryStream.Position = 0;
        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(content: memoryStream, overwrite: true);
        return blobName;
    }

    public async Task<string> GetBlobUrl(string imageName)
    {
        var container = await GetBlobContainerClient();

        var blob = container.GetBlobClient(imageName);

        BlobSasBuilder blobSasBuilder = new()
        {
            BlobContainerName = blob.BlobContainerName,
            BlobName = blob.Name,
            ExpiresOn = DateTime.UtcNow.AddMinutes(2),
            Protocol = SasProtocol.Https,
            Resource = "b"
        };
        blobSasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

        return blob.GenerateSasUri(blobSasBuilder).ToString();
    }

    public async Task RemoveBlob(string imageName)
    {
        var container = await GetBlobContainerClient();
        var blob = container.GetBlobClient(imageName);
        await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
    }

    private async Task<BlobContainerClient> GetBlobContainerClient()
    {
        try
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            await container.CreateIfNotExistsAsync();

            return container;
        }
        catch (Exception ex)
        {

            throw;
        }

    }
}