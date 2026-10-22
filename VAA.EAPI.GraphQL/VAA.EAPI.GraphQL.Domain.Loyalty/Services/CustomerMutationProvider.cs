using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.UpgradeVACMember;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class CustomerMutationProvider : CustomerTransactionsProviderBase, ICustomerMutationProvider
    {
        public CustomerMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        {
        }
        public async Task<TopMemberAwardResponse> AssignTopMember(string customerId)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl).TrimEnd('/')}/{customerId}/awards/topmember";
            var response = await _connector.SendAsync<TopMemberAwardResponse>(uri, GetHeaders(), await GetToken(), HttpMethod.Post, string.Empty);
            return response.Response;
        }
        public async Task<string> RegisterCampaign(CampaignRegisterRequest request)
        {
            string registerCampaignUri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/RegisterCampaign";
            string token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
            };

            return (await _connector.SendAsync<string>(registerCampaignUri, headers, token, HttpMethod.Post, Json.Serialize(request))).Response;
        }

        public async Task<string> UpgradeVACMember(UpgradeMemberMSRequest request)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CustomerMsUrl).TrimEnd('/')}" + Common.Constants.Application.UpgradeFlyingClubEndpoint;

            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId));

            // Retrieve Customer
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey) }
                            };

            return (await _connector.SendAsync<string>(uri, headers, token, HttpMethod.Post, Json.Serialize(request))).Response;
        }
    }
}