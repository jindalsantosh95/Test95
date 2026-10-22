using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class CampaignSearchQueryProvider : CustomerTransactionsProviderBase, ICampaignSearchQueryProvider
    {
        public CampaignSearchQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector) { }

        public async Task<List<CampaignSearchData>> Query(CampaignSearchRequest searchRequest)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/campaigns/search";
            string paramString = $"?CampaignId={searchRequest.CampaignId ?? string.Empty}&CampaignName={HttpUtility.UrlEncode(searchRequest.CampaignName) ?? string.Empty}&StartDateF={searchRequest.StartDateF ?? string.Empty}&EndDateF={searchRequest.EndDateF ?? string.Empty}&StartDateT={searchRequest.StartDateT ?? string.Empty}&EndDateT={searchRequest.EndDateT ?? string.Empty}&CampaignCode={HttpUtility.UrlEncode(searchRequest.CampaignCode) ?? string.Empty}&Status={searchRequest.Status ?? string.Empty}&Type={HttpUtility.UrlEncode(searchRequest.Type) ?? string.Empty}";
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
                            };

            var campaigns = await _connector.GetAsync<CampaignSearchResponse>(string.Concat(uri, paramString), headers, token);
            return campaigns.Response.Campaigns;
        }
    }
}
