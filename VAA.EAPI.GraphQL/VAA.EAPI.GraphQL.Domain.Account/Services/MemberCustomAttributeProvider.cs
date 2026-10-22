using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class MemberCustomAttributeProvider : CustomerTransactionsProviderBase, IMemberCustomAttributeProvider
    {
        public MemberCustomAttributeProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector) { }

        public async Task<IEnumerable<MemberCustomAttributesAccount>> GetMemberCustomerAttributeData(string customerId)
        {
            string uri = (ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl) + Common.Constants.Application.GetMemberCustomAttributes).Replace("{customerID}", customerId);

            return (await _connector.GetAsync<MemberCustomAttributesResponseAccount>(uri, GetHeaders(), await GetToken())).Response.MemberCustomAttributesDocuments;
        }
    }
}
