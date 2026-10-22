using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Email.Entities;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Infrastructure.Email.Client
{
    public class WorkflowClient : IWorkflowClient
    {
        private readonly ILogger<WorkflowClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _clientId;
        private readonly string _authority;
        private readonly string _url;

        public WorkflowClient(IHttpClientFactory httpClientFactory, ILogger<WorkflowClient> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public WorkflowClient()
        {
            _clientId = ConfigReader.Get(Constants.Environment.WorkFlowClientId);
            _authority = ConfigReader.Get(Constants.Environment.AzureADEndpoint);
            _url = ConfigReader.Get(Constants.Environment.WorkFlowAPIUri);
            Constants.Application.WorkFlowClientSecretCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Environment.WorkFlowClientSecret, Constants.Application.WorkFlowClientSecretCached);
            Constants.Application.WorkFlowResourceCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Environment.WorkFlowResource, Constants.Application.WorkFlowResourceCached);
            Constants.Application.WorkFlowOcpApimSubscriptionKeyCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Environment.WorkFlowOcpApimSubscriptionKey, Constants.Application.WorkFlowOcpApimSubscriptionKeyCached);
        }

        public async Task<KeyValuePair<bool, string>> SendEmailCommunication(WorkFlowRequest request, CancellationToken cancellationToken)
        {
            WorkflowClient client = new();
            var token = await ExternalHelper.GetBearerToken(client._clientId, Constants.Application.WorkFlowClientSecretCached, client._authority, Constants.Application.WorkFlowResourceCached);

            HttpClient httpClient = _httpClientFactory.CreateClient();
            StringContent workFlowRequest = new(Json.Serialize(request), Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Add(Constants.Constants.OcpApimSubscriptionKeyName, Constants.Application.WorkFlowOcpApimSubscriptionKeyCached);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.Constants.OcpApimBearerKeyName, token);
            httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
            httpClient.BaseAddress = new Uri(client._url);
            using HttpResponseMessage httpResponse = await httpClient.PostAsync(client._url, workFlowRequest);

            if (!httpResponse.IsSuccessStatusCode)
            {
                string error = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogWarning($"WorkFlow API Error Response: {error}");
                return new KeyValuePair<bool, string>(false, error);
            }

            string response = await httpResponse.Content.ReadAsStringAsync();
            return new KeyValuePair<bool, string>(true, response);
        }
    }
}
