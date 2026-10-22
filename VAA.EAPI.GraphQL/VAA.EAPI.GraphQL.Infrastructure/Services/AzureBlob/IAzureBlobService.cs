using System;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Infrastructure.Services.AzureBlob
{
    public interface IAzureBlobService
    {
        Task<byte[]> BlobBytesAsync(string name, CancellationToken cancellationToken);
        Task<string> BlobStringAsync(string name, CancellationToken cancellationToken);
        Task<T> BlobJsonAsAsync<T>(string name, CancellationToken cancellationToken);
        Task<T> BlobXmlAsAsync<T>(string name, CancellationToken cancellationToken);

        Task Download(string resourceUrl, string containerName, string name, string sasKey, CancellationToken cancellationToken);
        /// <summary>
        /// The <see cref="Download(Uri, CancellationToken)"/> operation downloads a blob from the azure blob service
        /// </summary>
        /// <param name="blobUri">A <see cref="Uri"/> referencing the blob that includes the name of the account, the name of the container, and the name of the blob.
        /// This is likely to be similar to "https://{account_name}.blob.core.windows.net/{container_name}/{blob_name}".</param>
        /// <param name="name">The name of this blob.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Download(Uri blobUri, string name, CancellationToken cancellationToken);
    }
}
