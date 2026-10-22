using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.Services.AzureBlob
{
    public interface IAzureBlobService
    {
        Task<byte[]> BlobBytesAsync(string name, CancellationToken cancellationToken);

        Task<string> BlobStringAsync(string name, CancellationToken cancellationToken);

        Task<T> BlobJsonAsAsync<T>(string name, CancellationToken cancellationToken);

        Task<T> BlobXmlAsAsync<T>(string name, CancellationToken cancellationToken);

        Task WriteToBlobAsync(string name, string content, CancellationToken cancellationToken);

        Task DownloadBlobAsync(string storageAccountUrl, string container, string name, string sasKey, CancellationToken cancellationToken);

        Task DownloadBlobAsync(Uri blobUri, string name, CancellationToken cancellationToken);


        Task UploadBlobAsync(string storageAccountUrl, string container, string sasKey, string name, string content, BlobType type, bool overwrite, CancellationToken cancellationToken);

        Task UploadBlobAsync(Uri containerUrl, string name, string content, BlobType type, bool overwrite, CancellationToken cancellationToken);

        Task<IEnumerable<string>> GetBlobsAsync(string storageAccountUrl, string container, string sasKey, CancellationToken cancellationToken);

        Task<IEnumerable<string>> GetBlobsAsync(Uri containerUrl, CancellationToken cancellationToken);

        Task<IEnumerable<string>> FindBlobsAsync(string storageAccountUrl, string container, string sasKey, string prefix, CancellationToken cancellationToken);

        Task<IEnumerable<string>> FindBlobsAsync(Uri containerUrl, string prefix, CancellationToken cancellationToken);

        Task DeleteBlobAsync(string storageAccountUrl, string container, string sasKey, string name, CancellationToken cancellationToken);

        Task DeleteBlobAsync(Uri containerUrl, string name, CancellationToken cancellationToken);


        [Obsolete("User DownloadAsync")]
        Task Download(string storageAccountUrl, string container, string name, string sasKey, CancellationToken cancellationToken);

        [Obsolete("User DownloadAsync")]
        Task Download(Uri blobUri, string name, CancellationToken cancellationToken);
    }
}
