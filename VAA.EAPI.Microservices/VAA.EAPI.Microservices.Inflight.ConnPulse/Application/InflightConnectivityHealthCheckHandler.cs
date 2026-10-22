using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Domain;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Helper;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Interfaces;
using Environment = VAA.EAPI.Microservices.Inflight.ConnPulse.Constants.Environment;

namespace VAA.EAPI.Microservices.Inflight.ConnPulse.Application
{
    public sealed class InflightConnectivityHealthCheckHandler : IInflightConnectivityHealthCheckHandler
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IHeartbeatService _starlinkService;
        private readonly ILogger<InflightConnectivityHealthCheckHandler> _logger;

        public InflightConnectivityHealthCheckHandler(
            IHttpConnector httpConnector,
            IHeartbeatService starlinkService,
            ILogger<InflightConnectivityHealthCheckHandler> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _starlinkService = starlinkService ?? throw new ArgumentNullException(nameof(starlinkService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HealthCheckAsync(CancellationToken cancellationToken)
        {
            var portalAsyncCheckUrl = ConfigReader.Get(Environment.CaptivePortalAsyncCheckUrl);
            var inflightConnUrl = ConfigReader.Get(Environment.InflightConnUrl);
            var graphQLUrl = ConfigReader.Get(Environment.GraphQLUrl);

            var portalActiveTask = CaptivePortalActiveWithRetryAsync(Environment.CaptivePortalAsyncCheckUrl, portalAsyncCheckUrl, cancellationToken);
            var inflightConnectivityActiveTask = InflightConnectivityActiveWithRetryAsync(Environment.InflightConnUrl, inflightConnUrl, cancellationToken);
            var graphqlActiveTask = EndpointActiveWithRetryAsync(Environment.GraphQLUrl, graphQLUrl, cancellationToken);

            await Task.WhenAll(portalActiveTask, inflightConnectivityActiveTask, graphqlActiveTask).ConfigureAwait(false);

            var portalActive = portalActiveTask.Result;
            var inflightConnectivityActive = inflightConnectivityActiveTask.Result;
            var graphqlActive = graphqlActiveTask.Result;
            var isHealthy = portalActive && inflightConnectivityActive && graphqlActive;

            if (!isHealthy)
            {
                _logger.LogWarning(
                    "Health check probe reported inactive endpoint(s). CaptivePortalAsyncCheckUrl active: {PortalActive}, InflightConnUrl active: {InflightConnectivityActive}, GraphQLUrl active: {GraphqlActive}",
                    portalActive,
                    inflightConnectivityActive,
                    graphqlActive);
            }

            await _starlinkService.SendHeartbeatAsync(new StarlinkHeartbeatRequest
            {
                Health = isHealthy
            }, cancellationToken);
        }

        private Task<bool> CaptivePortalActiveWithRetryAsync(string endpointName, string url, CancellationToken cancellationToken = default)
            => EndpointActiveWithRetryAsync(endpointName, url, CaptivePortalActiveAsync, cancellationToken);

        private Task<bool> EndpointActiveWithRetryAsync(string endpointName, string url, CancellationToken cancellationToken = default)
            => EndpointActiveWithRetryAsync(endpointName, url, EndpointActiveAsync, cancellationToken);

        private Task<bool> InflightConnectivityActiveWithRetryAsync(string endpointName, string url, CancellationToken cancellationToken = default)
            => EndpointActiveWithRetryAsync(endpointName, url, InflightConnectivityActiveAsync, cancellationToken);

        private async Task<bool> EndpointActiveWithRetryAsync(
            string endpointName,
            string url,
            Func<Uri, string, CancellationToken, Task<bool>> activeProbeAsync,
            CancellationToken cancellationToken = default)
        {
            if (url.IsNullOrEmpty())
            {
                return true;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                _logger.LogWarning("{EndpointName} probe skipped because its configured URL is invalid.", endpointName);
                return false;
            }

            var firstAttemptActive = await activeProbeAsync(uri, url, cancellationToken).ConfigureAwait(false);

            if (firstAttemptActive)
            {
                return true;
            }

            _logger.LogWarning("{EndpointName} probe first attempt failed. Retrying once before marking endpoint inactive.", endpointName);

            var secondAttemptActive = await activeProbeAsync(uri, url, cancellationToken).ConfigureAwait(false);
            if (!secondAttemptActive)
            {
                _logger.LogWarning("{EndpointName} probe failed. Marking endpoint inactive and continuing heartbeat flow.", endpointName);
            }

            return secondAttemptActive;
        }

        private async Task<bool> InflightConnectivityActiveAsync(Uri uri, string url, CancellationToken cancellationToken = default)
        {
            var subscriptionKey = ConfigReader.Try(Environment.InflightConnectivityHealth);
            if (subscriptionKey.IsNullOrWhiteSpace())
            {
                _logger.LogWarning("Inflight connectivity probe cannot run because {SettingName} is not configured. Marking endpoint inactive.", Environment.InflightConnectivityHealth);

                return false;
            }

            try
            {
                using var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);
                headRequest.Headers.TryAddWithoutValidation(Constants.Application.OcpApimSubscriptionKeyHeaderName, subscriptionKey);
                using var response = await HttpRequestFallbackHelper.SendWithHeadFallbackToGetAsync(_httpConnector, headRequest, uri, cancellationToken).ConfigureAwait(false);

                return IsActiveStatusCode(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Inflight connectivity probe failed.");
                return false;
            }
        }

        private async Task<bool> EndpointActiveAsync(Uri uri, string url, CancellationToken cancellationToken = default)
        {
            try
            {
                using var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);
                using var response = await HttpRequestFallbackHelper.SendWithHeadFallbackToGetAsync(_httpConnector, headRequest, uri, cancellationToken).ConfigureAwait(false);

                return IsActiveStatusCode(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Endpoint probe failed.");
                return false;
            }
        }

        private async Task<bool> CaptivePortalActiveAsync(Uri uri, string url, CancellationToken cancellationToken = default)
        {
            var expectedBackendHeaderValue = ConfigReader.Try(Environment.CaptivePortalExpectedBackendHeaderValue);
            if (expectedBackendHeaderValue.IsNullOrWhiteSpace())
            {
                _logger.LogWarning("Captive portal probe cannot run because {SettingName} is not configured. Marking endpoint inactive.", Environment.CaptivePortalExpectedBackendHeaderValue);

                return false;
            }

            try
            {
                using var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);
                using var response = await HttpRequestFallbackHelper.SendWithHeadFallbackToGetAsync(_httpConnector, headRequest, uri, cancellationToken).ConfigureAwait(false);

                if (!IsActiveStatusCode(response))
                {
                    return false;
                }

                return HasExpectedCaptivePortalBackendHeader(response, url, expectedBackendHeaderValue);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Captive portal probe failed.");
                return false;
            }
        }

        private bool HasExpectedCaptivePortalBackendHeader(HttpResponseMessage response, string probeUrl, string expectedHeaderValue)
        {
            if (!response.Headers.TryGetValues(Constants.Application.CaptivePortalBackendHeaderName, out var backendHeaderValues))
            {
                _logger.LogWarning("Captive portal probe response is missing the required {HeaderName} header.", Constants.Application.CaptivePortalBackendHeaderName);

                return false;
            }

            var values = backendHeaderValues.ToArray();
            if (values.Any(x => x.Is(expectedHeaderValue)))
            {
                return true;
            }

            _logger.LogWarning("Captive portal probe response did not match the configured {HeaderName} header value.", Constants.Application.CaptivePortalBackendHeaderName);

            return false;
        }

        private static bool IsActiveStatusCode(HttpResponseMessage response)
            => response.IsSuccessStatusCode || (int)response.StatusCode is >= 300 and < 400;
    }
}
