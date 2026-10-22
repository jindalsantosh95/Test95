using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Constants;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Interface;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Models;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Repositories.InflightOrders.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Helper.Caching
{
    public class CachedCarrier : ICachedCarrier
    {
        private readonly IMemoryCache _cache;
        private readonly ICommonRefDBRepository _commonRefDBRepository;
        public CachedCarrier(IMemoryCache cache, ICommonRefDBRepository commonRefDBRepository)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }
        public List<CarrierDetails> GetCachedCarrier()
        {
            _cache.TryGetValue(Constants.Application.CarrierCacheKey, out List<CarrierDetails> carrierDetails);
            if (carrierDetails == null)
            {
                carrierDetails = _commonRefDBRepository.GetAllCarrierDetails();
                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetSlidingExpiration(TimeSpan.FromDays(Convert.ToDouble(ConfigReader.Get(Constants.Environment.CommonCachingSlidingExpirationInDay)))).SetAbsoluteExpiration(TimeSpan.FromDays(Convert.ToDouble(ConfigReader.Get(Constants.Environment.CommonCachingAbsoluteExpirationInDay))));
                _cache.Set(Constants.Application.CarrierCacheKey, carrierDetails, cacheEntryOptions);
            }
            return carrierDetails;
        }
    }
}
