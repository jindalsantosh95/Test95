using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Collections.Concurrent;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Utility.HttpMocker.Infrastructure.Azure
{
    public sealed class AzureBlobContainerService : IAzureBlobContainerService
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly Timer _timer;
        private ConcurrentDictionary<string, BlobDownloadResult> _items = [];
        private bool _synchronising;

        public IDictionary<string, BlobDownloadResult> Items { get => _items; }
        public DateTime LastSynced { get; private set; } = DateTime.MinValue;

        public AzureBlobContainerService() : this(ConfigReader.Get<Uri>("HttpMocker.AzureBlobContainerService.Uri"), ConfigReader.Get("HttpMocker.AzureBlobContainerService.Container"), ConfigReader.Try("HttpMocker.AzureBlobContainerService.Sas")) { }
        public AzureBlobContainerService(string connectionString, string blobContainerName)
        {
            if (connectionString.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(connectionString));
            if (blobContainerName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(blobContainerName));

            _blobContainerClient = new BlobContainerClient(connectionString, blobContainerName);

            // lets just check we can access the resource
            if (!_blobContainerClient.Exists(CancellationToken.None)) throw new DirectoryNotFoundException($"{connectionString};{nameof(blobContainerName)}={blobContainerName}");

            ConfigureInternalTimer(out _timer);
        }
        public AzureBlobContainerService(Uri resourceUri, string blobContainerName, string sass)
        {
            if (blobContainerName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(blobContainerName));

            var uri = $"{resourceUri}{blobContainerName}?{sass?.TrimStart('?') ?? string.Empty}";

            _blobContainerClient = new BlobContainerClient(new Uri(uri));

            // lets just check we can access the resource
            if (!_blobContainerClient.Exists(CancellationToken.None)) throw new DirectoryNotFoundException($"{resourceUri}{blobContainerName}");

            ConfigureInternalTimer(out _timer);
        }

        public BlobDownloadResult? TryGetBlob(string name) => _items.TryGetValue(name, out var item) ? item : default;

        public async Task<bool> SynchroniseBlob(string blobName)
        {
            if (_synchronising) return false;

            try
            {
                var blobClient = _blobContainerClient.GetBlobClient(blobName);

                if (blobClient is null) return false;

                if (!blobClient.Exists(CancellationToken.None)) return false;

                var content = await blobClient.DownloadContentAsync();

                _items.AddOrUpdate(blobName, content.Value, (k, v) => content.Value);

                return true;
            }
            catch
            {
            }

            return false;
        }

        public async Task Synchronise()
        {
            if (_synchronising) return;

            _synchronising = true;

            try
            {
                // download all files in the container, this will take a while if the
                // container contains a lot of files especially if they are large files

                ConcurrentDictionary<string, BlobDownloadResult> items = [];

                await foreach (BlobItem blobItem in _blobContainerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, null, CancellationToken.None))
                {
                    if (!blobItem.Deleted)
                    {
                        var content = await _blobContainerClient.GetBlobClient(blobItem.Name).DownloadContentAsync();

                        items.TryAdd(blobItem.Name, content.Value);
                    }
                }

                _items = items;

                LastSynced = DateTime.UtcNow;
            }
            finally
            {
                _synchronising = false;
            }
        }

        private void ConfigureInternalTimer(out Timer timer) => timer = new Timer(async (state) => await Synchronise(), null, TimeSpan.FromSeconds(3), TimeSpan.FromMinutes(20));
    }
}
