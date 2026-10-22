using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Application;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Domain;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Interfaces;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Services;
using Environment = VAA.EAPI.Microservices.Inflight.ConnPulse.Constants.Environment;
using TestHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;
using Xunit;

namespace VAA.EAPI.Microservices.Inflight.ConnPulse.Tests
{
    public sealed class InflightConnectivityHealthCheckHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private const string CaptivePortalExpectedBackendHeaderValue = "starlink-portal";
        private const string CaptivePortalAsyncCheckHealthyUrl = "https://wifi.virginatlantic.com/";
        private const string CaptivePortalAsyncCheckUnhealthyUrl = "https://wifi-wrong.virginatlantic.com/";
        private const string InflightConnUrl = "https://inflightconn.test/heartbeat";
        private const string InflightConnectivityHealth = "inflightconn-subscription-key";
        private const string GraphQLUrl = "https://graphql.test/heartbeat";

        private readonly IServiceProvider _serviceProvider;

        public InflightConnectivityHealthCheckHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConnPulseTestsCommon.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<CapturingHeartbeatService>()
                .AddSingleton<IHeartbeatService>(provider => provider.GetRequiredService<CapturingHeartbeatService>())
                .AddSingleton<IInflightConnectivityHealthCheckHandler, InflightConnectivityHealthCheckHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public void Constructor_ThrowsWhenHttpConnectorIsNull()
        {
            var heartbeatService = _serviceProvider.GetRequiredService<IHeartbeatService>();
            var logger = _serviceProvider.GetRequiredService<ILogger<InflightConnectivityHealthCheckHandler>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new InflightConnectivityHealthCheckHandler(null!, heartbeatService, logger));

            Assert.Equal("httpConnector", exception.ParamName);
        }

        [Fact]
        public void Constructor_ThrowsWhenStarlinkServiceIsNull()
        {
            var httpConnector = _serviceProvider.GetRequiredService<IHttpConnector>();
            var logger = _serviceProvider.GetRequiredService<ILogger<InflightConnectivityHealthCheckHandler>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new InflightConnectivityHealthCheckHandler(httpConnector, null!, logger));

            Assert.Equal("starlinkService", exception.ParamName);
        }

        [Fact]
        public async Task HealthCheckAsync_AlwaysSendsHeartbeat()
        {
            ConfigureSkippedProbeUrls();

            var handler = ResolveHandler();

            await handler.HealthCheckAsync(CancellationToken.None);

            var heartbeatRequest = Assert.Single(_serviceProvider.GetRequiredService<CapturingHeartbeatService>().Requests);
            Assert.True(heartbeatRequest.Health);
        }

        [Fact]
        public async Task HealthCheckAsync_MarksHealthyWhenCaptivePortalBackendHeaderMatches()
        {
            ConfigureProbeUrls(CaptivePortalAsyncCheckHealthyUrl);

            var handler = ResolveHandler();
            var httpClientFactory = (TestHttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;

            await handler.HealthCheckAsync(CancellationToken.None);

            var heartbeatRequest = Assert.Single(_serviceProvider.GetRequiredService<CapturingHeartbeatService>().Requests);
            Assert.True(heartbeatRequest.Health);

            var portalResponse = Assert.Single(httpClientFactory.GetResponses(BuildCaptivePortalProbeUri(CaptivePortalAsyncCheckHealthyUrl)));
            Assert.Equal(HttpMethod.Head, portalResponse.RequestMessage!.Method);
            AssertProbeFallsBackToGet(httpClientFactory, InflightConnUrl, requiresSubscriptionKey: true);
            AssertProbeFallsBackToGet(httpClientFactory, GraphQLUrl);
        }

        [Fact]
        public async Task HealthCheckAsync_MarksUnhealthyWhenInflightConnectivitySubscriptionKeyIsMissing()
        {
            ConfigureSkippedProbeUrls();
            ConfigReader.Set(Environment.InflightConnUrl, InflightConnUrl);
            ConfigReader.Set(Environment.InflightConnectivityHealth, string.Empty);

            var handler = ResolveHandler();

            await handler.HealthCheckAsync(CancellationToken.None);

            var heartbeatRequest = Assert.Single(_serviceProvider.GetRequiredService<CapturingHeartbeatService>().Requests);
            Assert.False(heartbeatRequest.Health);
        }

        [Fact]
        public async Task HealthCheckAsync_MarksUnhealthyWhenCaptivePortalBackendHeaderDoesNotMatch()
        {
            ConfigureProbeUrls(CaptivePortalAsyncCheckUnhealthyUrl);

            var handler = ResolveHandler();
            var httpClientFactory = (TestHttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;

            await handler.HealthCheckAsync(CancellationToken.None);

            var heartbeatRequest = Assert.Single(_serviceProvider.GetRequiredService<CapturingHeartbeatService>().Requests);
            Assert.False(heartbeatRequest.Health);

            var portalRequests = httpClientFactory.GetResponses(BuildCaptivePortalProbeUri(CaptivePortalAsyncCheckUnhealthyUrl))
                .Select(response => response.RequestMessage!)
                .ToArray();

            Assert.Equal(2, portalRequests.Length);
            Assert.All(portalRequests, request => Assert.Equal(HttpMethod.Head, request.Method));
        }

        private IInflightConnectivityHealthCheckHandler ResolveHandler()
            => _serviceProvider.GetRequiredService<IInflightConnectivityHealthCheckHandler>();

        private static void ConfigureSkippedProbeUrls()
        {
            ConfigReader.Set(Environment.CaptivePortalAsyncCheckUrl, string.Empty);
            ConfigReader.Set(Environment.InflightConnUrl, string.Empty);
            ConfigReader.Set(Environment.GraphQLUrl, string.Empty);
        }

        private static void ConfigureProbeUrls(string captivePortalAsyncCheckUrl)
        {
            ConfigReader.Set(Environment.CaptivePortalAsyncCheckUrl, captivePortalAsyncCheckUrl);
            ConfigReader.Set(Environment.CaptivePortalExpectedBackendHeaderValue, CaptivePortalExpectedBackendHeaderValue);
            ConfigReader.Set(Environment.InflightConnUrl, InflightConnUrl);
            ConfigReader.Set(Environment.InflightConnectivityHealth, InflightConnectivityHealth);
            ConfigReader.Set(Environment.GraphQLUrl, GraphQLUrl);
        }

        private static string BuildCaptivePortalProbeUri(string captivePortalAsyncCheckUrl)
            => captivePortalAsyncCheckUrl;

        private static void AssertProbeFallsBackToGet(TestHttpClientFactory httpClientFactory, string probeUrl, bool requiresSubscriptionKey = false)
        {
            var requests = httpClientFactory.GetResponses(probeUrl)
                .Select(response => response.RequestMessage!)
                .TakeLast(2)
                .ToArray();

            Assert.Equal([HttpMethod.Head, HttpMethod.Get], requests.Select(request => request.Method));
            if (requiresSubscriptionKey)
            {
                Assert.All(
                    requests,
                    request => Assert.Equal(
                        InflightConnectivityHealth,
                        request.Headers.GetValues(Constants.Application.OcpApimSubscriptionKeyHeaderName).Single()));
            }
        }

        private sealed class CapturingHeartbeatService : IHeartbeatService
        {
            public List<StarlinkHeartbeatRequest> Requests { get; } = [];

            public Task SendHeartbeatAsync(StarlinkHeartbeatRequest request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                return Task.CompletedTask;
            }
        }
    }
}
