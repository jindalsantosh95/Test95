using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.Disruption;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Services
{
    public class DisruptionInRuleService : IDisruptionInRuleService
    {
        private readonly IHttpConnector _httpConnector;

        public DisruptionInRuleService(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<InRuleResponse> EvaluateAsync(string requestBody, CancellationToken cancellationToken)
        {
            var inRuleApiUrl = ConfigReader.Get(Constants.EnvironmentSettings.InRuleDisruptionAPIURL);
            var inRuleApiKey = ConfigReader.Get(Constants.EnvironmentSettings.InRuleDisruptionAPIKey);

            if (inRuleApiUrl.IsNullOrWhiteSpace() || inRuleApiKey.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("InRule disruption API configuration is missing.");
            }

            var inRuleRequestBody = Json.Serialize(new InRuleRequest
            {
                Request = new Newtonsoft.Json.Linq.JRaw(requestBody)
            });
            var headers = new Dictionary<string, string>
            {
                ["inrule-apikey"] = inRuleApiKey,
                ["inrule-requestid"] = AsyncContext.Current.CorrelationId.ToString()
            };
            using var timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(ConfigReader.Try<double>(VAA.EAPI.Common.Constants.Environment.HttpClientTimeOutInMilliSeconds, 100_000)));

            using var requestContent = new StringContent(inRuleRequestBody, Encoding.UTF8, "application/json");
            using var httpResponse = await _httpConnector.SendAsync(inRuleApiUrl, headers, null, HttpMethod.Post, requestContent, timeoutCancellationTokenSource.Token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException("InRule disruption API request failed.", null, httpResponse.StatusCode);
            }

            var responseBody = await httpResponse.Content.ReadAsStringAsync(timeoutCancellationTokenSource.Token);

            if (responseBody.IsNullOrWhiteSpace())
            {
                throw new HttpRequestException("InRule disruption API returned an empty response.", null, System.Net.HttpStatusCode.BadGateway);
            }

            try
            {
                var inRuleResponse = Json.Deserialize<InRuleDisruptionResponse>(responseBody);

                if (inRuleResponse?.Output is null)
                {
                    throw new HttpRequestException("InRule disruption API returned an invalid response.", null, System.Net.HttpStatusCode.BadGateway);
                }

                return inRuleResponse.Output;
            }
            catch (JsonException exception)
            {
                throw new HttpRequestException("InRule disruption API returned an invalid response.", exception, System.Net.HttpStatusCode.BadGateway);
            }
        }

        private class InRuleDisruptionResponse
        {
            [JsonProperty("output")]
            public InRuleResponse Output { get; set; }
        }
    }
}
