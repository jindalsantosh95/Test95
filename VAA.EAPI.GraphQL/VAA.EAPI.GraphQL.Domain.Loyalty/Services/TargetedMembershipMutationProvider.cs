using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class TargetedMembershipMutationProvider<T> : CustomerTransactionsProviderBase, ITargetedMembershipMutationProvider<T>
    {
        public TargetedMembershipMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        { }

        public async Task<T> Mutate(TargetedMembershipMutationRequest request)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl).TrimEnd('/')}/{request.CustomerId}/TargetedMemberships";
            var contactNote = await _connector.SendAsync<T>(uri, GetHeaders(), await GetToken(), request.RequestType, Common.Serialisation.Json.Serialize(request));

            return contactNote.Response;
        }
    }
}
