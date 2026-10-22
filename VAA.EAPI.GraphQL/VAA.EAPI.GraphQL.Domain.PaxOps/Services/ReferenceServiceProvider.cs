using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.ReferenceData;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Services
{
    public class ReferenceServiceProvider : IReferenceServiceProvider
    {
        protected readonly IHttpConnector _connector;

        public ReferenceServiceProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public Task<IEnumerable<Models.ReferenceData.Gender>> GetGenderDetails()
         => GetReferenceDataAsync<Gender>(Common.Constants.Application.GenderDetails);

        public Task<IEnumerable<Models.ReferenceData.Nationality>> GetNationalityDetails()
        => GetReferenceDataAsync<Nationality>(Common.Constants.Application.NationalityDetails);

        public Task<IEnumerable<Models.ReferenceData.PassportCountry>> GetPassportCountries()
            => GetReferenceDataAsync<PassportCountry>(Common.Constants.Application.PassportCountryDetails);

        public Task<IEnumerable<Models.ReferenceData.StatesOfUSA>> GetStatesOfUSA()
            => GetReferenceDataAsync<StatesOfUSA>(Common.Constants.Application.StatesOfUSADetails);

        public Task<IEnumerable<CountryProfile>> GetCountryProfile()
            => GetReferenceDataAsync<CountryProfile>(Common.Constants.Application.CountryProfile);

        public Task<IEnumerable<CurrencyNumberReferenceData>> GetAir4CurrencyNumberAsync()
            => GetReferenceDataAsync<CurrencyNumberReferenceData>(Common.Constants.Application.CurrencyNumber);

        // Generic method to avoid code duplication
        private async Task<IEnumerable<T>> GetReferenceDataAsync<T>(string endpoint)
        {
            var result = await _connector.GetAsync<IEnumerable<T>>($"{ConfigReader.Get(Common.Constants.Environment.ReferenceMsUrl)}{endpoint}", null, string.Empty);
            return result.Response;
        }

        public Task<IEnumerable<AirportDetails>> RetrieveAirportDataAsync()
            => GetReferenceDataAsync<AirportDetails>("/Airportdetail");
    }
}
