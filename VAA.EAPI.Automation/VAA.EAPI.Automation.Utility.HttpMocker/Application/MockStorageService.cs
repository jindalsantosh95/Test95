using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Http.Mocking;
using VAA.EAPI.Automation.Utility.HttpMocker.Infrastructure.Azure;

namespace VAA.EAPI.Automation.Utility.HttpMocker.Application
{
    public sealed class MockStorageService : IMockStorageService
    {
        public enum SourceType
        {
            LocalStorage = 0,
            AzureStorage
        }

        private readonly string _localPath;
        private readonly IAzureBlobContainerService? _azureBlobService;
        private readonly ILogger<MockStorageService> _logger;

        private HttpMock[] _items = [];

        public SourceType Source { get; }

        public MockStorageService(string localPath, ILogger<MockStorageService> logger)
        {
            Source = SourceType.LocalStorage;

            if (localPath.IsNullOrWhiteSpace())
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(localPath));

            _localPath = localPath;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (!Directory.Exists(localPath))
                return;

            SynchroniseMocks();
        }
        public MockStorageService(IAzureBlobContainerService azureBlobService, ILogger<MockStorageService> logger)
        {
            Source = SourceType.AzureStorage;

            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SynchroniseMocks();
        }

        public async Task<HttpMock?> FindMockAsync(FunctionContext context)
        {
            var requestUriRaw = HttpUtility.UrlDecode(context.BindingContext.BindingData.TryGetValue("requestUri", out var requestUriBinding) ? requestUriBinding!.ToString()! : string.Empty);
            var httpRequestData = (await context.GetHttpRequestDataAsync().ConfigureAwait(false))!;
            using var httpRequestMessage = httpRequestData.ToHttpRequestMessage(requestUriRaw);

            return _items.FindMock(httpRequestMessage, _logger);
        }

        private HttpMock[] GetItems() => _items;

        public Task<bool> SynchroniseMock(string name)
        {
            return Source switch
            {
                SourceType.LocalStorage => Task.FromResult(true),
                SourceType.AzureStorage => _azureBlobService!.SynchroniseBlob(name),
                _ => throw new NotImplementedException(),
            };
        }

        public Task Synchronise()
        {
            SynchroniseMocks();

            return Task.CompletedTask;
        }

        private void SynchroniseMocks()
        {
            switch (Source)
            {
                case SourceType.AzureStorage:

                    _azureBlobService!.Synchronise().GetAwaiter().GetResult();

                    _items = [.. _azureBlobService!.Items.Select(x =>
                                                   {
                                                       try
                                                       {
                                                           return (HttpMock)x.Value;
                                                       }
                                                       catch (Exception ex)
                                                       {
                                                           _logger.LogError(ex, "Failed to load mock file {MockFile}. Skipping.", x.Key);
                                                       }

                                                       return default(HttpMock);
                                                   })
                                                   .Where(m => m is not null)
                                                   .Select(m => m!)];

                    break;
                case SourceType.LocalStorage:

                    _items = [.. Directory.GetFiles(_localPath, "*.mock", SearchOption.TopDirectoryOnly)
                                          .Select(path =>
                                          {
                                              try
                                              {
                                                  return (HttpMock)new FileInfo(path);
                                              }
                                              catch (Exception ex)
                                              {
                                                  _logger.LogError(ex, "Failed to load mock file {MockFile}. Skipping.", path);
                                                  return null;
                                              }
                                          })
                                          .Where(m => m is not null)
                                          .Select(m => m!)];
                    break;
            }
        }
    }
}
