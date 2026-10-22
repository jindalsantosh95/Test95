using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class VredPointProvider : IVredPointProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly static Dictionary<string, string> VRedheaders = new Dictionary<string, string>()
        {
            { Common.Constants.Application.OcpKeyHeader,  ConfigReader.Get(Common.Constants.Environment.VRedOcpSubID)}
        };


        public VredPointProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<VRedResponse> VredPointBalance(string memberNumber)
        {
            string vredPointUri = $"{ConfigReader.Get(Common.Constants.Environment.VredMsUrl)}{Common.Constants.Application.VredMSVirginPointsEndPoint}".Replace("{memberID}", memberNumber);

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.VRedAppId));

            Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.VRedOcpSubID) } };

            return (await _connector.GetAsync<VRedResponse>(vredPointUri, headers, token)).Response;
        }

    }
}
