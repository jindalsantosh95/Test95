using System.Collections.Concurrent;
using System.Net.Mime;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Serialisation;

namespace VAA.EAPI.Automation.Core.Helpers.StorageAccount.Blob
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
                Ttl = ConfigReader.Try(Constants.Environment.CommonCachingAbsoluteExpirationInDay, TimeSpan.FromDays(1));

                // Constants.Environment.CommonCachingSlidingExpirationInDay
            }

            public async Task<BinaryData> DownloadContentAsync(CancellationToken cancellationToken)
            {
                if (!HasExpired)
                    return Content;

                try
                {
                    await _semaphoreSlim.WaitAsync(cancellationToken);

                    if (!HasExpired)
                        return Content;
                    if (!_blobClient.Exists(cancellationToken))
                        throw new InvalidOperationException($"Resource, {Key}, cannot be found in blob storage, please check resource exists");

                    Content = (await _blobClient.DownloadContentAsync(cancellationToken)).Value.Content;
                    LastDownloaded = DateTime.UtcNow;
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    _semaphoreSlim.Release();
                }

                return Content;
            }

            public async Task<T> ContentAsAsync<T>(BlobType type, CancellationToken cancellationToken)
            {
                if (type is BlobType.Binary or BlobType.Text)
                    throw new ArgumentException("Json & Xml blob types are only supported", nameof(type));

                System.BinaryData binaryData = await DownloadContentAsync(cancellationToken);

                switch (type)
                {
                    case BlobType.Json:

                        string json = binaryData.ToString();

                        return Json.Deserialize<T>(json);

                    case BlobType.Xml:

                        string xml = binaryData.ToString();

                        return Serialisation.Xml.Deserialize<T>(xml);
                }

                return default;
            }
            public async Task<string> ContentAsStringAsync(CancellationToken cancellationToken) => (await DownloadContentAsync(cancellationToken)).ToString();
            public async Task<byte[]> ContentAsBytesAsync(CancellationToken cancellationToken) => (await DownloadContentAsync(cancellationToken)).ToArray();

            public async Task WriteAsync(string content, CancellationToken cancellationToken)
            {
                if (content.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(content));

                await _blobClient.OpenWriteAsync(true, cancellationToken: cancellationToken);

                Content = (await _blobClient.DownloadContentAsync(cancellationToken)).Value.Content;
                LastDownloaded = DateTime.UtcNow;
            }
        }
        internal sealed class BlobContainerConnection
        {
            public static explicit operator BlobContainerConnection(BlobContainerClient blobContainerClient) => new(blobContainerClient) { };

            private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
            private readonly BlobContainerClient _blobContainerClient;

            public string Key => _blobContainerClient?.Uri.ToString().ToLower();
            public string Name => _blobContainerClient?.Name;
            /// <summary>
            /// The <see cref="Exists"/> operation can be called on a <see cref="BlobContainerConnection"/> to see if the 
            /// container exists in the storage account in the storage service.
            /// </summary>
            /// <returns>Returns true if the blob container exists.</returns>
            public bool Exists
            {
                get
                {
                    try
                    {
                        return _blobContainerClient.Exists(CancellationToken.None);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            private BlobContainerConnection(BlobContainerClient blobContainerClient)
            {
                _blobContainerClient = blobContainerClient;
            }

            public async Task UploadAsync(string name, string content, BlobType type, CancellationToken cancellationToken)
            {
                if (name.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(name));
                if (content.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(content));

                try
                {
                    await _semaphoreSlim.WaitAsync(cancellationToken);

                    string mediaType = type == BlobType.Json ? MediaTypeNames.Application.Json : MediaTypeNames.Application.Xml;
                    System.BinaryData binaryData = new BinaryData(UTF32Encoding.UTF8.GetBytes(content), mediaType);

                    await _blobContainerClient.UploadBlobAsync(name, binaryData, cancellationToken);
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }

            public async Task<IEnumerable<string>> GetBlobsAsync(CancellationToken cancellationToken) => await FindBlobsAsync(string.Empty, cancellationToken);

            public async Task<IEnumerable<string>> FindBlobsAsync(string prefix, CancellationToken cancellationToken)
            {
                try
                {
                    List<BlobItem> blobs = [];

                    await foreach (BlobItem blobItem in _blobContainerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
                        blobs.Add(blobItem);

                    return blobs.OrderByDescending(b => b.Properties.LastModified)
                                .Select(x => x.Name);
                }
                catch { }

                return [];
            }
        }

        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, BlobConnection> _blobConnections = new();
        private static readonly ConcurrentDictionary<string, BlobContainerConnection> _blobContainerConnections = new();

        private static async Task<BlobConnection> ConnectToBlob(Uri uri, CancellationToken cancellationToken)
        {
            string key = uri.ToString().ToLower();

            if (_blobConnections.TryGetValue(key, out BlobConnection value))
            {
                try
                {
                    // lets just check we can access the resource on the current client
                    if (!value.Exists)
                        throw new NullReferenceException();

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

                if (_blobConnections.TryGetValue(key, out value))
                    return value;

                VAA.EAPI.Automation.Core.Helpers.StorageAccount.Blob.AzureBlobService.BlobConnection blobConnection = (BlobConnection)new BlobClient(uri);

                _blobConnections.AddOrUpdate(key, blobConnection, (k, v) => blobConnection);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return _blobConnections[key];
        }
        private static async Task<BlobContainerConnection> ConnectToContainer(Uri uri, CancellationToken cancellationToken)
        {
            string key = uri.ToString().ToLower();

            if (_blobContainerConnections.TryGetValue(key, out BlobContainerConnection value))
            {
                try
                {
                    // lets just check we can access the resource on the current client
                    if (!value.Exists)
                        throw new NullReferenceException();

                    return value;
                }
                catch
                {
                    _blobContainerConnections.TryRemove(key, out _);
                }
            }

            try
            {
                await _semaphoreSlim.WaitAsync(cancellationToken);

                if (_blobContainerConnections.TryGetValue(key, out value))
                    return value;

                VAA.EAPI.Automation.Core.Helpers.StorageAccount.Blob.AzureBlobService.BlobContainerConnection blobContainerConnection = (BlobContainerConnection)new BlobContainerClient(uri);

                _blobContainerConnections.AddOrUpdate(key, blobContainerConnection, (k, v) => blobContainerConnection);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return _blobContainerConnections[key];
        }

        public static Uri CreateBlobUri(string resourceUrl, string containerName, string name, string sasKey)
        {
            if (resourceUrl.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(resourceUrl));
            if (containerName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(containerName));
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(name));

            sasKey ??= string.Empty;

            return new Uri($"{resourceUrl}/{containerName}/{name}?{sasKey.TrimStart('?')}");
        }

        public static Uri CreateBlobContainerUri(string resourceUrl, string containerName, string sasKey)
        {
            if (resourceUrl.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(resourceUrl));
            if (containerName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(containerName));

            sasKey ??= string.Empty;

            return new Uri($"{resourceUrl}/{containerName}?{sasKey.TrimStart('?')}");
        }

        public async Task<byte[]> BlobBytesAsync(string name, CancellationToken cancellationToken) => await _blobConnections.Values.FirstOrDefault(x => x.Name.Is(name))?.ContentAsBytesAsync(cancellationToken) ?? default;

        public async Task<string> BlobStringAsync(string name, CancellationToken cancellationToken) => await _blobConnections.Values.FirstOrDefault(x => x.Name.Is(name))?.ContentAsStringAsync(cancellationToken) ?? default;

        public async Task<T> BlobJsonAsAsync<T>(string name, CancellationToken cancellationToken) => await _blobConnections.Values.FirstOrDefault(x => x.Name.Is(name))?.ContentAsAsync<T>(BlobType.Json, cancellationToken) ?? default;

        public async Task<T> BlobXmlAsAsync<T>(string name, CancellationToken cancellationToken) => await _blobConnections.Values.FirstOrDefault(x => x.Name.Is(name))?.ContentAsAsync<T>(BlobType.Xml, cancellationToken) ?? default;

        public async Task WriteToBlobAsync(string name, string content, CancellationToken cancellationToken) => await _blobConnections.Values.FirstOrDefault(x => x.Name.Is(name))?.WriteAsync(content, cancellationToken);

        public async Task DownloadBlobAsync(string storageAccountUrl, string container, string name, string sasKey, CancellationToken cancellationToken)
        {
            if (storageAccountUrl.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(storageAccountUrl));
            if (container.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(container));
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(name));
            if (sasKey.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sasKey));

            await DownloadBlobAsync(CreateBlobUri(storageAccountUrl, container, name, sasKey), name, cancellationToken);
        }

        public async Task DownloadBlobAsync(Uri blobUri, string name, CancellationToken cancellationToken)
        {
            string key = blobUri.ToString().ToLower();

            if (!_blobConnections.TryGetValue(key, out BlobConnection blobConnection))
                blobConnection = await ConnectToBlob(blobUri, cancellationToken);

            _ = await blobConnection.DownloadContentAsync(cancellationToken);
        }


        public async Task UploadBlobAsync(string storageAccountUrl, string container, string sasKey, string name, string content, BlobType type, CancellationToken cancellationToken)
        {
            if (storageAccountUrl.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(storageAccountUrl));
            if (container.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(container));
            if (sasKey.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sasKey));
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(name));
            if (content.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(content));
            if (type is BlobType.Binary or BlobType.Text)
                throw new ArgumentException("Json & Xml blob types are only supported", nameof(type));

            await UploadBlobAsync(CreateBlobContainerUri(storageAccountUrl, container, sasKey), name, content, type, cancellationToken);
        }

        public async Task UploadBlobAsync(Uri containerUrl, string name, string content, BlobType type, CancellationToken cancellationToken)
        {
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(name));
            if (content.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(content));
            if (type is BlobType.Binary or BlobType.Text)
                throw new ArgumentException("Json & Xml blob types are only supported", nameof(type));

            string key = containerUrl.ToString().ToLower();

            if (!_blobContainerConnections.TryGetValue(key, out BlobContainerConnection blobContainerConnection))
                blobContainerConnection = await ConnectToContainer(containerUrl, cancellationToken);

            await blobContainerConnection.UploadAsync(name, content, type = BlobType.Json, cancellationToken);

            string[] resource = key.Split('?');
            string[] resourceParts = resource[0].Split('/', StringSplitOptions.RemoveEmptyEntries);
            string storage = string.Join('/', resourceParts.Take(resourceParts.Length - 1)).Replace(":/", "://");
            string container = resourceParts.Last();
            string sasKey = resource.Length == 2 ? resource[1] : string.Empty;

            //await DownloadBlobAsync(storage, container, name, sasKey, cancellationToken);
        }

        public async Task<IEnumerable<string>> GetBlobsAsync(string storageAccountUrl, string container, string sasKey, CancellationToken cancellationToken)
        {
            if (storageAccountUrl.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(storageAccountUrl));
            if (container.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(container));
            if (sasKey.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sasKey));

            return await GetBlobsAsync(CreateBlobContainerUri(storageAccountUrl, container, sasKey), cancellationToken);
        }

        public async Task<IEnumerable<string>> GetBlobsAsync(Uri containerUrl, CancellationToken cancellationToken)
        {
            string key = containerUrl.ToString().ToLower();

            if (!_blobContainerConnections.TryGetValue(key, out BlobContainerConnection blobContainerConnection))
                blobContainerConnection = await ConnectToContainer(containerUrl, cancellationToken);

            return await blobContainerConnection.GetBlobsAsync(cancellationToken);
        }

        public async Task<IEnumerable<string>> FindBlobsAsync(string storageAccountUrl, string container, string sasKey, string prefix, CancellationToken cancellationToken)
        {
            if (storageAccountUrl.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(storageAccountUrl));
            if (container.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(container));
            if (sasKey.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sasKey));

            return await FindBlobsAsync(CreateBlobContainerUri(storageAccountUrl, container, sasKey), prefix, cancellationToken);
        }

        public async Task<IEnumerable<string>> FindBlobsAsync(Uri containerUrl, string prefix, CancellationToken cancellationToken)
        {
            string key = containerUrl.ToString().ToLower();

            if (!_blobContainerConnections.TryGetValue(key, out BlobContainerConnection blobContainerConnection))
                blobContainerConnection = await ConnectToContainer(containerUrl, cancellationToken);

            return await blobContainerConnection.FindBlobsAsync(prefix, cancellationToken);
        }


        [Obsolete("User DownloadAsync")]
        public async Task Download(string storageAccountUrl, string container, string name, string sasKey, CancellationToken cancellationToken) => await DownloadBlobAsync(storageAccountUrl, container, name, sasKey, cancellationToken);

        [Obsolete("User DownloadAsync")]
        public async Task Download(Uri blobUri, string name, CancellationToken cancellationToken) => await DownloadBlobAsync(blobUri, name, cancellationToken);
    }
}
