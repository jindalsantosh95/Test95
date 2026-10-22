using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class PromotionSearchQueryProvider : CustomerTransactionsProviderBase, IPromotionSearchQueryProvider
    {
        public PromotionSearchQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector) { }

        public async Task<List<PromotionSearchData>> Query(string PromotionId, string PromotionName, string StartDateF, string EndDateF, string StartDateT, string EndDateT, string PartnerName, string Active)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/PromotionSearch";
            string paramString = $"?PromotionId={PromotionId ?? string.Empty}&PromotionName={HttpUtility.UrlEncode(PromotionName) ?? string.Empty}&StartDateF={StartDateF ?? string.Empty}&EndDateF={EndDateF ?? string.Empty}&StartDateT={StartDateT ?? string.Empty}&EndDateT={EndDateT ?? string.Empty}&PartnerName={HttpUtility.UrlEncode(PartnerName) ?? string.Empty}&Active={Active ?? string.Empty}";
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
                            };
            var promotions = await _connector.GetAsync<PromotionSearchResponse>(string.Concat(uri, paramString), headers, token);
            return promotions.Response.Promotions;
        }
    }
}