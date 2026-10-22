using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Reference.Application.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using Environment = VAA.EAPI.Microservices.Retail.Reference.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public sealed class MobileFeaturesQueryHandler : IMobileFeaturesQueryHandler
    {
        private readonly IAzureBlobService _azureBlobService;

        public MobileFeaturesQueryHandler(IAzureBlobService azureBlobService)
        {
            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        }

        public async Task<MobileFeatures> HandleAsync(CancellationToken cancellationToken)
        {
            var blobName = ConfigReader.Get(Environment.MobileFeaturesBlob);
            string json;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _azureBlobService.DownloadBlobAsync(ConfigReader.Get(Environment.BlobStorageUri),
                    ConfigReader.Get(Environment.ContainerName), blobName, ConfigReader.Get(Environment.SasKey), cancellationToken);
                json = await _azureBlobService.BlobStringAsync(blobName, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }
            catch (Exception)
            {
                // Blob exceptions can contain SAS-bearing URIs; do not retain the message or inner exception.
                throw new InvalidOperationException("Mobile feature configuration is unavailable.");
            }
            var document = JsonConvert.DeserializeObject<JToken>(json, new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None
            });
            if (document is not JObject root || root["features"] is not JObject features ||
                features.Properties().Any(feature => feature.Name.IsNullOrWhiteSpace() || feature.Value.Type != JTokenType.String))
            {
                throw new JsonSerializationException("Invalid mobile feature configuration.");
            }

            return new MobileFeatures
            {
                Features = features.Properties().ToDictionary(feature => feature.Name, feature => (string)feature.Value)
            };
        }
    }
}
