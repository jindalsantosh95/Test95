using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Services
{
    public sealed class ReferenceServiceProvider : IRefServiceProvider
    {
        private static Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.ReferenceMSOcpKey) } };

        private readonly IHttpConnector _connector;

        public ReferenceServiceProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<IEnumerable<AirlineRefData>> RetrieveAirlineDataAsync()
        {
            string airlineDetailsUri = ConfigReader.Get(Constants.Environment.ReferenceMSAirlineDetailsUrl);

            return (await _connector.SendAsync<IEnumerable<AirlineRefData>>(airlineDetailsUri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<IEnumerable<AirportRefData>> RetrieveAirportDataAsync()
        {
            string airportDetailsUri = ConfigReader.Get(Constants.Environment.ReferenceMSAirportDetailsUrl);

            return (await _connector.SendAsync<IEnumerable<AirportRefData>>(airportDetailsUri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<IEnumerable<AircraftRefData>> RetrieveAircraftDataAsync()
        {
            string aircraftDetailsUri = ConfigReader.Get(Constants.Environment.ReferenceMSAircraftDetailsUrl);

            return (await _connector.SendAsync<IEnumerable<AircraftRefData>>(aircraftDetailsUri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<IEnumerable<CabinRefData>> RetrieveCabinDataAsync()
        {
            string cabinDetailsUri = ConfigReader.Get(Constants.Environment.ReferenceMSCabinDetailsUrl);

            return (await _connector.SendAsync<IEnumerable<CabinRefData>>(cabinDetailsUri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<IEnumerable<CurrencyNumberRefData>> RetrieveAir4CurrencyNumberAsync()
        {
            string currencyNumberUri = ConfigReader.Get(Constants.Environment.ReferenceMSAir4CurrencyNumberUrl);

            return (await _connector.SendAsync<IEnumerable<CurrencyNumberRefData>>(currencyNumberUri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<IEnumerable<CountryRefData>> RetrieveCountriesAsync()
        {
            string countriesUri = ConfigReader.Get(Constants.Environment.ReferenceMSCountriesUrl);

            return (await _connector.SendAsync<IEnumerable<CountryRefData>>(countriesUri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<IEnumerable<CabinClassRefDetails>> RetrieveCabinClassAsync()
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.ReferenceMsUrl).TrimEnd('/')}/CabinClass";
            return (await _connector.SendAsync<IEnumerable<CabinClassRefDetails>>(uri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<IEnumerable<BlackListedEmails>> RetrieveBlackListedEmailsAsync()
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.ReferenceMsUrl).TrimEnd('/')}/BlackListedEmails";
            return (await _connector.SendAsync<IEnumerable<BlackListedEmails>>(uri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }
    }
}
