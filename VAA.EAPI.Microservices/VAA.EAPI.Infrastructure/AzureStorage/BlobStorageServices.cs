using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Infrastructure.AzureStorage
{
    [Obsolete("Please use VAA.EAPI.Infrastructure.Services.AzureBlob.AzureBlobService")]
    public class BlobStorageServices : IBlobStorageServices
    {

        private readonly BlobClient _blobClient;

        public BlobStorageServices(string connectionString, string containerName, string blobName)
        {
            _blobClient = new BlobClient(connectionString, containerName, blobName);
        }

        private async Task<string> GetText()
        {
            BlobDownloadResult downloadResult = await _blobClient.DownloadContentAsync();
            string downloadedData = downloadResult.Content.ToString();
            return downloadedData;
        }

        public async Task<T> Deserialize<T>()
        {
            string json = await GetText();
            return Json.Deserialize<T>(json);
        }

    }

}