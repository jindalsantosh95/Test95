using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Helper.Caching;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class PartnersProfilesQueryProvider : CustomerTransactionsProviderBase, IPartnersProfilesQueryProvider
    {
        private readonly ICacheService _cacheService;
        private static SemaphoreSlim semaphoreSlim = new(1, 1);

        public PartnersProfilesQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector, ICacheService cacheService) : base(tokenProvider, connector)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task<IEnumerable<PartnerProfileDetails>> GetPartnerProfileDetails()
        {
            IEnumerable<PartnerProfileDetails> partnerProfileDetails = await _cacheService.GetCacheValue<IEnumerable<PartnerProfileDetails>>(Constants.Application.ListOfPartnersCacheKey);

            if (partnerProfileDetails != null)
            {
                return partnerProfileDetails;
            }

            await semaphoreSlim.WaitAsync();

            partnerProfileDetails = await _cacheService.GetCacheValue<IEnumerable<PartnerProfileDetails>>(Constants.Application.ListOfPartnersCacheKey);

            if (partnerProfileDetails is null)
            {
                try
                {
                    string uri = ConfigReader.Get(Common.Constants.Environment.RefDataMSGenericUrl).Replace("{0}", "PartnerProfile");
                    var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.RefDataAppId));

                    Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.RefDataOcpKey) }
                            };

                    partnerProfileDetails = (await _connector.GetAsync<IEnumerable<PartnerProfileDetails>>(uri, headers, token)).Response;

                    await _cacheService.SetCacheValue(Constants.Application.ListOfPartnersCacheKey, partnerProfileDetails, true);
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }

            return partnerProfileDetails;
        }
    }
}
