using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services
{
    public class TargetedMemberDocumentService : ITargetedMemberDocumentService
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ILogger<TargetedMemberDocumentService> _logger;

        public TargetedMemberDocumentService(IHttpConnector apiConnector, ILogger<TargetedMemberDocumentService> logger)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<TargetedMembershipResponse> GetMemberCampaigns(string customerId)
        {
            _logger.LogInformation("Customer Transaction MS method GetTargetedMembershipByCustomerID call");
            string clientId = ConfigReader.Get(Constants.Environment.CampaignManagementAppId);
            string clientSecret = ConfigReader.Get(Constants.Environment.CampaignManagementClientSecret);
            string authority = ConfigReader.Get(Constants.Environment.AzureADEndpoint);
            string resource = ConfigReader.Get(Constants.Environment.CustomerTxnMSResourceId);
            string endpoint = ConfigReader.Get(Constants.Environment.CustomerTxnMSDetailsEndpoint);
            string url = endpoint + customerId + "/targetedmemberships";
            string token = await ExternalHelper.GetBearerToken(clientId, clientSecret, authority, resource);
            Dictionary<string, string> headers = new()
            {
                { Constants.Environment.OcpKeyHeader, ConfigReader.Get(Constants.Environment.CampaignManagementOcpKey) }
            };
            return await _apiConnector.GetAsync<TargetedMembershipResponse>(uri: url, headers: headers, bearerToken: token);
        }
    }
}
