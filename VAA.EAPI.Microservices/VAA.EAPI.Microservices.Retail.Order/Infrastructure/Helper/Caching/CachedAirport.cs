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
    public class CachedAirport : ICachedAirport
    {
        private readonly IMemoryCache _cache;
        private readonly ICommonRefDBRepository _commonRefDbRepository;

        public CachedAirport(IMemoryCache cache, ICommonRefDBRepository commonRefDbRepository)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _commonRefDbRepository = commonRefDbRepository ?? throw new ArgumentNullException(nameof(commonRefDbRepository));
        }

        public List<AirportDetails> GetCachedAirport()
        {
            _cache.TryGetValue(Constants.Application.AirportCacheKey, out List<AirportDetails> airportDetails);

            if (airportDetails == null)
            {
                airportDetails = _commonRefDbRepository.GetAllAirportDetails();
                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(Convert.ToDouble(ConfigReader.Get(Constants.Environment.CommonCachingSlidingExpirationInDay)))).SetAbsoluteExpiration(TimeSpan.FromDays(Convert.ToDouble(ConfigReader.Get(Constants.Environment.CommonCachingAbsoluteExpirationInDay))));
                _cache.Set(Constants.Application.AirportCacheKey, airportDetails, cacheEntryOptions);
            }

            return airportDetails;
        }
    }
}
