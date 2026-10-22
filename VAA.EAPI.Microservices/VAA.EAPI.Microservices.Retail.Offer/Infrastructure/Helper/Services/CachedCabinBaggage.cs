using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Services
{
    public class CachedCabinBaggage : ICachedCabinBaggage
    {
        private readonly IAzureBlobService _blobService;

        public CachedCabinBaggage(IAzureBlobService blobService, IMemoryCache cache)
        {
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
        }

        public async Task<IEnumerable<CabinBaggageEntity>> GetCachedCabinBaggageInformation(string allowance)
        {
            var blobData = await _blobService.BlobJsonAsAsync<JToken>(ConfigReader.Get(Constants.Environment.CabinBagAllowanceBlobName), CancellationToken.None);

            return blobData.Select(x => x.ToObject<CabinBaggageEntity>());
        }
    }
}
