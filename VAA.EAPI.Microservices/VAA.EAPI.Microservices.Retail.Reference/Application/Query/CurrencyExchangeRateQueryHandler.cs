using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Reference.Application.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

using Environment = VAA.EAPI.Microservices.Retail.Reference.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class CurrencyExchangeRateQueryHandler : ICurrencyExchangeRateQuery
    {
        private readonly IAzureBlobService _azureBlobService;

        public CurrencyExchangeRateQueryHandler(IAzureBlobService azureBlobService)
        {
            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        }

        public async Task<ExchangeRateResponse> Handle()
        {
            var blobName = Constants.Application.CurrencyConversionBlobName;

            await _azureBlobService.Download(ConfigReader.Get(Environment.BlobStorageUri), ConfigReader.Get(Environment.ContainerName), blobName, ConfigReader.Get(Environment.SasKey), CancellationToken.None);

            var blobResponse = await _azureBlobService.BlobStringAsync(blobName, CancellationToken.None);

            return Json.Deserialize<ExchangeRateResponse>(blobResponse);
        }
    }
}
