using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberCardLink;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class VredCardDetailsQueryProvider : CustomerTransactionsProviderBase, IVredCardDetailsQueryProvider
    {
        public VredCardDetailsQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        {
        }

        public async Task<VredCardDetails> GetVredCardDetails(string memberNumber)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.VredMsUrl);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.VRedAppId));

            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.VRedOcpSubID) }
                            };

            return (await _connector.GetAsync<VredCardDetails>($"{uri}{Common.Constants.Application.VredCardLinksEndPoint}".Replace("{MemberNumber}", memberNumber), headers, token)).Response;
        }

        public async Task<VRedResponse> VredPointBalance(string memberNumber)
        {
            string vredPointUri = $"{ConfigReader.Get(Common.Constants.Environment.VredMsUrl)}{Common.Constants.Application.VredMSVirginPointsEndPoint}".Replace("{memberID}", memberNumber);

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.VRedAppId));

            Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.VRedOcpSubID) } };

            return (await _connector.GetAsync<VRedResponse>(vredPointUri, headers, token)).Response;
        }

        public async Task<VredStatementResponse> GetVredStatements(string memberNumber)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.VredMsUrl)}{Common.Constants.Application.GetVredStatements}".Replace("{MemberNumber}", memberNumber);

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.VRedAppId));

            Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.VRedOcpSubID) } };

            return (await _connector.SendAsync<VredStatementResponse>(uri, headers, token, HttpMethod.Get, string.Empty)).Response;
        }
    }
}
