using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.SFPD;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class LoyaltySyncMutationProvider<T> : LoyaltySyncProviderBase, ILoyaltySyncMutationProvider<T>
    {
        public LoyaltySyncMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        { }

        public async Task<T> Mutate(SFPDSyncRequest request)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.LoyaltySync);
            return (await _connector.SendAsync<T>($"{uri}{Common.Constants.Application.SyncSFPDEndPoint}", GetHeaders(), await GetToken(), HttpMethod.Post, Common.Serialisation.Json.Serialize(request))).Response;
        }

    }
}
