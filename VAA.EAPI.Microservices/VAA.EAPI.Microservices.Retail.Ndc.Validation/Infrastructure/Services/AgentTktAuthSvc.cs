using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services.Interfaces;
using Environment = VAA.EAPI.Microservices.Retail.Ndc.Validation.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services
{
    public class AgentTktAuthSvc : IAgentTktAuthSvc
    {
        private readonly IHttpConnector _apiConnector;

        public AgentTktAuthSvc(IHttpConnector apiConnector)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<AgentTktAuthResponse> GetAgentTktAuthResponse(string agencyID, string requestAgencyType, string environment)
        {
            string url = ConfigReader.Get(Environment.AgentTktAuthorityEndpointGet).Replace("{reqAgentType}", requestAgencyType).Replace("{env}", environment).Replace("{agentid}", agencyID);

            Dictionary<string, string> headers = new()
            {
                { "CLIENT-KEY", ConfigReader.Get(Environment.AgentTktAuthorityClientId) },
                { "CLIENT-SECRET", ConfigReader.Get(Environment.AgentTktAuthoritySecretKey) }
            };

            return await _apiConnector.GetAsync<AgentTktAuthResponse>(uri: url, headers: headers, string.Empty);
        }

        public async Task<AgentTktAuthResponse> UpdateAgentTktAuthResponse(string agencyID, string requestAgencyType, string environment)
        {
            string url = ConfigReader.Get(Environment.AgentTktAuthorityEndpointUpdate).Replace("{reqAgentType}", requestAgencyType).Replace("{env}", environment).Replace("{agentid}", agencyID);

            Dictionary<string, string> headers = new()
            {
                { "CLIENT-KEY", ConfigReader.Get(Environment.AgentTktAuthorityClientId) },
                { "CLIENT-SECRET", ConfigReader.Get(Environment.AgentTktAuthoritySecretKey) }
            };

            return await _apiConnector.SendAsync<AgentTktAuthResponse>(uri: url, headers: headers, string.Empty, verb: System.Net.Http.HttpMethod.Post, string.Empty);
        }
    }
}
