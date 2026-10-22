using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Model.InRule;
using VAA.EAPI.NDC.Order.Infrastructure.Services.InRule;
using Xunit;

namespace VAA.EAPI.NDC.Order.Tests.Infrastructure.Services.InRule
{
    [Collection(InRuleConfigurationCollection.Name)]
    public class InRuleServiceTests : IClassFixture<HttpClientFactoryFixture>, IDisposable
    {
        private const string InRuleUrl = "https://Testresources.virginatlantic.com/dev/inrule/disruptionrules";
        private const string SuccessCorrelationId = "fef7a05d-1937-45dc-982d-4f5f6757338d";
        private const string UnavailableCorrelationId = "051ba42d-1d6b-4081-9de0-2c94eb1d756e";
        private const string TimeoutCorrelationId = "d936302c-35ae-4c64-a876-eac67a43f7cd";
        private const string MockRequestUrl = InRuleUrl;
        private const string InRuleSubscriptionKey = "test-inrule-subscription-key";
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly ServiceProvider _serviceProvider;
        private readonly InRuleService _service;

        public InRuleServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _httpClientFactoryFixture = httpClientFactoryFixture;
            _serviceProvider = CreateServiceProvider();
            _service = _serviceProvider.GetRequiredService<InRuleService>();
        }

        [Fact]
        public async Task GetDisruptionResponseAsync_UsesConfiguredEndpoint()
        {
            using var inRuleConfiguration = ConfigureMock();
            var request = CreateRequest();

            var responsesBefore = _httpClientFactoryFixture.Current.GetResponses(MockRequestUrl).Count;

            var response = await _service.GetDisruptionResponseAsync(request, CancellationToken.None);

            Assert.NotNull(response);
            Assert.Equal(request.Event.EventId, response.Event.EventId);
            Assert.Equal(request.Event.EventType, response.Event.EventType);
            Assert.Equal(request.CorrelationId, response.InRuleResponse.CorrelationId);
            Assert.Equal("Accept", response.InRuleResponse.Value);
            Assert.Equal("R5_SCH_Major", response.InRuleResponse.RuleId);
            Assert.Equal(request.OrderId, Assert.Single(response.OrderDetails).OrderId);
            Assert.Equal("SEG1", Assert.Single(response.OrderDetails).SegmentId);

            var serializedRequest = Json.Serialize(request);
            Assert.Contains("\"event\"", serializedRequest);
            Assert.Contains("\"order\"", serializedRequest);
            Assert.DoesNotContain("eventName", serializedRequest, StringComparison.OrdinalIgnoreCase);

            var responses = _httpClientFactoryFixture.Current.GetResponses(MockRequestUrl);
            Assert.Equal(responsesBefore + 1, responses.Count);
            var requestMessage = responses.Last().RequestMessage
                ?? throw new InvalidOperationException("The InRule response did not contain its request message.");
            var requestUri = requestMessage.RequestUri
                ?? throw new InvalidOperationException("The InRule request did not contain a request URI.");
            Assert.Equal(MockRequestUrl, requestUri.AbsoluteUri, ignoreCase: true);
            Assert.NotNull(requestMessage.Headers.Authorization);
            Assert.Equal("Bearer", requestMessage.Headers.Authorization.Scheme);
            Assert.Equal("test-oauth-token", requestMessage.Headers.Authorization.Parameter);
            Assert.True(requestMessage.Headers.TryGetValues("Ocp-Apim-Subscription-Key", out var subscriptionKeyValues));
            Assert.Equal(InRuleSubscriptionKey, Assert.Single(subscriptionKeyValues));
        }

        [Fact]
        public async Task GetDisruptionResponseAsync_ReturnsTheRulesEngineResponse()
        {
            using var inRuleConfiguration = ConfigureMock();
            var request = CreateRequest();

            var response = await _service.GetDisruptionResponseAsync(request, CancellationToken.None);

            var orderDetails = Assert.Single(response.OrderDetails);
            Assert.Equal("SEG1", orderDetails.SegmentId);
            Assert.Equal(request.OrderId, orderDetails.OrderId);
            Assert.Equal(request.BookingReference, orderDetails.Rloc);
        }

