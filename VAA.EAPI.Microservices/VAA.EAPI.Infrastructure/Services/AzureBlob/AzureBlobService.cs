using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Infrastructure.Services.AzureBlob
{
    public sealed class AzureBlobService : IAzureBlobService
    {
        internal sealed class BlobConnection
        {
            public static explicit operator BlobConnection(BlobClient blobClient) => new(blobClient) { };

            private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
            private readonly BlobClient _blobClient;
            private readonly ConcurrentDictionary<string, object> _typeCache = [];

            public string Key => _blobClient?.Uri.ToString().ToLower();
            public string Name => _blobClient?.Name;
            public TimeSpan Ttl { get; private set; }
            public BinaryData Content { get; private set; }
            public DateTime LastDownloaded { get; private set; }
            public bool HasExpired => DateTime.UtcNow >= LastDownloaded.Add(Ttl);

            private BlobConnection(BlobClient blobClient)
            {
                _blobClient = blobClient;

                LastDownloaded = DateTime.MinValue;
                Ttl = ConfigReader.Try(Constants.Environment.AzureBlobServiceBlobExpiration, TimeSpan.FromDays(1));
            }

            public async Task<bool> ExistsAsync(CancellationToken cancellationToken)
            {
                try
                {
                    var response = await _blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);

                    return response.Value;
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    return false;
                }
            }

            public async Task<BinaryData> DownloadContentAsync(CancellationToken cancellationToken)
            {
                if (!HasExpired) return Content;

                bool locked = false;

                try
                {
                    await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

                    locked = true;

                    if (!HasExpired) return Content;
                    if (!await ExistsAsync(cancellationToken).ConfigureAwait(false)) throw new InvalidOperationException($"Resource, {Key}, cannot be found in blob storage, please check resource exists");

                    Content = (await _blobClient.DownloadContentAsync(cancellationToken).ConfigureAwait(false)).Value.Content;
                    LastDownloaded = DateTime.UtcNow;

                    _typeCache.Clear();
                }
                finally
                {
                    if (locked)
                        _semaphoreSlim.Release();
                }

                return Content;
            }

            public async Task<T> ContentAsAsync<T>(BlobType type, CancellationToken cancellationToken)
            {
                if (type is BlobType.Binary or BlobType.Text) throw new ArgumentException("Json & Xml blob types are only supported", nameof(type));

                var key = $"{typeof(T).FullName}:{type}";
                var binaryData = await DownloadContentAsync(cancellationToken);

                if (_typeCache.TryGetValue(key, out object value)) return (T)value;

                bool locked = false;

                try
                {
                    await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

                    locked = true;

                    if (_typeCache.TryGetValue(key, out value)) return (T)value;

                    switch (type)
                    {
                        case BlobType.Json:

                            var json = binaryData.ToString();

                            value = Json.Deserialize<T>(json);

                            break;
                        case BlobType.Xml:

                            var xml = binaryData.ToString();

                            value = Xml.Deserialize<T>(xml);

                            break;
                    }

                    if (value is not null)
                        _typeCache.TryAdd(key, value);
                }
                finally
                {
                    if (locked)
                        _semaphoreSlim.Release();
                }

                return value is null ? default : (T)value;
            }
            public async Task<string> ContentAsStringAsync(CancellationToken cancellationToken) => (await DownloadContentAsync(cancellationToken).ConfigureAwait(false)).ToString();
            public async Task<byte[]> ContentAsBytesAsync(CancellationToken cancellationToken) => (await DownloadContentAsync(cancellationToken).ConfigureAwait(false)).ToArray();

            public async Task UploadAsync(string content, CancellationToken cancellationToken)
            {
                if (content.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(content));

                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

                await _blobClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken).ConfigureAwait(false);

                Content = (await _blobClient.DownloadContentAsync(cancellationToken).ConfigureAwait(false)).Value.Content;
                LastDownloaded = DateTime.UtcNow;

                _typeCache.Clear();
            }
        }
        internal sealed class BlobContainerConnection
        {
            public static explicit operator BlobContainerConnection(BlobContainerClient blobContainerClient) => new(blobContainerClient) { };

            private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
            private readonly BlobContainerClient _blobContainerClient;

            public string Key => _blobContainerClient?.Uri.ToString().ToLower();
            public string Name => _blobContainerClient?.Name;

            private BlobContainerConnection(BlobContainerClient blobContainerClient)
            {
                _blobContainerClient = blobContainerClient;
            }

            public async Task<bool> ExistsAsync(CancellationToken cancellationToken)
            {
                try
                {
                    var response = await _blobContainerClient.ExistsAsync(cancellationToken).ConfigureAwait(false);

                    return response.Value;
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    return false;
                }
            }

            public async Task UploadAsync(string name, string content, BlobType type, bool overwrite, CancellationToken cancellationToken)
            {
                if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));
                if (content.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(content));

                bool locked = false;

                try
                {
                    await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

                    locked = true;

                    var mediaType = type switch
                    {
                        BlobType.Json => MediaTypeNames.Application.Json,
                        BlobType.Xml => MediaTypeNames.Application.Xml,
                        BlobType.Text => MediaTypeNames.Text.Plain,
                        BlobType.Binary => MediaTypeNames.Application.Octet,
                        _ => throw new ArgumentException("Json, Xml, Text & Binary blob types are only supported", nameof(type))
                    };

                    var bytes = Encoding.UTF8.GetBytes(content);

                    if (overwrite)
                        await _blobContainerClient.GetBlobClient(name)
                                                  .UploadAsync(new BinaryData(bytes, mediaType), true, cancellationToken)
                                                  .ConfigureAwait(false);
                    else
                        await _blobContainerClient.UploadBlobAsync(name, new BinaryData(bytes, mediaType), cancellationToken)
                                                  .ConfigureAwait(false);
                }
                finally
                {
                    if (locked)
                        _semaphoreSlim.Release();
                }
            }

            public async Task<IEnumerable<string>> GetBlobsAsync(CancellationToken cancellationToken) => await FindBlobsAsync(string.Empty, cancellationToken).ConfigureAwait(false);

            public async Task<IEnumerable<string>> FindBlobsAsync(string prefix, CancellationToken cancellationToken)
            {
                try
                {
                    List<BlobItem> blobs = [];

                    await foreach (BlobItem blobItem in _blobContainerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken).ConfigureAwait(false))
                        blobs.Add(blobItem);

                    return blobs.OrderByDescending(b => b.Properties.LastModified)
                                .Select(x => x.Name);
                }
                catch
                {
                    return [];
                }
            }

            public async Task DeleteBlobAsync(string name, CancellationToken cancellationToken)
            {
                if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));

                bool locked = false;

                try
                {
                    await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

                    locked = true;

                    await _blobContainerClient.DeleteBlobAsync(name, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    if (locked)
                        _semaphoreSlim.Release();
                }
            }
        }

        private static readonly ConcurrentDictionary<string, BlobConnection> _blobConnections = new();
        private static readonly ConcurrentDictionary<string, BlobContainerConnection> _blobContainerConnections = new();

        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        private static async Task<BlobConnection> ConnectToBlob(Uri uri, CancellationToken cancellationToken)
        {
            var key = uri.ToString().ToLower();

            if (_blobConnections.TryGetValue(key, out BlobConnection value))
            {
                try
                {
                    var exists = await value.ExistsAsync(cancellationToken).ConfigureAwait(false);

                    if (exists) return value;

                    _blobConnections.TryRemove(key, out _);
                }
                catch
                {
                    _blobConnections.TryRemove(key, out _);
                }
            }

            bool locked = false;

            try
            {
                await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

                locked = true;

                if (_blobConnections.TryGetValue(key, out value)) return value;

                var blobConnection = (BlobConnection)new BlobClient(uri);

                _blobConnections.AddOrUpdate(key, blobConnection, (k, v) => blobConnection);
            }
            finally
            {
                if (locked)
                    _semaphoreSlim.Release();
            }

            return _blobConnections[key];
        }
        private static async Task<BlobContainerConnection> ConnectToContainer(Uri uri, CancellationToken cancellationToken)
        {
            var key = uri.ToString().ToLower();

            if (_blobContainerConnections.TryGetValue(key, out BlobContainerConnection value))
            {
                try
                {
                    var exists = await value.ExistsAsync(cancellationToken).ConfigureAwait(false);

                    if (exists) return value;

                    _blobContainerConnections.TryRemove(key, out _);
                }
                catch
                {
                    _blobContainerConnections.TryRemove(key, out _);
                }
            }

            bool locked = false;

            try
            {
                await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

                locked = true;

                if (_blobContainerConnections.TryGetValue(key, out value)) return value;

                var blobContainerConnection = (BlobContainerConnection)new BlobContainerClient(uri);

                _blobContainerConnections.AddOrUpdate(key, blobContainerConnection, (k, v) => blobContainerConnection);
            }
            finally
            {
                if (locked)
                    _semaphoreSlim.Release();
            }

            return _blobContainerConnections[key];
        }

        public static Uri CreateBlobUri(string resourceUrl, string containerName, string name, string sasKey)
        {
            if (resourceUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(resourceUrl));
            if (containerName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(containerName));
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));

            sasKey ??= string.Empty;

            return new Uri($"{resourceUrl}/{containerName}/{name}?{sasKey.TrimStart('?')}");
        }

        public static Uri CreateBlobContainerUri(string resourceUrl, string containerName, string sasKey)
        {
            if (resourceUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(resourceUrl));
            if (containerName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(containerName));

            sasKey ??= string.Empty;

            return new Uri($"{resourceUrl}/{containerName}?{sasKey.TrimStart('?')}");
        }

        public async Task<byte[]> BlobBytesAsync(string name, CancellationToken cancellationToken)
        {
            var blobConnection = _blobConnections.Values.FirstOrDefault(x => x.Name.Is(name)) ?? throw new InvalidOperationException($"No blob connection exists for blob '{name}'.");

            return await blobConnection.ContentAsBytesAsync(cancellationToken) ?? default;
        }

        public async Task<string> BlobStringAsync(string name, CancellationToken cancellationToken)
        {
            var blobConnection = _blobConnections.Values.FirstOrDefault(x => x.Name.Is(name)) ?? throw new InvalidOperationException($"No blob connection exists for blob '{name}'.");

            return await blobConnection.ContentAsStringAsync(cancellationToken) ?? default;
        }

        public async Task<T> BlobJsonAsAsync<T>(string name, CancellationToken cancellationToken)
        {
            var blobConnection = _blobConnections.Values.FirstOrDefault(x => x.Name.Is(name)) ?? throw new InvalidOperationException($"No blob connection exists for blob '{name}'.");

            return await blobConnection.ContentAsAsync<T>(BlobType.Json, cancellationToken) ?? default;
        }

        public async Task<T> BlobXmlAsAsync<T>(string name, CancellationToken cancellationToken)
        {
            var blobConnection = _blobConnections.Values.FirstOrDefault(x => x.Name.Is(name)) ?? throw new InvalidOperationException($"No blob connection exists for blob '{name}'.");

            return await blobConnection.ContentAsAsync<T>(BlobType.Xml, cancellationToken) ?? default;
        }

        public async Task WriteToBlobAsync(string name, string content, CancellationToken cancellationToken)
        {
            var blobConnection = _blobConnections.Values.FirstOrDefault(x => x.Name.Is(name)) ?? throw new InvalidOperationException($"No blob connection exists for blob '{name}'.");

            await blobConnection.UploadAsync(content, cancellationToken);
        }

        public async Task DownloadBlobAsync(string storageAccountUrl, string container, string name, string sasKey, CancellationToken cancellationToken)
        {
            if (storageAccountUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(storageAccountUrl));
            if (container.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(container));
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));
            if (sasKey.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(sasKey));

            await DownloadBlobAsync(CreateBlobUri(storageAccountUrl, container, name, sasKey), name, cancellationToken).ConfigureAwait(false);
        }

        public async Task DownloadBlobAsync(Uri blobUri, string name, CancellationToken cancellationToken)
        {
            var key = blobUri.ToString().ToLower();

            if (!_blobConnections.TryGetValue(key, out BlobConnection blobConnection))
                blobConnection = await ConnectToBlob(blobUri, cancellationToken).ConfigureAwait(false);

            _ = await blobConnection.DownloadContentAsync(cancellationToken).ConfigureAwait(false);
        }


        public async Task UploadBlobAsync(string storageAccountUrl, string container, string sasKey, string name, string content, BlobType type, bool overwrite, CancellationToken cancellationToken)
        {
            if (storageAccountUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(storageAccountUrl));
            if (container.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(container));
            if (sasKey.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(sasKey));
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));
            if (content.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(content));

            await UploadBlobAsync(CreateBlobContainerUri(storageAccountUrl, container, sasKey), name, content, type, overwrite, cancellationToken);
        }

        public async Task UploadBlobAsync(Uri containerUrl, string name, string content, BlobType type, bool overwrite, CancellationToken cancellationToken)
        {
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));
            if (content.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(content));

            var key = containerUrl.ToString().ToLower();

            if (!_blobContainerConnections.TryGetValue(key, out BlobContainerConnection blobContainerConnection))
                blobContainerConnection = await ConnectToContainer(containerUrl, cancellationToken).ConfigureAwait(false);

            await blobContainerConnection.UploadAsync(name, content, type, overwrite, cancellationToken).ConfigureAwait(false);

            var resource = key.Split('?');
            var resourceParts = resource[0].Split('/', StringSplitOptions.RemoveEmptyEntries);
            var storage = string.Join('/', resourceParts.Take(resourceParts.Length - 1)).Replace(":/", "://");
            var container = resourceParts.Last();
            var sasKey = resource.Length == 2 ? resource[1] : string.Empty;

            //await DownloadBlobAsync(storage, container, name, sasKey, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> GetBlobsAsync(string storageAccountUrl, string container, string sasKey, CancellationToken cancellationToken)
        {
            if (storageAccountUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(storageAccountUrl));
            if (container.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(container));
            if (sasKey.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(sasKey));

            return await GetBlobsAsync(CreateBlobContainerUri(storageAccountUrl, container, sasKey), cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> GetBlobsAsync(Uri containerUrl, CancellationToken cancellationToken)
        {
            var key = containerUrl.ToString().ToLower();

            if (!_blobContainerConnections.TryGetValue(key, out BlobContainerConnection blobContainerConnection))
                blobContainerConnection = await ConnectToContainer(containerUrl, cancellationToken).ConfigureAwait(false);

            return await blobContainerConnection.GetBlobsAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> FindBlobsAsync(string storageAccountUrl, string container, string sasKey, string prefix, CancellationToken cancellationToken)
        {
            if (storageAccountUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(storageAccountUrl));
            if (container.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(container));
            if (sasKey.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(sasKey));

            return await FindBlobsAsync(CreateBlobContainerUri(storageAccountUrl, container, sasKey), prefix, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> FindBlobsAsync(Uri containerUrl, string prefix, CancellationToken cancellationToken)
        {
            var key = containerUrl.ToString().ToLower();

            if (!_blobContainerConnections.TryGetValue(key, out BlobContainerConnection blobContainerConnection))
                blobContainerConnection = await ConnectToContainer(containerUrl, cancellationToken).ConfigureAwait(false);

            return await blobContainerConnection.FindBlobsAsync(prefix, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteBlobAsync(string storageAccountUrl, string container, string sasKey, string name, CancellationToken cancellationToken)
        {
            if (storageAccountUrl.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(storageAccountUrl));
            if (container.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(container));
            if (sasKey.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(sasKey));
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));

            await DeleteBlobAsync(CreateBlobContainerUri(storageAccountUrl, container, sasKey), name, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteBlobAsync(Uri containerUrl, string name, CancellationToken cancellationToken)
        {
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));

            var key = containerUrl.ToString().ToLower();

            if (!_blobContainerConnections.TryGetValue(key, out BlobContainerConnection blobContainerConnection))
                blobContainerConnection = await ConnectToContainer(containerUrl, cancellationToken).ConfigureAwait(false);

            await blobContainerConnection.DeleteBlobAsync(name, cancellationToken).ConfigureAwait(false);
        }

        [Obsolete("Use DownloadBlobAsync")]
        public async Task Download(string storageAccountUrl, string container, string name, string sasKey, CancellationToken cancellationToken) => await DownloadBlobAsync(storageAccountUrl, container, name, sasKey, cancellationToken).ConfigureAwait(false);

        [Obsolete("Use DownloadBlobAsync")]
        public async Task Download(Uri blobUri, string name, CancellationToken cancellationToken) => await DownloadBlobAsync(blobUri, name, cancellationToken).ConfigureAwait(false);
    }
}
