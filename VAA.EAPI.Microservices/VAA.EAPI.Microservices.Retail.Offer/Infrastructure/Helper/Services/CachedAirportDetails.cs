using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Services
{
    public class CachedAirportDetails : ICachedAirportDetails
    {
        private readonly IAzureBlobService _blobService;

        public CachedAirportDetails(IAzureBlobService blobService)
        {
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
        }

        public async Task<AirportDetailsTableEntity> GetCachedAirportDetails(string rowKey)
        {
            var blobData = await _blobService.BlobJsonAsAsync<JToken>(ConfigReader.Get(Constants.Environment.AirportLocationBlobName), CancellationToken.None);

            return blobData.SelectToken($"$.[?(@.RowKey== '{rowKey}')]")?.ToObject<AirportDetailsTableEntity>();
        }
    }
}
