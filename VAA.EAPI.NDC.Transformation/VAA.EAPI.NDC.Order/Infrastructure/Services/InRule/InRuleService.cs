using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.InRule;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.InRule
{
    public class InRuleService : IInRuleService
    {
        private static readonly TimeSpan DefaultLookupTimeout = TimeSpan.FromSeconds(5);
        private readonly IHttpConnector _httpConnector;
        private readonly IOAuthTokenProvider _oauthTokenProvider;
        private readonly ILogger<InRuleService> _logger;
        private readonly TimeSpan _lookupTimeout;

        public InRuleService(IHttpConnector httpConnector, IOAuthTokenProvider oauthTokenProvider, ILogger<InRuleService> logger)
            : this(httpConnector, oauthTokenProvider, logger, DefaultLookupTimeout)
        {
        }

        internal InRuleService(IHttpConnector httpConnector, IOAuthTokenProvider oauthTokenProvider, ILogger<InRuleService> logger, TimeSpan lookupTimeout)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _oauthTokenProvider = oauthTokenProvider ?? throw new ArgumentNullException(nameof(oauthTokenProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lookupTimeout = lookupTimeout > TimeSpan.Zero
                ? lookupTimeout
                : throw new ArgumentOutOfRangeException(nameof(lookupTimeout));
        }

        public async Task<DisruptionResponse> GetDisruptionResponseAsync(DisruptionRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            if (request.Event is null || request.Order is null)
            {
                return null;
            }

            var requestUrl = ConfigReader.Try(Constants.Environment.InRuleUrl, string.Empty);

            if (requestUrl.IsNullOrWhiteSpace())
            {
                _logger.LogWarning(
                    "Skipping InRule disruption evaluation because no endpoint is configured for correlation {CorrelationId}.",
                    request.CorrelationId);
                return null;
            }

            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(_lookupTimeout);

            var token = await _oauthTokenProvider
                .AcquireTokenAsync(ConfigReader.Get(Constants.Environment.InRuleScope))
                .WaitAsync(timeoutSource.Token);

            if (token.IsNullOrWhiteSpace())
            {
                _logger.LogWarning(
                    "Skipping InRule disruption evaluation because OAuth token acquisition was unsuccessful for correlation {CorrelationId}.",
                    request.CorrelationId);
                return null;
            }

            using var content = new StringContent(Json.Serialize(request), Encoding.UTF8, "application/json");
            var headers = new Dictionary<string, string>
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.InRuleSubscriptionKey) }
            };

            using var httpResponseMessage = await _httpConnector.SendAsync(
                requestUrl,
                headers,
                token,
                HttpMethod.Post,
                content,
                timeoutSource.Token);


            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogWarning(
                    "InRule disruption evaluation returned HTTP {StatusCode} for correlation {CorrelationId}.",
                    (int)httpResponseMessage.StatusCode,
                    request.CorrelationId);
                return null;
            }

            var response = await httpResponseMessage.Content.ReadAsStringAsync(timeoutSource.Token);

            if (response.IsNullOrWhiteSpace())
            {
                _logger.LogWarning(
                    "InRule disruption evaluation returned an empty response for correlation {CorrelationId}.",
                    request.CorrelationId);
                return null;
            }

            try
            {
                var inRuleResponse = Json.Deserialize<DisruptionResponse>(response);

                if (inRuleResponse is null)
                {
                    _logger.LogWarning(
                        "InRule disruption evaluation returned a null response for correlation {CorrelationId}.",
                        request.CorrelationId);
                    return null;
                }

                return inRuleResponse;
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(
                    exception,
                    "InRule disruption evaluation returned an invalid response for correlation {CorrelationId}.",
                    request.CorrelationId);
                return null;
            }
        }

    }
}
