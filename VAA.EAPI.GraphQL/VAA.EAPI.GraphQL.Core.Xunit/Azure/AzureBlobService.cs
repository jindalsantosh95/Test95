using System.Collections.Concurrent;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Infrastructure.Services.AzureBlob;

namespace VAA.EAPI.GraphQL.Core.Xunit.Azure
{
    public sealed class AzureBlobService : IAzureBlobService
    {
        private readonly ConcurrentDictionary<string, object> _blobs = [];

        public Task<byte[]> BlobBytesAsync(string name, CancellationToken cancellationToken)
        {
            if (_blobs.TryGetValue(name, out object value)) return Task.FromResult((byte[])value);

            throw new NotImplementedException("Call AzureBlobService.RegisterBlob(name, value) first to register test blobs as part of your build up.");
        }
        public Task<string> BlobStringAsync(string name, CancellationToken cancellationToken)
        {
            if (_blobs.TryGetValue(name, out object value)) return Task.FromResult((string)value);

            throw new NotImplementedException("Call AzureBlobService.RegisterBlob(name, value) first to register test blobs as part of your build up.");
        }
        public Task<T> BlobJsonAsAsync<T>(string name, CancellationToken cancellationToken)
        {
            if (_blobs.TryGetValue(name, out object value)) return Task.FromResult((T)value);

            throw new NotImplementedException("Call AzureBlobService.RegisterBlob(name, value) first to register test blobs as part of your build up.");
        }
        public Task<T> BlobXmlAsAsync<T>(string name, CancellationToken cancellationToken)
        {
            if (_blobs.TryGetValue(name, out object value)) return Task.FromResult((T)value);

            throw new NotImplementedException("Call AzureBlobService.RegisterBlob(name, value) first to register test blobs as part of your build up.");
        }

        public async Task Download(string resourceUrl, string containerName, string name, string sasKey, CancellationToken cancellationToken) => await Download(CreateBlobUri(resourceUrl, containerName, name, sasKey), name, cancellationToken);
        public async Task Download(Uri blobUri, string name, CancellationToken cancellationToken)
        {
            // Empty implementation as blobs should be registered during the build up phase of unit tests
        }

        public void RegisterBlob<T>(string name, T value) => _blobs.AddOrUpdate(name, value, (k, v) => value);

        public static Uri CreateBlobUri(string resourceUrl, string containerName, string name, string sasKey)
        {
            if (resourceUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(resourceUrl));
            if (containerName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(containerName));
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));

            sasKey ??= string.Empty;

            return new Uri($"{resourceUrl}/{containerName}/{name}{sasKey}");
        }
    }
}
