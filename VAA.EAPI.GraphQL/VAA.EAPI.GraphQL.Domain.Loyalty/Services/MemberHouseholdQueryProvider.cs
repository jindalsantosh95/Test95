using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class MemberHouseholdQueryProvider : CustomerTransactionsProviderBase, IMemberHouseholdQueryProvider
    {
        public MemberHouseholdQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector) { }

        public async Task<MemberHouseholdResponse> Query(string customerId)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.LoyaltyMemberManagementMsUrl)}{Common.Constants.Application.MemberHouseholdRetrievalEndPoint}".Replace("{custid}", customerId);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.MemberManagementAppId));
            Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.MemberManagementOcpKey) } };
            var memberHouseholdResponse = await _connector.GetAsync<MemberHouseholdResponse>(uri, headers, token);

            return memberHouseholdResponse.Response;
        }
    }
}