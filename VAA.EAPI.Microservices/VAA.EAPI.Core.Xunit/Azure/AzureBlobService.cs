using System.Collections.Concurrent;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services.AzureBlob;

namespace VAA.EAPI.Core.Xunit.Azure
{
    public sealed class AzureBlobService : IAzureBlobService
    {
        public static Uri CreateBlobUri(string resourceUrl, string containerName, string name, string sasKey)
        {
            if (resourceUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(resourceUrl));
            if (containerName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(containerName));
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));

            sasKey ??= string.Empty;

            return new Uri($"{resourceUrl.TrimEnd('/')}/{containerName.Trim('/')}/{name.Trim('/')}?{sasKey.TrimStart('?')}");
        }

        public static Uri CreateBlobContainerUri(string resourceUrl, string containerName, string sasKey)
        {
            if (resourceUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(resourceUrl));
            if (containerName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(containerName));

            sasKey ??= string.Empty;

            return new Uri($"{resourceUrl.TrimEnd('/')}/{containerName.Trim('/')}?{sasKey.TrimStart('?')}");
        }


        private readonly ConcurrentDictionary<string, object> _blobs = new(StringComparer.OrdinalIgnoreCase);
        private readonly IHttpConnector? _httpConnector;

        public AzureBlobService() => _httpConnector = default;
        public AzureBlobService(IHttpConnector httpConnector) => _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task<byte[]> BlobBytesAsync(string name, CancellationToken cancellationToken)
        {
            if (_blobs.TryGetValue(name, out object value)) return (byte[])value;

            throw new NotImplementedException("Call AzureBlobService.RegisterBlob(name, value) first to register test blobs as part of your build up.");
        }

        public async Task<string> BlobStringAsync(string name, CancellationToken cancellationToken)
        {
            if (_blobs.TryGetValue(name, out object value)) return (string)value;

            throw new NotImplementedException("Call AzureBlobService.RegisterBlob(name, value) first to register test blobs as part of your build up.");
        }

        public async Task<T> BlobJsonAsAsync<T>(string name, CancellationToken cancellationToken)
        {
            if (_blobs.TryGetValue(name, out object value))
            {
                if (value is T typedValue)
                    return typedValue;

                if (value is string json)
                    return Json.Deserialize<T>(json);

                throw new InvalidCastException($"Blob '{name}' content type '{value.GetType().Name}' cannot be converted to '{typeof(T).Name}'.");
            }

            throw new NotImplementedException("Call AzureBlobService.RegisterBlob(name, value) first to register test blobs as part of your build up.");
        }

        public async Task<T> BlobXmlAsAsync<T>(string name, CancellationToken cancellationToken)
        {
            if (_blobs.TryGetValue(name, out object value)) return (T)value;

            throw new NotImplementedException("Call AzureBlobService.RegisterBlob(name, value) first to register test blobs as part of your build up.");
        }

        public async Task WriteToBlobAsync(string name, string content, CancellationToken cancellationToken)
        {
            if (_blobs.ContainsKey(name))
                _blobs[name] = content;
        }

        public async Task DownloadBlobAsync(string storageAccountUrl, string container, string name, string sasKey, CancellationToken cancellationToken)
        {
            if (storageAccountUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(storageAccountUrl));
            if (container.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(container));
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));
            if (sasKey.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(sasKey));

            await DownloadBlobAsync(CreateBlobUri(storageAccountUrl, container, name, sasKey), name, cancellationToken);
        }

        public async Task DownloadBlobAsync(Uri blobUri, string name, CancellationToken cancellationToken)
        {
            if (_httpConnector is null) throw new InvalidOperationException("AzureBlobService must be initialized with the IHttpConnector test dependency");

            var response = await _httpConnector.GetAsync(blobUri.ToString(), default);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var containerUri = new Uri($"{blobUri.Scheme}://{blobUri.Host}/{string.Join('/', blobUri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries)[..^1])}{blobUri.Query}");

                await UploadBlobAsync(containerUri, name, content, BlobType.Json, true, cancellationToken);
            }
        }


        public async Task UploadBlobAsync(string storageAccountUrl, string container, string sasKey, string name, string content, BlobType type, bool overwrite, CancellationToken cancellationToken)
        {
            if (storageAccountUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(storageAccountUrl));
            if (container.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(container));
            if (sasKey.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(sasKey));
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));
            if (content.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(content));
            if (type is BlobType.Binary or BlobType.Text) throw new ArgumentException("Json & Xml blob types are only supported", nameof(type));

            await UploadBlobAsync(CreateBlobContainerUri(storageAccountUrl, container, sasKey), name, content, type, overwrite, cancellationToken);
        }

        public async Task UploadBlobAsync(Uri containerUrl, string name, string content, BlobType type, bool overwrite, CancellationToken cancellationToken)
        {
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));
            if (content.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(content));
            if (type is BlobType.Binary) throw new ArgumentException("Json, Xml, and Text blob types are only supported", nameof(type));

            var key = name.ToLower();

            if (overwrite)
                _blobs.AddOrUpdate(key, content, (k, v) => content);
            else
                _blobs.TryAdd(key, content);
        }

        public async Task<IEnumerable<string>> GetBlobsAsync(string storageAccountUrl, string container, string sasKey, CancellationToken cancellationToken)
        {
            if (storageAccountUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(storageAccountUrl));
            if (container.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(container));
            if (sasKey.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(sasKey));

            return await GetBlobsAsync(CreateBlobContainerUri(storageAccountUrl, container, sasKey), cancellationToken);
        }

        public async Task<IEnumerable<string>> GetBlobsAsync(Uri containerUrl, CancellationToken cancellationToken) => await FindBlobsAsync(containerUrl, string.Empty, cancellationToken);

        public async Task<IEnumerable<string>> FindBlobsAsync(string storageAccountUrl, string container, string sasKey, string prefix, CancellationToken cancellationToken)
        {
            if (storageAccountUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(storageAccountUrl));
            if (container.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(container));
            if (sasKey.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(sasKey));

            return await FindBlobsAsync(CreateBlobContainerUri(storageAccountUrl, container, sasKey), prefix, cancellationToken);
        }

        public async Task<IEnumerable<string>> FindBlobsAsync(Uri containerUrl, string prefix, CancellationToken cancellationToken) => _blobs.Keys.Where(x => x.StartsWith(prefix));

        public async Task DeleteBlobAsync(string storageAccountUrl, string container, string sasKey, string name, CancellationToken cancellationToken)
        {
            if (storageAccountUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(storageAccountUrl));
            if (container.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(container));
            if (sasKey.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(sasKey));
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));

            await DeleteBlobAsync(CreateBlobContainerUri(storageAccountUrl, container, sasKey), name, cancellationToken);
        }

        public async Task DeleteBlobAsync(Uri containerUrl, string name, CancellationToken cancellationToken)
        {
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));

            var key = name.ToLower();

            _blobs.TryRemove(key, out var _);
        }


        [Obsolete("User DownloadAsync")]
        public async Task Download(string storageAccountUrl, string container, string name, string sasKey, CancellationToken cancellationToken) => await DownloadBlobAsync(storageAccountUrl, container, name, sasKey, cancellationToken);

        [Obsolete("User DownloadAsync")]
        public async Task Download(Uri blobUri, string name, CancellationToken cancellationToken) => await DownloadBlobAsync(blobUri, name, cancellationToken);
    }
}
