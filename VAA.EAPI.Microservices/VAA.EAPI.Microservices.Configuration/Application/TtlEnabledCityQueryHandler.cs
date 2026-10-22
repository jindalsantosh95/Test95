
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class TtlEnabledCityQueryHandler : ITtlEnabledCity
    {
        private readonly ITtlEnabledCityData _ttlEnabledCityData;

        public TtlEnabledCityQueryHandler(ITtlEnabledCityData ttlEnabledCityData)
        {
            _ttlEnabledCityData = ttlEnabledCityData ?? throw new ArgumentNullException(nameof(ttlEnabledCityData));
        }

        public async Task<IEnumerable<TtlEnabledCityData>> Handle(string airportCode, CancellationToken cancellationToken)
        {
            var ttlCityDetails = await _ttlEnabledCityData.GetTtlEnabledCityData();

            var result = ttlCityDetails.Where(x => x.AirportCode == airportCode).Select(x => new TtlEnabledCityData
            {
                RegionName = x.RegionName,
                CountryCode = x.CountryCode,
                AirportCode = x.AirportCode,
                LocalCurrencyCode = x.LocalCurrencyCode,
                CityCode = x.CityCode
            });

            return result;
        }
    }
}
