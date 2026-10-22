using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class PromotionCampaignSearchQueryProvider : CustomerTransactionsProviderBase, IPromotionCampaignSearchQueryProvider
    {
        public PromotionCampaignSearchQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector) { }

        public async Task<List<PromotionCampaignSearchData>> Query(string PromotionId)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/PromotionCampaignSearch";
            string paramString = $"?PromotionId={PromotionId ?? string.Empty}";
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
                            };
            var promotionCampaigns = await _connector.GetAsync<PromotionCampaignSearchResponse>(string.Concat(uri, paramString), headers, token);
            return promotionCampaigns.Response.PromotionCampaigns;
        }
    }
}