using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class PromotionManagementMutationProvider : IPromotionManagementMutationProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public PromotionManagementMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }
        public async Task<string> UpdatePromotion(PromotionUpdateMutationRequest promotion)
        {
            string updatePromotionUri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/Promotion/{promotion.PromotionId}";

            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));

            //Invoke update promotion
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
                            };

            var response = await _connector.SendAsync<PromotionUpdateMutationResponse>(updatePromotionUri, headers, token, HttpMethod.Patch, Json.Serialize(promotion));
            return response.Response.Value;
        }
        public async Task<string> CreatePromotion(PromotionCreateMutationRequest promotion)
        {
            string createPromotionUri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/Promotion";
            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));

            //Invoke create promotion
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
                            };

            var response = await _connector.SendAsync<PromotionCreateMutationResponse>(createPromotionUri, headers, token, HttpMethod.Post, Json.Serialize(promotion));
            return response.Response.Value;
        }
    }
}
