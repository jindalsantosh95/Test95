using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberCardLink;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class PartnerMembershipDocQueryProvider : CustomerTransactionsProviderBase, IPartnerMembershipDocQueryProvider
    {
        public PartnerMembershipDocQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        {
        }

        public async Task<PartnerMembershipResponse> GetPartnerMembershipDoc(string CustomerID)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl);
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustTransAppId));

            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustTransOcpKey) }
                            };

            return (await _connector.GetAsync<PartnerMembershipResponse>($"{uri}{Common.Constants.Application.RetrievePartnerMembershipEndPoint}".Replace("{customerID}", CustomerID), headers, token)).Response;
        }
    }
}
