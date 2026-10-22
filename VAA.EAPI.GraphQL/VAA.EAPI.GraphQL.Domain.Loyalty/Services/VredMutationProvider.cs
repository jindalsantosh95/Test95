using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class VredMutationProvider : CustomerTransactionsProviderBase, IVredMutationProvider
    {
        public VredMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        {
        }

        public async Task<VRedTransactionResponse> PostTransactionToVred(VredSyncTransactionRequest vredTransactionRequest)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.VredMsUrl);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.VRedAppId));

            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.VRedOcpSubID) }
                            };

            return (await _connector.SendAsync<VRedTransactionResponse>($"{uri}{Common.Constants.Application.VredTransactionEndPoint}", headers, token, HttpMethod.Post, Common.Serialisation.Json.Serialize(vredTransactionRequest))).Response;
        }

        public async Task<VRedTransactionResponse> CancelTransactionVred(VredCancelTransactionRequest vredCancelRequest)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.VredMsUrl);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.VRedAppId));

            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.VRedOcpSubID) }
                            };

            return (await _connector.SendAsync<VRedTransactionResponse>($"{uri}{Common.Constants.Application.VredCancelltionEndPoint}", headers, token, HttpMethod.Post, Common.Serialisation.Json.Serialize(vredCancelRequest))).Response;
        }
    }
}
