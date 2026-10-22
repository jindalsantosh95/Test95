using Azure.Storage.Blobs;
using System.IO;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper
{
    public class BlobStorageHelper
    {
        public static async Task<string> FileUploadToBlob(Stream stream, string fileName, string containerName)
        {
            var blobClient = new BlobContainerClient(ConfigReader.Get(Constants.Environment.BlobConnStringForDownloads), containerName);

            _ = blobClient.CreateIfNotExistsAsync();

            var blob = blobClient.GetBlobClient($"{fileName}.xlsx");
            stream.Position = 0;

            await blob.UploadAsync(stream, overwrite: true);

            return blob.Uri.AbsoluteUri;
        }
    }
}
