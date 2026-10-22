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
    public class CampaignManagementMutationProvider : ICampaignManagementMutationProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public CampaignManagementMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }
        public async Task<string> UpdateCampaign(CampaignUpdateMutationRequest campaign)
        {
            string updateCampaignUri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/UpdateCampaign";
            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));

            //Invoke update campaign
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
                            };

            var response = await _connector.SendAsync<CampaignUpdateMutationResponse>(updateCampaignUri, headers, token, HttpMethod.Patch, Json.Serialize(campaign));
            return response.Response.Value;
        }

        public async Task<string> AssignCampaign(CampaignAssignRequest campaignAssignRequest)
        {
            string assignCampaignUri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/AssignCampaign";
            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));

            //Invoke update campaign
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
                            };

            return (await _connector.SendAsync<string>(assignCampaignUri, headers, token, HttpMethod.Post, Json.Serialize(campaignAssignRequest))).Response;
        }

        public async Task<string> CreateCampaign(CampaignCreateMutationRequest campaign)
        {
            string createCampaignUri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/CreateCampaign";
            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));

            //Invoke create campaign
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
                            };

            var response = await _connector.SendAsync<CampaignCreateMutationResponse>(createCampaignUri, headers, token, HttpMethod.Post, Json.Serialize(campaign));
            return response.Response.Value;
        }

        public async Task<CampaignUnassignMutationResponse> UnassignCampaign(CampaignUnassignMutationRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.CustomerId) ||
                string.IsNullOrWhiteSpace(request.DocumentId))
                throw new ArgumentException($"'{nameof(request.CustomerId)}' And {nameof(request.DocumentId)} must be spefied.");

            string unassignCampaignUri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/UnassignCampaign";
            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));

            //Invoke update campaign
            Dictionary<string, string> headers = new()
                        {
                            { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
                        };

            return (await _connector.SendAsync<CampaignUnassignMutationResponse>(unassignCampaignUri, headers, token, HttpMethod.Post, Json.Serialize(request))).Response;
        }

        public async Task<string> LinkCampaignPromotion(CampaignLinkingMutationRequest campaign)
        {
            string linkCampaignPromotionUri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/LinkCampaignPromotion";
            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));

            //Invoke Link campaign promotion
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
                            };

            var response = await _connector.SendAsync<CampaignUpdateMutationResponse>(linkCampaignPromotionUri, headers, token, HttpMethod.Patch, Json.Serialize(campaign));
            return response.Response.Value;
        }

        public async Task<string> DelinkCampaignPromotion(CampaignLinkingMutationRequest campaignDelinkingMutationRequest)
        {
            string delinkCampaignPromotionUri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl).TrimEnd('/')}/DelinkCampaignPromotion";
            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));

            //Invoke Delink campaign promotion
            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) }
                            };

            var response = await _connector.SendAsync<CampaignUpdateMutationResponse>(delinkCampaignPromotionUri, headers, token, HttpMethod.Patch, Json.Serialize(campaignDelinkingMutationRequest));
            return response.Response.Value;
        }
    }
}
