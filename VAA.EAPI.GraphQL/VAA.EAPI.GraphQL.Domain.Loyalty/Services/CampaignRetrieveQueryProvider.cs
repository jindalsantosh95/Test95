using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class CampaignRetrieveQueryProvider : CustomerTransactionsProviderBase, ICampaignRetrieveQueryProvider
    {
        public CampaignRetrieveQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector) { }

        public async Task<CampaignRetrieveData> Query(string campaignId, string campaignCode)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/campaigndetails";
            string paramString = $"?CampaignId={campaignId ?? string.Empty}&CampaignCode={HttpUtility.UrlEncode(campaignCode) ?? string.Empty}";
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
                            };

            return (await _connector.GetAsync<CampaignRetrieveData>(string.Concat(uri, paramString), headers, token)).Response;
        }
    }
}
