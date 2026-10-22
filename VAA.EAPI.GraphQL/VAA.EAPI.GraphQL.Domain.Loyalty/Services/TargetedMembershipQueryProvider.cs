using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class TargetedMembershipQueryProvider : CustomerTransactionsProviderBase, ITargetedMembershipQueryProvider
    {
        public TargetedMembershipQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        { }

        public async Task<IEnumerable<TargetedMembership>> Query(string customerId)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl).TrimEnd('/')}/{customerId}/TargetedMemberships";
            var custTransResponse = await _connector.GetAsync<TargetedMembershipResponse>(uri, GetHeaders(), await GetToken());
            return custTransResponse.Response.TargetedMembershipDocuments;
        }
    }
}
