using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.BulkMemberUpdate;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class BulkMemberUpdateMutationProvider : CustomerTransactionsProviderBase, IBulkMemberUpdateMutationProvider
    {
        public BulkMemberUpdateMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        {
        }

        public async Task<BulkMemberUpdateResponse> BulkUpdateWithFile(BulkMemberUpdateRequest request)
        {
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.MemberManagementAppId));

            string uri = $"{ConfigReader.Get(Common.Constants.Environment.LoyaltyMemberManagementMsUrl).TrimEnd('/')}/{Common.Constants.Application.BulkMemberUpdateEndpoint}";
            var headers = new Dictionary<string, string>
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.MemberManagementOcpKey)}
            };

            var result = await _connector.SendAsync<BulkMemberUpdateResponse>(uri, headers, token, HttpMethod.Post, Json.Serialize(request));

            return result.Response;
        }
    }
}