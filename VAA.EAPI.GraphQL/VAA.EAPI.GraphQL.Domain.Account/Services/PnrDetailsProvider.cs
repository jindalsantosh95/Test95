using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Infrastructure.Entities.OrderDataMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class PnrDetailsProvider : IPnrDetailsProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public PnrDetailsProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<PnrDetailsResponse> RetrievePNR(string pnr)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.OrderSearchMSUrl);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.OrderSearchAppId));

            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.OrderSearchOcpKey) }
                            };

            return (await _connector.GetAsync<PnrDetailsResponse>($"{uri}{Common.Constants.Application.OrderSearchMSRetrievePNREndPoint}".Replace("{PNR}", pnr), headers, token)).Response;
        }
    }
}
