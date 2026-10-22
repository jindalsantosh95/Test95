using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class CityCodeQueryHandler : ICityCodeQuery
    {
        private readonly ICityCodeDetails _cityCodeDetails;

        public CityCodeQueryHandler(ICityCodeDetails cityCodeDetails)
        {
            _cityCodeDetails = cityCodeDetails ?? throw new ArgumentNullException(nameof(cityCodeDetails));
        }

        public async Task<CityCodeData> Handle(string countryCode, CancellationToken cancellationToken)
        {
            var cityCodes = await _cityCodeDetails.GetCityCodeList();
            var cityCode = cityCodes.FirstOrDefault(x => x.CountryCode == countryCode);

            if (cityCode == null)
            {
                return null;
            }

            var result = (CityCodeData)cityCode;

            return result;
        }
    }
}