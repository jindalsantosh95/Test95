using Azure.Storage.Blobs;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL.Infrastructure.Services.AzureBlob
{
    public sealed class AzureBlobService : IAzureBlobService
    {
        internal sealed class BlobConnection
        {
            public static explicit operator BlobConnection(BlobClient blobClient) => new(blobClient) { };

            private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
            private readonly BlobClient _blobClient;

            public string Key => _blobClient?.Uri.ToString().ToLower();
            public string Name => _blobClient?.Name;
            /// <summary>
            /// The <see cref="Exists"/> operation can be called on a <see cref="BlobConnection"/> to see if the 
            /// associated blob exists in the container on the storage account in the storage service.
            /// </summary>
            /// <returns>Returns true if the blob exists.</returns>
            public bool Exists
            {
                get
                {
                    try
                    {
                        return _blobClient.Exists(CancellationToken.None);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            public TimeSpan Ttl { get; private set; }
            public BinaryData Content { get; private set; }
            public DateTime LastDownloaded { get; private set; }
            public bool HasExpired => DateTime.UtcNow >= LastDownloaded.Add(Ttl);

            private BlobConnection(BlobClient blobClient)
            {
                _blobClient = blobClient;

                LastDownloaded = DateTime.MinValue;
                Ttl = ConfigReader.Try(Common.Constants.Environment.CommonCachingAbsoluteExpirationInDay, TimeSpan.FromDays(1));

                // Constants.Environment.CommonCachingSlidingExpirationInDay
            }

            public async Task<BinaryData> DownloadContentAsync(CancellationToken cancellationToken)
            {
                if (!HasExpired) return Content;

                try
                {
                    await _semaphoreSlim.WaitAsync(cancellationToken);

                    if (!HasExpired) return Content;
                    if (!_blobClient.Exists(cancellationToken)) throw new InvalidOperationException($"Resource, {Key}, cannot be found in blob storage, please check resource exists");

                    Content = (await _blobClient.DownloadContentAsync(cancellationToken)).Value.Content;
                    LastDownloaded = DateTime.UtcNow;
                }
                finally
                {
                    _semaphoreSlim.Release();
                }

                return Content;
            }

            public async Task<T> ContentAsAsync<T>(BlobType type, CancellationToken cancellationToken)
            {
                if (type is BlobType.Binary or BlobType.Text) throw new ArgumentException("Json & Xml blob types are only supported", nameof(type));

                var binaryData = await DownloadContentAsync(cancellationToken);

                switch (type)
                {
                    case BlobType.Json:

                        var json = binaryData.ToString();

                        return Json.Deserialize<T>(json);

                    case BlobType.Xml:

                        var xml = binaryData.ToString();

                        return Xml.Deserialize<T>(xml);
                }

                return default;
            }
            public async Task<string> ContentAsStringAsync(CancellationToken cancellationToken) => (await DownloadContentAsync(cancellationToken)).ToString();
            public async Task<byte[]> ContentAsBytesAsync(CancellationToken cancellationToken) => (await DownloadContentAsync(cancellationToken)).ToArray();
        }

        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, BlobConnection> _blobConnections = new();

        private static async Task<BlobConnection> ConnectToBlob(Uri uri, CancellationToken cancellationToken)
        {
            var key = uri.ToString().ToLower();

            if (_blobConnections.TryGetValue(key, out BlobConnection value))
            {
                try
                {
                    // lets just check we can access the resource on the current client
                    if (!value.Exists) throw new NullReferenceException();

                    return value;
                }
                catch
                {
                    _blobConnections.TryRemove(key, out _);
                }
            }

            try
            {
                await _semaphoreSlim.WaitAsync(cancellationToken);

                if (_blobConnections.TryGetValue(key, out value)) return value;

                var blobConnection = (BlobConnection)new BlobClient(uri);

                _blobConnections.AddOrUpdate(key, blobConnection, (k, v) => blobConnection);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return _blobConnections[key];
        }

        public async Task<byte[]> BlobBytesAsync(string name, CancellationToken cancellationToken) => await _blobConnections.Values.FirstOrDefault(x => x.Name.Is(name))?.ContentAsBytesAsync(cancellationToken) ?? default;
        public async Task<string> BlobStringAsync(string name, CancellationToken cancellationToken) => await _blobConnections.Values.FirstOrDefault(x => x.Name.Is(name))?.ContentAsStringAsync(cancellationToken) ?? default;
        public async Task<T> BlobJsonAsAsync<T>(string name, CancellationToken cancellationToken) => await _blobConnections.Values.FirstOrDefault(x => x.Name.Is(name))?.ContentAsAsync<T>(BlobType.Json, cancellationToken) ?? default;
        public async Task<T> BlobXmlAsAsync<T>(string name, CancellationToken cancellationToken) => await _blobConnections.Values.FirstOrDefault(x => x.Name.Is(name))?.ContentAsAsync<T>(BlobType.Xml, cancellationToken) ?? default;

        public async Task Download(string resourceUrl, string containerName, string name, string sasKey, CancellationToken cancellationToken) => await Download(CreateBlobUri(resourceUrl, containerName, name, sasKey), name, cancellationToken);
        public async Task Download(Uri blobUri, string name, CancellationToken cancellationToken)
        {
            var key = blobUri.ToString().ToLower();

            if (!_blobConnections.TryGetValue(key, out BlobConnection blobConnection))
                blobConnection = await ConnectToBlob(blobUri, cancellationToken);

            _ = await blobConnection.DownloadContentAsync(cancellationToken);
        }

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
