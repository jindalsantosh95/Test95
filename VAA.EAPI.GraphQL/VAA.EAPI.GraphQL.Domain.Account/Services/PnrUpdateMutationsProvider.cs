using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class PnrUpdateMutationsProvider : IPnrUpdateMutationsProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public PnrUpdateMutationsProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<string> StampingFCNumberToPNR(UpdatePNRRequest request)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.StampingFCNumberToPNRUrl)}{Common.Constants.Application.UpdatePnrEndpoint}";

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.OrderManagementAppId));

            Dictionary<string, string> headers = new()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.OrderManagementOcpKey) }
            };

            return (await _connector.SendAsync<string>(uri, headers, token, HttpMethod.Patch, Json.Serialize(request))).Response;
        }
    }
}
