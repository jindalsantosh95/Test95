using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using Environment = VAA.EAPI.Microservices.Retail.Reference.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Reference.Application
{
    public class ReferenceDataCommandHandler : IRequestHandler<ReferenceDataCommand, string>
    {
        private readonly IAzureBlobService _azureBlobService;

        public ReferenceDataCommandHandler(IAzureBlobService azureBlobService)
        {
            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        }

        public async Task<string> Handle(ReferenceDataCommand request, CancellationToken cancellationToken)
        {
            var blobName = GetBlobName(request.ReferenceDataType);

            await _azureBlobService.Download(ConfigReader.Get(Environment.BlobStorageUri), ConfigReader.Get(Environment.ContainerName), blobName, ConfigReader.Get(Environment.SasKey), cancellationToken);

            return await _azureBlobService.BlobStringAsync(blobName, cancellationToken);
        }

        private string GetBlobName(ReferenceDataTypeEnum dataType)
        {
            return dataType switch
            {
                ReferenceDataTypeEnum.TAXCODE => ConfigReader.Get(Environment.TaxCodeBlob),
                ReferenceDataTypeEnum.VSECON => ConfigReader.Get(Environment.VsEconBlob),
                ReferenceDataTypeEnum.PRODUCTIDENTIFIRES => ConfigReader.Get(Environment.ProductIdentifierBlob),
                ReferenceDataTypeEnum.AIRPORT => ConfigReader.Get(Environment.AirportBlob),
                ReferenceDataTypeEnum.AIRLINE => ConfigReader.Get(Environment.AirlineBlob),
                ReferenceDataTypeEnum.CABIN => ConfigReader.Get(Environment.CabinBlob),
                ReferenceDataTypeEnum.AIRCRAFT => ConfigReader.Get(Environment.AircraftBlob),
                ReferenceDataTypeEnum.BAGGAGE => ConfigReader.Get(Environment.BaggageBlob),
                ReferenceDataTypeEnum.COUNTRY => ConfigReader.Get(Environment.CountryBlob),
                ReferenceDataTypeEnum.Offers => ConfigReader.Get(Environment.UpgradeOffersDataBlob),
                ReferenceDataTypeEnum.SSRDETAIL => ConfigReader.Get(Environment.SSRDetailsDataBlob),
                ReferenceDataTypeEnum.AIR4CURRENCYNUMBER => ConfigReader.Get(Environment.Air4CurrencyNumberBlob),
                ReferenceDataTypeEnum.FQTPROGS => ConfigReader.Get(Environment.FQTProgramsBlob),
                ReferenceDataTypeEnum.CABINCLASS => ConfigReader.Get(Environment.CabinClassBlob),
                ReferenceDataTypeEnum.SalesBanner_DestinationURLs => ConfigReader.Get(Environment.DestinationURL),
                ReferenceDataTypeEnum.GENDERS => ConfigReader.Get(Environment.GendersBlob),
                ReferenceDataTypeEnum.STATESOFUSA => ConfigReader.Get(Environment.StatesOfUSABlob),
                ReferenceDataTypeEnum.PASSPORTCOUNTRIES => ConfigReader.Get(Environment.PassportCountriesBlob),
                ReferenceDataTypeEnum.NATIONALITIES => ConfigReader.Get(Environment.NationalitiesBlob),
                ReferenceDataTypeEnum.SSRMEALDETAILS => ConfigReader.Get(Environment.SSRMealBlob),
                ReferenceDataTypeEnum.PAYMENTMETHODDETAILS => ConfigReader.Get(Environment.PaymentMethodDetailsBlob),
                _ => throw new IndexOutOfRangeException()
            };
        }
    }
}
