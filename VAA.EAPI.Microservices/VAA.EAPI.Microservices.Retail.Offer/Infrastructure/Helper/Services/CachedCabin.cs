using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Services
{
    public class CachedCabin : ICachedCabin
    {
        private readonly IAzureBlobService _blobService;

        public CachedCabin(IAzureBlobService blobService)
        {
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
        }

        public async Task<CabinTableEntity> GetCachedCabinData(string rowKey)
        {
            var blobData = await _blobService.BlobJsonAsAsync<JToken>(ConfigReader.Get(Constants.Environment.CabinDataBlobName), CancellationToken.None);

            return blobData.SelectToken($"$.[?(@.RowKey== '{rowKey}')]")?.ToObject<CabinTableEntity>();
        }
    }
}