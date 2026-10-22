using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Client;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.Disruption;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Tests
{
    public sealed class InRuleTests : IClassFixture<HttpClientFactoryFixture>
    {
        private const string InRuleUrl = "https://inrule.example/api/DisruptionRules/decision/Disruptions";
        private static readonly Guid InRuleRequestId = Guid.Parse("8c046aaf-4112-49f7-bb32-c6e9125b5784");
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly IHttpConnector _httpConnector;

        public InRuleTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _httpClientFactoryFixture = httpClientFactoryFixture;
            _httpConnector = new HttpConnector(NullLogger<HttpConnector>.Instance, httpClientFactoryFixture.Current);
            ConfigReader.Set("InRuleDisruptionAPIURL", InRuleUrl);
            ConfigReader.Set("InRuleDisruptionAPIKey", "test-api-key");
        }

        [Fact]
        public void DeserializeRequest_MapsRequiredFieldsAndRetainsOptionalBaggageAllowances()
        {
            const string requestBody = "{\"event\":{\"eventId\":\"ENS-12345\",\"eventType\":\"SCHEDULE_CHANGE\",\"eventVersion\":\"1\",\"source\":\"SABRE_ENS\",\"occurredAt\":\"2026-09-14T12:00:00Z\",\"correlationId\":\"disruption-123\"},\"order\":[{\"DataList\":{\"SegmentDetails\":[{\"SegmentId\":\"SEG1\",\"ScheduleChangeIndicator\":true}],\"BaggageAllowances\":[{\"BaggageAllowanceId\":\"BAG1\"}]},\"OrderNumber\":{\"OrderId\":\"VS9320OWWGYG26\"},\"BookingReference\":{\"RLOC\":\"OWWGYG\"},\"ChangeType\":\"OrderItemAdded\"}]}";

            var request = Json.Deserialize<InRuleEvaluationRequest>(requestBody);

            Assert.Equal("ENS-12345", request.Event.EventId);
            Assert.Equal<DateTime?>(new DateTime(2026, 9, 14, 12, 0, 0, DateTimeKind.Utc), request.Event.OccurredAt);
            var order = Assert.Single(request.Order);
            Assert.Equal("VS9320OWWGYG26", order.OrderNumber.OrderId);
            Assert.Equal("OWWGYG", order.BookingReference.RLOC);
            Assert.True(Assert.Single(order.DataList.SegmentDetails).ScheduleChangeIndicator);
            Assert.NotNull(order.DataList.BaggageAllowances);
        }

        [Fact]
        public async Task EvaluateAsync_ForwardsPayloadAndMapsInRuleResponse()
        {
            const string requestBody = "{\"event\":{\"correlationId\":\"disruption-123\"},\"order\":[{\"BookingReference\":{\"RLOC\":\"ABC123\"}}]}";
            var client = CreateDisruptionInRuleService(_httpConnector);

            var response = await client.EvaluateAsync(requestBody, CancellationToken.None);

            Assert.Equal("Accept,Reshop,Cancel", response.Value);
            Assert.Equal("R5_SCH_Major", response.RuleId);
            Assert.Equal("disruption-123", response.CorrelationId);
        }

        [Fact]
        public async Task EvaluateAsync_SendsAsyncContextCorrelationIdAsInRuleRequestId()
        {
            var handler = new CapturingResponseHandler();
            var connector = new HttpConnector(
                NullLogger<HttpConnector>.Instance,
                new SingleHttpClientFactory(handler));
            var client = CreateDisruptionInRuleService(connector);

            await client.EvaluateAsync("{\"event\":{},\"order\":[]}", CancellationToken.None);

            Assert.Equal(InRuleRequestId.ToString(), handler.InRuleRequestId);
        }

        [Fact]
        public async Task EvaluateAsync_WhenRequestContainsTimestamp_ReturnsInRuleResponse()
        {
            const string requestBody = "{\"event\":{\"occurredAt\":\"2026-09-14T12:00:00+05:30\"},\"order\":[]}";
            var client = CreateDisruptionInRuleService(_httpConnector);

            var response = await client.EvaluateAsync(requestBody, CancellationToken.None);

            Assert.NotNull(response);
        }

        [Fact]
        public void SerializeInRuleRequest_PreservesTheRawRequestBody()
        {
            const string requestBody = "{\"event\":{\"occurredAt\":\"2026-09-14T12:00:00+05:30\"},\"order\":[]}";

            var payload = Json.Serialize(new InRuleRequest
            {
                Request = new Newtonsoft.Json.Linq.JRaw(requestBody)
            });

            Assert.Equal(string.Concat("{\"request\":", requestBody, "}"), payload);
        }

        [Fact]
        public async Task EvaluateAsync_WhenInRuleFails_ThrowsHttpRequestException()
        {
            const string requestBody = "{\"event\":{\"correlationId\":\"failure\"},\"order\":[]}";
            var client = CreateDisruptionInRuleService(_httpConnector);

            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => client.EvaluateAsync(requestBody, CancellationToken.None));

            Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        }

        [Theory]
        [InlineData("request-empty")]
        [InlineData("request-null")]
        [InlineData("request-malformed")]
        [InlineData("request-missing-output")]
        public async Task EvaluateAsync_WhenInRuleReturnsInvalidResponse_ThrowsBadGateway(string responseScenario)
        {
            var requestBody = string.Concat("{\"event\":{\"correlationId\":\"", responseScenario, "\"},\"order\":[]}");
            var client = CreateDisruptionInRuleService(_httpConnector);

            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => client.EvaluateAsync(requestBody, CancellationToken.None));

            Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        }

        [Fact]
        public async Task EvaluateAsync_WhenResponseStalls_UsesConfiguredTimeout()
        {
            const string timeoutSetting = "HttpClientTimeOutInMilliSeconds";
            ConfigReader.Set(timeoutSetting, "10");
            var connector = new HttpConnector(
                NullLogger<HttpConnector>.Instance,
                new StallingHttpClientFactory());
            var client = CreateDisruptionInRuleService(connector);

            try
            {
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.EvaluateAsync("{\"event\":{},\"order\":[]}", CancellationToken.None));
            }
            finally
            {
                ConfigReader.Set(timeoutSetting, "100000");
            }
        }

        [Fact]
        public async Task Handle_WhenInRuleSucceeds_ReturnsMappedResponse()
        {
            var expectedResponse = new InRuleResponse
            {
                CorrelationId = "disruption-123",
                RuleId = "schedule-change-rule"
            };
            var service = new DisruptionRulesClient(
                new StaticDisruptionInRuleService(expectedResponse),
                NullLogger<DisruptionRulesClient>.Instance);
            const string requestBody = "{\"event\":{\"eventId\":\"event-123\"},\"order\":[]}";

            var result = await service.EvaluateAsync(CreateDisruptionRequest(requestBody), requestBody, CancellationToken.None);

            Assert.Equal(Status.OK, result.Status);
            Assert.Equal("event-123", result.Value.Event.EventId);
            Assert.Equal(expectedResponse, result.Value.InRuleResponse);
        }

        [Fact]
        public async Task Handle_WhenRequestContainsChangedAndUnchangedSegments_ReturnsEventAndAllOrderDetails()
        {
            const string requestBody = "{\"event\":{\"eventId\":\"event-123\"},\"order\":[{\"OrderNumber\":{\"OrderId\":\"order-123\"},\"BookingReference\":{\"RLOC\":\"pnr-456\"},\"DataList\":{\"SegmentDetails\":[{\"SegmentId\":\"segment-changed\",\"ScheduleChangeIndicator\":true},{\"SegmentId\":\"segment-unchanged\",\"ScheduleChangeIndicator\":false}]}}]}";
            var service = new DisruptionRulesClient(
                new StaticDisruptionInRuleService(CreateInRuleResponse()),
                NullLogger<DisruptionRulesClient>.Instance);

            var result = await service.EvaluateAsync(CreateDisruptionRequest(requestBody), requestBody, CancellationToken.None);

            Assert.Equal(Status.OK, result.Status);
            Assert.Equal("event-123", result.Value.Event.EventId);
            Assert.Collection(result.Value.OrderDetails,
                orderDetail =>
                {
                    Assert.Equal("order-123", orderDetail.OrderId);
                    Assert.Equal("pnr-456", orderDetail.Rloc);
                    Assert.Equal("segment-changed", orderDetail.SegmentId);
                },
                orderDetail =>
                {
                    Assert.Equal("order-123", orderDetail.OrderId);
                    Assert.Equal("pnr-456", orderDetail.Rloc);
                    Assert.Equal("segment-unchanged", orderDetail.SegmentId);
                });
        }

        [Fact]
        public async Task Handle_WhenRequestOrderIsArray_ReturnsAllOrderDetails()
        {
            const string requestBody = "{\"event\":{\"eventId\":\"event-123\"},\"order\":[{\"OrderNumber\":{\"OrderId\":\"order-123\"},\"BookingReference\":{\"RLOC\":\"pnr-456\"},\"DataList\":{\"SegmentDetails\":[{\"SegmentId\":\"segment-changed\",\"ScheduleChangeIndicator\":true},{\"SegmentId\":\"segment-unchanged\",\"ScheduleChangeIndicator\":false}]}}]}";
            var service = new DisruptionRulesClient(
                new StaticDisruptionInRuleService(CreateInRuleResponse()),
                NullLogger<DisruptionRulesClient>.Instance);

            var result = await service.EvaluateAsync(CreateDisruptionRequest(requestBody), requestBody, CancellationToken.None);

            Assert.Equal(Status.OK, result.Status);
            Assert.Collection(result.Value.OrderDetails,
                orderDetail => Assert.Equal("segment-changed", orderDetail.SegmentId),
                orderDetail => Assert.Equal("segment-unchanged", orderDetail.SegmentId));
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest, Status.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized, Status.BadGateway)]
        [InlineData(HttpStatusCode.Forbidden, Status.BadGateway)]
        [InlineData(HttpStatusCode.NotFound, Status.BadGateway)]
        [InlineData(HttpStatusCode.RequestTimeout, Status.GatewayTimeout)]
        [InlineData(HttpStatusCode.TooManyRequests, Status.TooManyRequests)]
        [InlineData(HttpStatusCode.BadGateway, Status.BadGateway)]
        [InlineData(HttpStatusCode.ServiceUnavailable, Status.ServiceUnavailable)]
        [InlineData(HttpStatusCode.GatewayTimeout, Status.GatewayTimeout)]
        public async Task Handle_WhenInRuleReturnsHttpFailure_ReturnsMappedStatusWithSafeErrorDetails(HttpStatusCode inRuleStatus, Status expectedStatus)
        {
            var previousContext = AsyncContext.Current;
            AsyncContext.Current = CreateAsyncContext(InRuleRequestId);
            var service = new DisruptionRulesClient(
                new ThrowingDisruptionInRuleService(new HttpRequestException("InRule rejected the request.", null, inRuleStatus)),
                NullLogger<DisruptionRulesClient>.Instance);

            try
            {
                const string requestBody = "{\"event\":{\"correlationId\":\"disruption-123\"},\"order\":[]}";
                var result = await service.EvaluateAsync(CreateDisruptionRequest(requestBody), requestBody, CancellationToken.None);

                Assert.Equal(expectedStatus, result.Status);
                var error = Assert.Single(result.Errors);
                Assert.Equal(InRuleRequestId.ToString(), error.Id);
                Assert.Equal("INRULE_EVALUATION_FAILED", error.Code);
                Assert.Equal("InRule disruption evaluation could not be completed.", error.Message);
            }
            finally
            {
                AsyncContext.Current = previousContext;
            }
        }

        [Fact]
        public async Task Handle_WhenInRuleThrowsUnexpectedError_ReturnsSafeErrorDetails()
        {
            var previousContext = AsyncContext.Current;
            AsyncContext.Current = CreateAsyncContext(InRuleRequestId);
            var service = new DisruptionRulesClient(
                new ThrowingDisruptionInRuleService(new InvalidOperationException("InRule disruption API configuration is missing.")),
                NullLogger<DisruptionRulesClient>.Instance);

            try
            {
                const string requestBody = "{\"event\":{\"correlationId\":\"disruption-123\"},\"order\":[]}";
                var result = await service.EvaluateAsync(CreateDisruptionRequest(requestBody), requestBody, CancellationToken.None);

                Assert.Equal(Status.Error, result.Status);
                var error = Assert.Single(result.Errors);
                Assert.Equal(InRuleRequestId.ToString(), error.Id);
                Assert.Equal("INRULE_EVALUATION_FAILED", error.Code);
                Assert.Equal("InRule disruption evaluation could not be completed.", error.Message);
            }
            finally
            {
                AsyncContext.Current = previousContext;
            }
        }

        [Fact]
        public async Task Handle_WhenInRuleTransportFailureHasNoStatus_ReturnsBadGateway()
        {
            var previousContext = AsyncContext.Current;
            AsyncContext.Current = CreateAsyncContext(InRuleRequestId);
            var service = new DisruptionRulesClient(
                new ThrowingDisruptionInRuleService(new HttpRequestException("InRule connection failed.")),
                NullLogger<DisruptionRulesClient>.Instance);

            try
            {
                const string requestBody = "{\"event\":{},\"order\":[]}";
                var result = await service.EvaluateAsync(CreateDisruptionRequest(requestBody), requestBody, CancellationToken.None);

                Assert.Equal(Status.BadGateway, result.Status);
            }
            finally
            {
                AsyncContext.Current = previousContext;
            }
        }

        [Fact]
        public async Task Handle_WhenInRuleTimesOut_ReturnsGatewayTimeout()
        {
            var previousContext = AsyncContext.Current;
            AsyncContext.Current = CreateAsyncContext(InRuleRequestId);
            var service = new DisruptionRulesClient(
                new ThrowingDisruptionInRuleService(new TaskCanceledException("InRule request timed out.")),
                NullLogger<DisruptionRulesClient>.Instance);

            try
            {
                const string requestBody = "{\"event\":{},\"order\":[]}";
                var result = await service.EvaluateAsync(CreateDisruptionRequest(requestBody), requestBody, CancellationToken.None);

                Assert.Equal(Status.GatewayTimeout, result.Status);
            }
            finally
            {
                AsyncContext.Current = previousContext;
            }
        }

        [Fact]
        public async Task Handle_WhenInvocationIsCancelled_RethrowsCancellation()
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var service = new DisruptionRulesClient(
                new ThrowingDisruptionInRuleService(new TaskCanceledException("Invocation cancelled.")),
                NullLogger<DisruptionRulesClient>.Instance);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.EvaluateAsync(
                CreateDisruptionRequest("{\"event\":{},\"order\":[]}"), "{\"event\":{},\"order\":[]}", cancellationTokenSource.Token));
        }

        [Fact]
        public void ServiceProvider_CanResolveDisruptionInRuleService()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IHttpClientFactory>(_httpClientFactoryFixture.Current);
            services.AddSingleton<IHttpConnector, HttpConnector>();
            services.AddSingleton<IDisruptionInRuleService, DisruptionInRuleService>();
            services.AddSingleton<IDisruptionRulesClient, DisruptionRulesClient>();

            using var serviceProvider = services.BuildServiceProvider();

            Assert.IsType<DisruptionInRuleService>(serviceProvider.GetRequiredService<IDisruptionInRuleService>());
            Assert.IsType<DisruptionRulesClient>(serviceProvider.GetRequiredService<IDisruptionRulesClient>());
        }

        [Fact]
        public async Task EvaluateDisruptionRules_WithoutRequestId_ReturnsOk()
        {
            var controller = new DisruptionRulesController(CreateDisruptionRulesClient(), NullLogger<DisruptionRulesController>.Instance);
            HttpRequestData request = CreateRequest("{\"event\":{},\"order\":[]}", cancellationToken: TestContext.Current.CancellationToken);

            var response = await controller.EvaluateDisruptionRules(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void EvaluateDisruptionRules_UsesDisruptionRulesRoute()
        {
            var parameter = typeof(DisruptionRulesController)
                .GetMethod(nameof(DisruptionRulesController.EvaluateDisruptionRules))
                .GetParameters()
                .Single();
            var trigger = Assert.IsType<HttpTriggerAttribute>(parameter.GetCustomAttributes(typeof(HttpTriggerAttribute), false).Single());

            Assert.Equal("disruptionrules", trigger.Route);
        }

        [Fact]
        public async Task EvaluateDisruptionRules_WhenClientThrowsUnexpectedError_ReturnsInternalServerError()
        {
            var previousContext = AsyncContext.Current;
            AsyncContext.Current = CreateAsyncContext(Guid.NewGuid());

            try
            {
                var controller = new DisruptionRulesController(new ThrowingDisruptionRulesClient(), NullLogger<DisruptionRulesController>.Instance);
                HttpRequestData request = CreateRequest("{\"event\":{},\"order\":[]}", cancellationToken: TestContext.Current.CancellationToken);

                var response = await controller.EvaluateDisruptionRules(request);

                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            }
            finally
            {
                AsyncContext.Current = previousContext;
            }
        }

        [Fact]
        public async Task EvaluateDisruptionRules_WhenClientThrowsUnexpectedError_UsesCurrentCorrelationId()
        {
            var expectedCorrelationId = Guid.Parse("f3017ee9-5cdd-475d-90af-1a31cb2c27b4");
            var previousContext = AsyncContext.Current;
            AsyncContext.Current = CreateAsyncContext(expectedCorrelationId);

            try
            {
                var controller = new DisruptionRulesController(new ThrowingDisruptionRulesClient(), NullLogger<DisruptionRulesController>.Instance);
                HttpRequestData request = CreateRequest("{\"event\":{},\"order\":[]}", cancellationToken: TestContext.Current.CancellationToken);

                var response = await controller.EvaluateDisruptionRules(request);
                var error = Assert.Single(await ReadErrorsAsync(response));

                Assert.Equal(expectedCorrelationId.ToString(), error.Id);
            }
            finally
            {
                AsyncContext.Current = previousContext;
            }
        }

        [Theory]
        [InlineData("{}")]
        [InlineData("{\"request\":{\"event\":{},\"order\":[]}}")]
        [InlineData("{\"event\":{}}")]
        [InlineData("{\"event\":{},\"order\":[null]}")]
        [InlineData("{\"event\":{},\"order\":[{\"DataList\":{\"SegmentDetails\":[null]}}]}")]
        public async Task EvaluateDisruptionRules_WhenRequestBodyIsInvalid_ReturnsBadRequestWithoutCallingService(string requestBody)
        {
            var controller = new DisruptionRulesController(CreateDisruptionRulesClient(), NullLogger<DisruptionRulesController>.Instance);
            HttpRequestData request = CreateRequest(requestBody, TestContext.Current.CancellationToken);

            var response = await controller.EvaluateDisruptionRules(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var error = Assert.Single(await ReadErrorsAsync(response));
            Assert.Equal("INRULE_REQUEST_INVALID", error.Code);
        }

        [Theory]
        [InlineData("{\"event\":")]
        [InlineData("{\"event\":{},\"order\":\"invalid\"}")]
        [InlineData("{\"event\":{},\"order\":{}}")]
        [InlineData("{\"event\":{},\"order\":[{\"OrderNumber\":{}},\"invalid\"]}")]
        public async Task EvaluateDisruptionRules_WhenRequestBodyCannotBeDeserialized_ReturnsInternalServerError(string requestBody)
        {
            var previousContext = AsyncContext.Current;
            AsyncContext.Current = CreateAsyncContext(Guid.NewGuid());

            try
            {
                var controller = new DisruptionRulesController(CreateDisruptionRulesClient(), NullLogger<DisruptionRulesController>.Instance);
                HttpRequestData request = CreateRequest(requestBody, TestContext.Current.CancellationToken);

                var response = await controller.EvaluateDisruptionRules(request);

                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            }
            finally
            {
                AsyncContext.Current = previousContext;
            }
        }

        [Fact]
        public async Task EvaluateDisruptionRules_WhenContentTypeIsNotJson_ReturnsUnsupportedMediaTypeWithoutCallingService()
        {
            var controller = new DisruptionRulesController(CreateDisruptionRulesClient(), NullLogger<DisruptionRulesController>.Instance);
            HttpRequestData request = CreateRequest("{\"event\":{},\"order\":[]}", TestContext.Current.CancellationToken, "text/plain");

            var response = await controller.EvaluateDisruptionRules(request);

            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }

        [Fact]
        public async Task EvaluateDisruptionRules_PassesInvocationCancellationTokenToService()
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            var service = new CapturingDisruptionRulesClient();
            var controller = new DisruptionRulesController(service, NullLogger<DisruptionRulesController>.Instance);
            HttpRequestData request = CreateRequest("{\"event\":{},\"order\":[]}", cancellationTokenSource.Token);

            var response = await controller.EvaluateDisruptionRules(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(cancellationTokenSource.Token, service.CancellationToken);
        }

        [Theory]
        [InlineData(Status.TooManyRequests, HttpStatusCode.TooManyRequests)]
        [InlineData(Status.BadGateway, HttpStatusCode.BadGateway)]
        [InlineData(Status.ServiceUnavailable, HttpStatusCode.ServiceUnavailable)]
        [InlineData(Status.GatewayTimeout, HttpStatusCode.GatewayTimeout)]
        public async Task ExecuteResultAsync_WhenInRuleStatusIsMapped_ReturnsMatchingHttpStatus(Status responseStatus, HttpStatusCode expectedHttpStatus)
        {
            HttpRequestData request = CreateRequest(string.Empty, cancellationToken: TestContext.Current.CancellationToken);

            var response = await ResultExtention<string>.ExecuteResultAsync(request, Result.Success(new Response<string>
            {
                Status = responseStatus
            }));

            Assert.Equal(expectedHttpStatus, response.StatusCode);
        }

        private static InRuleResponse CreateInRuleResponse() => new()
        {
            CorrelationId = "disruption-123"
        };

        private static InRuleEvaluationRequest CreateDisruptionRequest(string requestBody) => Json.Deserialize<InRuleEvaluationRequest>(requestBody);

        private static AsyncContext CreateAsyncContext(Guid correlationId)
        {
            var asyncContext = new AsyncContext { Context = new TestFunctionContext() };
            asyncContext.SetCorrelationId(correlationId);

            return asyncContext;
        }

        private static DisruptionInRuleService CreateDisruptionInRuleService(IHttpConnector httpConnector)
        {
            AsyncContext.Current = CreateAsyncContext(InRuleRequestId);

            return new DisruptionInRuleService(httpConnector);
        }

        private static IDisruptionRulesClient CreateDisruptionRulesClient() => new CapturingDisruptionRulesClient();

        private static HttpRequestData CreateRequest(string requestBody, CancellationToken cancellationToken = default, string? contentType = "application/json")
        {
            var headers = new Dictionary<string, string>();

            if (contentType is not null)
            {
                headers.Add("Content-Type", contentType);
            }

            return new TestHttpRequestData(
                cancellationToken.CanBeCanceled ? new CancellationTestFunctionContext(cancellationToken) : new TestFunctionContext(),
                new Uri("https://tempuri.org/api/disruptionrules"),
                new MemoryStream(Encoding.UTF8.GetBytes(requestBody)),
                "POST",
                headers);
        }

        private static async Task<List<ErrorDetails>> ReadErrorsAsync(HttpResponseData response)
        {
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body);

            return Json.Deserialize<List<ErrorDetails>>(await reader.ReadToEndAsync());
        }

        private sealed class StaticDisruptionInRuleService : IDisruptionInRuleService
        {
            private readonly InRuleResponse _response;

            public StaticDisruptionInRuleService(InRuleResponse response)
            {
                _response = response;
            }

            public Task<InRuleResponse> EvaluateAsync(string requestBody, CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }

        private sealed class ThrowingDisruptionInRuleService : IDisruptionInRuleService
        {
            private readonly Exception _exception;

            public ThrowingDisruptionInRuleService(Exception exception)
            {
                _exception = exception;
            }

            public Task<InRuleResponse> EvaluateAsync(string requestBody, CancellationToken cancellationToken)
            {
                return Task.FromException<InRuleResponse>(_exception);
            }
        }

        private sealed class StallingHttpClientFactory : IHttpClientFactory
        {
            public HttpClient CreateClient(string name) => new(new StallingResponseHandler());
        }

        private sealed class SingleHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpMessageHandler _handler;

            public SingleHttpClientFactory(HttpMessageHandler handler)
            {
                _handler = handler;
            }

            public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
        }

        private sealed class CapturingResponseHandler : HttpMessageHandler
        {
            public string InRuleRequestId { get; private set; } = string.Empty;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                InRuleRequestId = request.Headers.TryGetValues("inrule-requestid", out var requestIds)
                    ? requestIds.Single()
                    : string.Empty;

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"output\":{\"correlationId\":\"disruption-123\"}}")
                });
            }
        }

        private sealed class StallingResponseHandler : HttpMessageHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        private sealed class CapturingDisruptionRulesClient : IDisruptionRulesClient
        {
            public CancellationToken CancellationToken { get; private set; }

            public Task<Response<RulesEvaluationResponse>> EvaluateAsync(InRuleEvaluationRequest request, string requestBody, CancellationToken cancellationToken)
            {
                CancellationToken = cancellationToken;

                return Task.FromResult(new Response<RulesEvaluationResponse>
                {
                    Status = Status.OK,
                    Value = new RulesEvaluationResponse()
                });
            }
        }

        private sealed class ThrowingDisruptionRulesClient : IDisruptionRulesClient
        {
            public Task<Response<RulesEvaluationResponse>> EvaluateAsync(InRuleEvaluationRequest request, string requestBody, CancellationToken cancellationToken)
            {
                return Task.FromException<Response<RulesEvaluationResponse>>(new InvalidOperationException("Unexpected error."));
            }
        }

        private sealed class CancellationTestFunctionContext : FunctionContext
        {
            private readonly TestFunctionContext _inner = new();
            private readonly CancellationToken _cancellationToken;

            public CancellationTestFunctionContext(CancellationToken cancellationToken)
            {
                _cancellationToken = cancellationToken;
            }

            public override string InvocationId => _inner.InvocationId;
            public override string FunctionId => _inner.FunctionId;
            public override TraceContext TraceContext => _inner.TraceContext;
            public override BindingContext BindingContext => _inner.BindingContext;
            public override RetryContext RetryContext => _inner.RetryContext;
            public override IServiceProvider InstanceServices { get => _inner.InstanceServices; set => _inner.InstanceServices = value; }
            public override FunctionDefinition FunctionDefinition => _inner.FunctionDefinition;
            public override IDictionary<object, object> Items { get => _inner.Items; set => _inner.Items = value; }
            public override IInvocationFeatures Features => _inner.Features;
            public override CancellationToken CancellationToken => _cancellationToken;
        }
    }
}
