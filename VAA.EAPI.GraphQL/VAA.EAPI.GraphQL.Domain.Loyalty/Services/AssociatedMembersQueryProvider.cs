using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class AssociatedMembersQueryProvider : CustomerTransactionsProviderBase, IAssociatedMembersQueryProvider
    {
        public AssociatedMembersQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector) { }

        public async Task<AssociatedMembersDownloadResponse> Query(string campaignCode)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/AssociatedMembersDownload";
            string paramString = $"?CampaignCode={campaignCode ?? string.Empty}";
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
                            };

            return (await _connector.GetAsync<AssociatedMembersDownloadResponse>(string.Concat(uri, paramString), headers, token)).Response;
        }
    }
}
