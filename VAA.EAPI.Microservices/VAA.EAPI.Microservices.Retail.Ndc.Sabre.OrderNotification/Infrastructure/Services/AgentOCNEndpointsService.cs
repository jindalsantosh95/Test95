using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain.IATAAcknowledgement;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Infrastructure.Services
{
    public class AgentOCNEndpointsService(IHttpConnector httpConnector, ILogger<AgentOCNEndpointsService> logger) : IAgentOCNEndpointService
    {
        private readonly IHttpConnector _httpConnector = httpConnector;
        private readonly ILogger<AgentOCNEndpointsService> _logger = logger;

        public async Task<IATAAcknowledgement> Send(IATAOrderChangeNotifRQ request , AgentEndpointDetails agentEndpointDetails)
        {
            if (agentEndpointDetails.AuthType.Is("Basic"))
            {
                var credentials = $"{agentEndpointDetails.ClientKey}:{agentEndpointDetails.ClientSecret}";
                var base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials)); 
                    
                var headers = new Dictionary<string, string>()
                {
                    { "Authorization", $"Basic {base64Credentials}"},
                    { "Environment", null }
                };

                try
                {
                    var httpResponse = await _httpConnector.SendAsync(agentEndpointDetails.Endpoint, headers, string.Empty, HttpMethod.Post, Xml.Serialize(request));
                    var response = await httpResponse.Content.ReadAsStringAsync();
                    string responseXml = response.Trim('"').Replace("\\\"", "\"");

                    return Xml.Deserialize<IATAAcknowledgement>(responseXml);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogError(exception, "Agent OCN endpoint request failed. {Operation}", "AgentOrderChangeNotification");
                    return null;
                }
            }
            else if (agentEndpointDetails.AuthType.Is("OAuth"))
            {
                var tokenRequestParams = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", agentEndpointDetails.ClientKey },
                    { "client_secret", agentEndpointDetails.ClientSecret }
                };

                var tokenRequestContent = new FormUrlEncodedContent(tokenRequestParams);

                var tokenResponse = await _httpConnector.SendAsync(agentEndpointDetails.TokenUrl, null, string.Empty, HttpMethod.Post, tokenRequestContent);

                var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
                var tokenObj = JsonSerializer.Deserialize<JsonElement>(tokenResponseContent);

                if (tokenObj.TryGetProperty("access_token", out var accessTokenElement))
                {
                    var accessToken = accessTokenElement.GetString();

                    var headers = new Dictionary<string, string>()
                    {
                        { "Authorization", $"Bearer {accessToken}" },
                    };

                    var httpResponse = await _httpConnector.SendAsync(agentEndpointDetails.Endpoint, headers, string.Empty, HttpMethod.Post, Xml.Serialize(request));
                    var response = await httpResponse.Content.ReadAsStringAsync();
                    string responseXml = response.Trim('"').Replace("\\\"", "\"");

                    return Xml.Deserialize<IATAAcknowledgement>(responseXml);
                }
            }

            return null;
        }
    }
}
