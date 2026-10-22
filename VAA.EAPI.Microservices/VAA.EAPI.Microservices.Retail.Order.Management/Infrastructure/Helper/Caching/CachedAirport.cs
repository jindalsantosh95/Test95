using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.CommonRefDB;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Caching
{
    public class CachedAirport : ICachedAirport
    {
        private readonly IMemoryCache _cache;
        private readonly ICommonRefDBRepository _commonRefDBRepository;
        public CachedAirport(IMemoryCache cache, ICommonRefDBRepository commonRefDBRepository)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }
        public List<AirportDetails> GetCachedAirport()
        {
            _cache.TryGetValue(Constants.Application.AirportCacheKey, out List<AirportDetails> airportDetails);
            if (airportDetails == null)
            {
                airportDetails = _commonRefDBRepository.GetAllAirportDetails();
                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromDays(Convert.ToDouble(ConfigReader.Get(Constants.Environment.CommonCachingSlidingExpirationInDay)))).SetAbsoluteExpiration(TimeSpan.FromDays(Convert.ToDouble(ConfigReader.Get(Constants.Environment.CommonCachingAbsoluteExpirationInDay))));
                _cache.Set(Constants.Application.AirportCacheKey, airportDetails, cacheEntryOptions);
            }
            return airportDetails;
        }
    }
}
