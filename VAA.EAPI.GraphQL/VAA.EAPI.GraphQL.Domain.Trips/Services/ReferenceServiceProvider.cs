using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public class ReferenceServiceProvider : IReferenceServiceProvider
    {
        private static Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.ReferenceMsOcpKey) } };
        private readonly IHttpConnector _connector;

        public ReferenceServiceProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<IEnumerable<CabinRefData>> RetrieveCabinDataAsync()
        {
            string cabinDetailsUri = $"{ConfigReader.Get(Common.Constants.Environment.ReferenceMsUrl).TrimEnd('/')}/cabinDetails";

            return (await _connector.SendAsync<IEnumerable<CabinRefData>>(cabinDetailsUri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<IEnumerable<SSRMealRefData>> RetrieveSSRMealDataAsync()
        {
            string url = $"{ConfigReader.Get(Common.Constants.Environment.ReferenceMsUrl).TrimEnd('/')}/ssrmealdetails";
            return (await _connector.SendAsync<IEnumerable<SSRMealRefData>>(url, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<IEnumerable<AirportRefData>> RetrieveAirportDataAsync()
        {
            string airportDetailsUri = $"{ConfigReader.Get(Common.Constants.Environment.ReferenceMsUrl).TrimEnd('/')}/airportDetail";

            return (await _connector.SendAsync<IEnumerable<AirportRefData>>(airportDetailsUri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<IEnumerable<FQTProgramRefData>> RetrieveLoyaltyProgramsDataAsync()
        {
            string fqtProgramDetailsUri = $"{ConfigReader.Get(Common.Constants.Environment.ReferenceMsUrl).TrimEnd('/')}/fqtprograms";

            return (await _connector.SendAsync<IEnumerable<FQTProgramRefData>>(fqtProgramDetailsUri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<IEnumerable<CurrencyNumberRefData>> RetrieveAir4CurrencyNumberAsync()
        {
            string currencyNumberUri = $"{ConfigReader.Get(Common.Constants.Environment.ReferenceMsUrl).TrimEnd('/')}/Air4CurrencyNumberDetails";

            return (await _connector.SendAsync<IEnumerable<CurrencyNumberRefData>>(currencyNumberUri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<IEnumerable<AirlineRefData>> RetrieveAirlineDetails()
        {
            string currencyNumberUri = $"{ConfigReader.Get(Common.Constants.Environment.ReferenceMsUrl).TrimEnd('/')}/airlinedetails";

            return (await _connector.SendAsync<IEnumerable<AirlineRefData>>(currencyNumberUri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }


        public async Task<IEnumerable<PaymentMethod>> RetrievePaymentMethodDetails()
        {
            string paymentMethodDetailsUri = $"{ConfigReader.Get(Common.Constants.Environment.ReferenceMsUrl).TrimEnd('/')}/PaymentMethodDetails";

            return (await _connector.SendAsync<IEnumerable<PaymentMethod>>(paymentMethodDetailsUri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }
    }
}