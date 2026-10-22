using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class PDMProvider : IPDMProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        private readonly static Dictionary<string, string> headersPdmMS = new()
        {
            {Application.OcpKeyHeader, ConfigReader.Get(Environment.CommonEAPIOcpAPIMKey)}
        };

        public PDMProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<VSFlownDetailsRespone> PDMGetFlownDetails(string ticketNumber)
        {
            return (await _connector.GetAsync<VSFlownDetailsRespone>($"{ConfigReader.Get(Environment.PdmMSUrl)}{Application.VsFlownDetailsEndPoint}".Replace("{TicketNumber}", ticketNumber), headersPdmMS, await GetToken())).Response;
        }

        private async Task<string> GetToken()
        {
            return await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.PdmMSAppId));
        }

        public async Task<string> UpdateTicketStatus(TicketStatusRequest ticketstatusRequest)
        {
            return (await _connector.SendAsync<string>($"{ConfigReader.Get(Environment.PdmMSUrl)}{Application.UpdateTicketStatusEndpoint}", headersPdmMS, await GetToken(), HttpMethod.Post, Json.Serialize(ticketstatusRequest))).Response;
        }
    }
}
