using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Helper
{
    public class BlobStorageHelper
    {
        public static async Task FileUploadToBlob(Stream stream, string fileName, string containerName)
        {
            var blobClient = new BlobContainerClient(ConfigReader.Get(Constants.Environment.BlobConnString), containerName);

            _ = await blobClient.CreateIfNotExistsAsync();

            var blob = blobClient.GetBlobClient(fileName);
            stream.Position = 0;

            await blob.UploadAsync(stream, overwrite: true);

        }
    }
}
