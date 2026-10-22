using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.UpgradeVACMember;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class ReferenceDataMutationProvider : IReferenceDataMutationProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public ReferenceDataMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<string> GetSegmentsDistance(string originAirportCode, string destinationAirportCode)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.RefDataMSGenericUrl).Replace("{0}", Common.Constants.Application.RefMsGetSegmentDistanceUrl + originAirportCode + "/" + destinationAirportCode);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.RefDataMSAppId));

            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.RefDataMSOcpKey) }
                            };

            return (await _connector.GetAsync<string>($"{uri}", headers, token)).Response;
        }

        public async Task<AirportDistanceAndCountries> GetSegmentDistanceAndCountry(string originAirportCode, string destinationAirportCode)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.RefDataMSGenericUrl).Replace("{0}", Common.Constants.Application.RefMsGetSegmentDistanceAndCountryUrl + originAirportCode + "/" + destinationAirportCode);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.RefDataMSAppId));

            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.RefDataMSOcpKey) }
                            };

            return (await _connector.GetAsync<AirportDistanceAndCountries>($"{uri}", headers, token)).Response;
        }

        public async Task<CabinClassDetailsResponse> GetCabinClassDetails(CabinClassDetailsRequest cabinClassDetailsRequest)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.RefDataMSGenericUrl).Replace("{0}", Common.Constants.Application.RefMsGetCabinClassDetailsUrl);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.RefDataMSAppId));

            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.RefDataMSOcpKey) }
                            };

            return (await _connector.SendAsync<CabinClassDetailsResponse>($"{uri}", headers, token, HttpMethod.Post, Common.Serialisation.Json.Serialize(cabinClassDetailsRequest))).Response;
        }

        public async Task<IEnumerable<CountryDetails>> GetCountryDetails()
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.RefDataMSGenericUrl).Replace("{0}", Common.Constants.Application.RefMsGetCountryDetailsUrl);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.RefDataMSAppId));

            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.RefDataMSOcpKey) }
                            };

            return (await _connector.GetAsync<IEnumerable<CountryDetails>>($"{uri}", headers, token)).Response;
        }
    }
}