        [Fact]
        public async Task GetDisruptionResponseAsync_ReturnsNullWhenMockHasNoConfiguredResponse()
        {
            using var inRuleConfiguration = ConfigureMock();

            var response = await _service.GetDisruptionResponseAsync(CreateRequest(UnavailableCorrelationId), CancellationToken.None);

            Assert.Null(response);
        }

        [Fact]
        public async Task GetDisruptionResponseAsync_CancelsSlowLookupAtTheConfiguredBound()
        {
            using var inRuleConfiguration = ConfigureMock();
            using var timeoutServiceProvider = CreateServiceProvider(TimeSpan.FromMilliseconds(25));
            var service = timeoutServiceProvider.GetRequiredService<InRuleService>();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                service.GetDisruptionResponseAsync(CreateRequest(TimeoutCorrelationId), CancellationToken.None));
        }

        [Fact]
        public async Task GetDisruptionResponseAsync_CancelsStalledTokenAcquisitionAtTheConfiguredBound()
        {
            using var inRuleConfiguration = ConfigureMock();
            using var timeoutServiceProvider = CreateServiceProvider(
                TimeSpan.FromMilliseconds(25),
                new StalledOAuthTokenProvider());
            var service = timeoutServiceProvider.GetRequiredService<InRuleService>();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                service.GetDisruptionResponseAsync(CreateRequest(), CancellationToken.None));
        }

        private static InRuleConfigurationScope ConfigureMock(bool inRuleEnabled = true) =>
            InRuleConfigurationScope.Create(InRuleUrl, "api://inrule", InRuleSubscriptionKey, inRuleEnabled);

        private ServiceProvider CreateServiceProvider(
            TimeSpan? lookupTimeout = null,
            IOAuthTokenProvider oauthTokenProvider = null)
        {
            var services = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(_httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider>(oauthTokenProvider ?? new TestOAuthTokenProvider());

            if (lookupTimeout.HasValue)
            {
                services.AddSingleton<InRuleService>(serviceProvider => new InRuleService(
                    serviceProvider.GetRequiredService<IHttpConnector>(),
                    serviceProvider.GetRequiredService<IOAuthTokenProvider>(),
                    serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<InRuleService>>(),
                    lookupTimeout.Value));
            }
            else
            {
                services.AddSingleton<InRuleService>();
            }

            return services.BuildServiceProvider();
        }

        public void Dispose() => _serviceProvider.Dispose();

        private static DisruptionRequest CreateRequest(string correlationId = SuccessCorrelationId)
        {
            return new DisruptionRequest
            {
                Event = new Event
                {
                    EventId = $"OCN-{correlationId}",
                    EventType = "SCHEDULE_CHANGE",
                    EventVersion = "1",
                    Source = "SABRE_ENS",
                    CorrelationId = correlationId
                },
                Order =
                [
                    new OrderReference
                    {
                        OrderNumber = new VAA.EAPI.Domain.Virgin.Models.Order.OrderNumber { OrderId = "VS932OWWGYG26" },
                        BookingReference = new VAA.EAPI.Domain.Virgin.Models.Order.BookingReference { RLOC = "OWWGYG" }
                    }
                ]
            };
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public int AcquisitionCount { get; private set; }

            public Task<string> AcquireTokenAsync(string resource)
            {
                AcquisitionCount++;
                return Task.FromResult("test-oauth-token");
            }
        }

        private sealed class StalledOAuthTokenProvider : IOAuthTokenProvider
        {
            private readonly TaskCompletionSource<string> _token = new();

            public Task<string> AcquireTokenAsync(string resource) => _token.Task;
        }
    }
}
