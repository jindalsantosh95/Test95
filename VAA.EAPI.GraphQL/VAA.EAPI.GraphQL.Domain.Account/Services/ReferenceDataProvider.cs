using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Reference;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class ReferenceDataProvider(IHttpConnector connector) : IReferenceDataProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.ReferenceMsOcpKey) } };

        private readonly IHttpConnector _connector = connector ?? throw new ArgumentNullException(nameof(connector));

        public async Task<IEnumerable<CountryRefData>> RetrieveCountriesAsync()
        {
            string countriesUri = ConfigReader.Get(Common.Constants.Environment.ReferenceMsUrl) + Constants.Application.ReferenceCountriesEndpoint;

            return (await _connector.SendAsync<IEnumerable<CountryRefData>>(countriesUri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }
    }
}
