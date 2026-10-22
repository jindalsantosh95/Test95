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
    public class ProductsQueryProvider : CustomerTransactionsProviderBase, IProductsQueryProvider
    {
        private readonly ICacheService _cacheService;
        private static SemaphoreSlim semaphoreSlim = new(1, 1);


        public ProductsQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector, ICacheService cacheService) : base(tokenProvider, connector)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task<IEnumerable<ProductDetails>> GetProductDetails()
        {
            IEnumerable<ProductDetails> productDetails = await _cacheService.GetCacheValue<IEnumerable<ProductDetails>>(Constants.Application.ListOfProductsCacheKey);

            if (productDetails != null)
            {
                return productDetails;
            }

            await semaphoreSlim.WaitAsync();

            productDetails = await _cacheService.GetCacheValue<IEnumerable<ProductDetails>>(Constants.Application.ListOfProductsCacheKey);

            if (productDetails is null)
            {

                try
                {
                    string uri = ConfigReader.Get(Common.Constants.Environment.RefDataMSGenericUrl).Replace("{0}", "Products"); ;
                    var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.RefDataAppId));

                    Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.RefDataOcpKey) }
                            };

                    productDetails = (await _connector.GetAsync<IEnumerable<ProductDetails>>(uri, headers, token)).Response;

                    await _cacheService.SetCacheValue(Constants.Application.ListOfProductsCacheKey, productDetails, true);
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }

            return productDetails;
        }
    }
}
