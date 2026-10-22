using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class PromotionsListQueryProvider : CustomerTransactionsProviderBase, IPromotionsListQueryProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        public PromotionsListQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<PromotionsDownloadResponse> Query()
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/PromotionsDownload";
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
                            };
            var promotionsListResponse = await _connector.GetAsync<PromotionsDownloadResponse>(uri, headers, token);
            return promotionsListResponse.Response;
        }
    }
}
