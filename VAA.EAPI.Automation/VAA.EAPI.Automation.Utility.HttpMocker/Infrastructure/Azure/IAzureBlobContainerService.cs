using Azure.Storage.Blobs.Models;

namespace VAA.EAPI.Automation.Utility.HttpMocker.Infrastructure.Azure
{
    public interface IAzureBlobContainerService
    {
        IDictionary<string, BlobDownloadResult> Items { get; }
        BlobDownloadResult? TryGetBlob(string name);

        Task<bool> SynchroniseBlob(string blobName);

        Task Synchronise();
    }
}
