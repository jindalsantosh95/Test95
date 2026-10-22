using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Services
{
    public class CachedFlightProduct : ICachedFlightProduct
    {
        private readonly IMemoryCache _cache;
        private readonly ICommonRefDBServices _commonRefDbServices;
        private static readonly SemaphoreSlim Semaphore = new(1, 1);

        public CachedFlightProduct(IMemoryCache cache, ICommonRefDBServices commonRefDbServices)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _commonRefDbServices = commonRefDbServices ?? throw new ArgumentNullException(nameof(commonRefDbServices));
        }

        public async Task<Product> GetFlightProductAsync()
        {
            if (_cache.TryGetValue(Constants.Application.FlightProductCacheKey, out Product product))
                return product;

            await Semaphore.WaitAsync();

            try
            {
                if (_cache.TryGetValue(Constants.Application.FlightProductCacheKey, out product))
                    return product;

                product = await _commonRefDbServices.GetProductReferenceDataByName(Constants.Application.FlightProductName);

                if (product is not null)
                {
                    var slidingExpiration = TimeSpan.Parse(ConfigReader.Get(Constants.Environment.CommonCachingSlidingExpiration));
                    var absoluteExpiration = TimeSpan.Parse(ConfigReader.Get(Constants.Environment.CommonCachingAbsoluteExpiration));
                    _cache.Set(Constants.Application.FlightProductCacheKey, product, new MemoryCacheEntryOptions().SetSlidingExpiration(slidingExpiration).SetAbsoluteExpiration(absoluteExpiration));
                }

                return product;
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}
