using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Campaign;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class CampaignProvider : ICampaignProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CampaignManagementOcpKey) } };

        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public CampaignProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<ValidateEligibilityResponse> ValidateCampaign(string uniqueId)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl)}{Common.Constants.Application.ValidateUniqueIdForPCM}?Uniqueid=" + uniqueId;

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));

            return (await _connector.GetAsync<ValidateEligibilityResponse>(uri, _headers, token)).Response;
        }

        public async Task<string> AssignCampaign(AssignCampaignRequest assignCampaignRequest)
        {
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));

            return (await _connector.SendAsync<string>($"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl)}{Common.Constants.Application.AssignCampaign}", _headers, token, HttpMethod.Post, Json.Serialize(assignCampaignRequest))).Response;
        }

        public async Task<string> UpdateCampaign(UpdateCampaignRequest updateCampaignRequest)
        {
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CampaignManagementAppId));

            return (await _connector.SendAsync<string>($"{ConfigReader.Get(Common.Constants.Environment.CampaignManagementMsUrl)}{Common.Constants.Application.UpdateFCnumberForPCM}", _headers, token, HttpMethod.Post, Json.Serialize(updateCampaignRequest))).Response;
        }
    }
}
