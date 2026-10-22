using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class InflightConnectivityMutationProvider : IInflightConnectivityMutationProvider
    {
        private readonly IHttpConnector _connector;

        public InflightConnectivityMutationProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<AccountInflightDeviceStateResponse> CreateInflightConnectivity(AccountEnableIFWifiForDeviceRequest request)
        {
            var headers = new Dictionary<string, string>
                {
                    {
                        "Ocp-Apim-Subscription-Key", ConfigReader.Get(Environment.MyAccountInflightConnectivityOcpKey)
                    }
                };

            var response = await _connector.SendAsync<AccountInflightDeviceStateResponse>(
                $"{ConfigReader.Get(Environment.MyAccountInflightConnectivityMSURL)}{Application.InflightConnectivityEnableWifiEndpoint}",
                headers,
                null,
                HttpMethod.Post,
                Json.Serialize(request));

            return response.Response;
        }
    }
}