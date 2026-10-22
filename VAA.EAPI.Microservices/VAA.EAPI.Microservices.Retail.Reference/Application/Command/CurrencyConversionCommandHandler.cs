using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Reference.Application.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Reference.Mapper;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Command
{
    public class CurrencyConversionCommandHandler : ICurrencyConversionCommand
    {

        private readonly ICurrencyConversionService _currencyConversionService;
        private readonly IAzureBlobService _azureBlobService;


        public CurrencyConversionCommandHandler(ICurrencyConversionService currencyConversionService, IAzureBlobService azureBlobService)
        {
            _currencyConversionService = currencyConversionService ?? throw new ArgumentNullException(nameof(currencyConversionService));
            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        }

        public async Task Handle()
        {
            var request = CurrencyConversionRequestMapper.MapCurrencyConversionRequestToXml();

            var response = await _currencyConversionService.Handle(request);

            var exchangeRate = XDocument.Parse(response)
                                        .Descendants(XName.Get("Response", "http://schemas.virgin-atlantic.com/FinancialServices/ExchangeRateRequest/2007/12"))
                                        .FirstOrDefault()?
                                        .Attribute("convertedValue")?
                                        .Value;

            var blobResponse = new ExchangeRateResponse()
            {
                UsdToGbpExchangeRate = RoundOffHelper.RoundTo2(Decimal.Parse(exchangeRate)),
                UpdateTimestamp = DateTime.UtcNow.ToString(Constants.Application.DataTimeFormat)
            };

            await _azureBlobService.UploadBlobAsync(ConfigReader.Get(Constants.Environment.BlobStorageUri), ConfigReader.Get(Constants.Environment.ContainerName), ConfigReader.Get(Constants.Environment.SasKey), Constants.Application.CurrencyConversionBlobName, Json.Serialize(blobResponse), BlobType.Json, true, CancellationToken.None);
        }
    }
}
