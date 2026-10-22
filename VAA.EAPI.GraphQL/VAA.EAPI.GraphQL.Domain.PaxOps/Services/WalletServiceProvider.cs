using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.WalletMS;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Services
{
    public class WalletServiceProvider(IHttpConnector connector) : IWalletServiceProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CommonEAPIOcpAPIMKey) } };
        private static readonly string url = ConfigReader.Get(Common.Constants.Environment.WalletGeneratePassURL);

        protected readonly IHttpConnector _connector = connector ?? throw new ArgumentNullException(nameof(connector));

        public async Task<AirshipWalletResponse> GetAirshipWalletLinksAsync(AirshipPassRequest passLinksRequest)
        {
            ArgumentNullException.ThrowIfNull(passLinksRequest);

            var endpoint = $"{url.TrimEnd('/')}/BoardingPass";

            return (await _connector.SendAsync<AirshipWalletResponse>(endpoint, _headers, string.Empty, HttpMethod.Post, Json.Serialize(passLinksRequest))).Response;
        }
    }
}
