using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Diagnostics.Logging.Datadog;
using VAA.EAPI.Common.Diagnostics.Logging;
using VAA.EAPI.Common.Diagnostics.Telemetry;
using VAA.EAPI.Common.Diagnostics;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Http;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;

namespace VAA.EAPI.Microservices.Tests
{
    public sealed class AsyncContextTests
    {
        public AsyncContextTests()
        {
            var testFunctionContext = CreateFunctionContext(("api-channel", "api, mobile;key1=false;key2=test"));

            AsyncContext.Current = new AsyncContext { Context = testFunctionContext };
        }

        [Fact]
        public void Channel()
        {
            Assert.True(AsyncContext.Current.HasChannel("api"));
            Assert.True(AsyncContext.Current.HasChannel("mobile"));
            Assert.True(AsyncContext.Current.HasChannelKey("mobile", "key1"));
            Assert.True(AsyncContext.Current.HasChannelKey("mobile", "key2"));
            Assert.False(AsyncContext.Current.HasChannelKey("mobile", "key3"));
            Assert.Equal("false", AsyncContext.Current.GetChannelKey("mobile", "key1"));
            Assert.Equal("test", AsyncContext.Current.GetChannelKey("mobile", "key2"));
            Assert.Null(AsyncContext.Current.GetChannelKey("mobile", "key3"));
        }

        [Theory]
        [InlineData(LoggingProvider.Both, true, true)]
        [InlineData(LoggingProvider.AppInsights, true, false)]
        [InlineData(LoggingProvider.Datadog, false, true)]
        public void AddDefaultServices_RegistersOnlySelectedLoggingProviders(LoggingProvider loggingProvider, bool expectAppInsights, bool expectDatadog)
        {
            var services = new ServiceCollection();

            services.AddDefaultServices(loggingProvider);

            Assert.Equal(expectAppInsights, services.Any(descriptor => descriptor.ServiceType == typeof(ITelemetryInitializer)));
            Assert.Equal(expectDatadog, services.Any(descriptor => descriptor.ServiceType == typeof(DatadogLoggingOptions)));
            Assert.Equal(expectAppInsights, services.Any(descriptor => descriptor.ImplementationType == typeof(ApplicationInsightsCustomEventSink)));
            Assert.Equal(expectDatadog, services.Any(descriptor => descriptor.ImplementationType == typeof(DatadogCustomEventSink)));
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ICustomEventTracker));
        }

        [Fact]
        public void AddDefaultServices_DatadogOnly_ResolvesCustomEventTrackerWithoutApplicationInsights()
        {
            using var serviceProvider = new ServiceCollection()
                .AddDefaultServices(LoggingProvider.Datadog)
                .BuildServiceProvider();

            Assert.NotNull(serviceProvider.GetRequiredService<ICustomEventTracker>());
            Assert.Null(serviceProvider.GetService<TelemetryClient>());
        }

        [Theory]
        [InlineData("Both", LoggingProvider.Both)]
        [InlineData("appinsights", LoggingProvider.AppInsights)]
        [InlineData("DataDog", LoggingProvider.Datadog)]
        [InlineData("unsupported", LoggingProvider.AppInsights)]
        public void ParseLoggingProvider_ReturnsConfiguredProviderOrAppInsights(string value, LoggingProvider expected)
        {
            Assert.Equal(expected, ServiceCollectionExtension.ParseLoggingProvider(value));
        }

        [Fact]
        public async Task CreateAsync_CreatesChildContextAndPreservesTraceState()
        {
            const string parentTraceId = "4bf92f3577b34da6a3ce929d0e0e4736";
            var previousActivity = Activity.Current;
            Activity.Current = null;

            try
            {
                var functionContext = CreateFunctionContext(
                    ("traceparent", $"00-{parentTraceId}-00f067aa0ba902b7-01"),
                    ("tracestate", "vendor=value"));

                var context = await AsyncContext.CreateAsync(functionContext);

                Assert.Equal(parentTraceId, context.W3CTraceId);
                Assert.NotEqual("00f067aa0ba902b7", context.W3CSpanId);
                Assert.Equal("vendor=value", context.W3CTraceState);
                Assert.Equal(ActivityTraceFlags.Recorded, context.W3CTraceFlags);
                context.Complete();
            }
            finally
            {
                Activity.Current = previousActivity;
            }
        }

        [Fact]
        public async Task GetBaggage_ReturnsOnlyAllowedValues()
        {
            var context = await AsyncContext.CreateAsync(CreateFunctionContext(("baggage", "journey=refund,secret=do-not-log")));

            var baggage = context.GetBaggage(["journey"]);

            Assert.Equal("refund", baggage["journey"]);
            Assert.DoesNotContain("secret", baggage.Keys);
            context.Complete();
        }

        [Fact]
        public async Task CreateAsync_RejectsInvalidTraceStateAndNonAsciiBaggageKeys()
        {
            const string parentTraceId = "4bf92f3577b34da6a3ce929d0e0e4736";
            var context = await AsyncContext.CreateAsync(CreateFunctionContext(
                ("traceparent", $"00-{parentTraceId}-00f067aa0ba902b7-01"),
                ("tracestate", "Vendor=value"),
                ("baggage", "valid=ok,évil=value,bad/key=value,empty=")));

            var baggage = context.GetBaggage();

            Assert.Null(context.W3CTraceState);
            Assert.Equal(parentTraceId, context.W3CTraceId);
            Assert.NotEqual("00f067aa0ba902b7", context.W3CSpanId);
            Assert.Equal("ok", baggage["valid"]);
            Assert.Equal(string.Empty, baggage["empty"]);
            Assert.DoesNotContain("évil", baggage.Keys);
            Assert.DoesNotContain("bad/key", baggage.Keys);
            context.Complete();
        }

        [Fact]
        public async Task CreateAsync_RejectsBaggageThatDecodesToControlCharacters()
        {
            var context = await AsyncContext.CreateAsync(CreateFunctionContext(("baggage", "safe=value,encoded=%0D%0Ainjected")));

            var baggage = context.GetBaggage();

            Assert.Equal("value", baggage["safe"]);
            Assert.DoesNotContain("encoded", baggage.Keys);
            Assert.DoesNotContain(context.ExecutionActivity!.Baggage, item => item.Key == "encoded");
            context.Complete();
        }

        [Fact]
        public void SetTraceHeaders_UpdatesDerivedFieldsWithoutAmbientActivity()
        {
            var previousActivity = Activity.Current;
            Activity.Current = null;

            try
            {
                var context = new AsyncContext { Context = CreateFunctionContext() };
                context.SetOperationId(Guid.NewGuid());
                context.SetCorrelationId(Guid.NewGuid());

                context.SetTraceHeaders();

                Assert.True(ActivityContext.TryParse(context.W3CTraceParent, context.W3CTraceState, false, out var parsed));
                Assert.Equal(parsed.TraceId.ToHexString(), context.W3CTraceId);
                Assert.Equal(parsed.SpanId.ToHexString(), context.W3CSpanId);
                Assert.Equal(parsed.TraceFlags, context.W3CTraceFlags);
            }
            finally
            {
                Activity.Current = previousActivity;
            }
        }

        [Fact]
        public async Task MapFromCloudEvent_PreservesExistingContextWhenLegacyMetadataIsAbsent()
        {
            var context = await AsyncContext.CreateAsync(CreateFunctionContext());
            context.SetDiagnosticsEnabled(true);
            context.SetChannel(new[] { "WEB" });
            var sessionId = context.SessionId;
            var operationId = context.OperationId;
            var correlationId = context.CorrelationId;

            context.MapFromCloudEvent(new CloudEvent());

            Assert.True(context.DiagnosticsEnabled);
            Assert.Equal(new[] { "WEB" }, context.Channel);
            Assert.Equal(sessionId, context.SessionId);
            Assert.Equal(operationId, context.OperationId);
            Assert.Equal(correlationId, context.CorrelationId);

            context.Complete();
        }

        [Fact]
        public async Task MapFromCloudEvent_MapsPopulatedContextMetadata()
        {
            var context = await AsyncContext.CreateAsync(CreateFunctionContext());
            var cloudEvent = new CloudEvent
            {
                DiagnosticsEnabled = true,
                Channel = new[] { "WEB", "APP" },
                SessionId = Guid.NewGuid(),
                OperationId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid()
            };

            context.MapFromCloudEvent(cloudEvent);

            Assert.True(context.DiagnosticsEnabled);
            Assert.Equal(cloudEvent.Channel, context.Channel);
            Assert.Equal(cloudEvent.SessionId, context.SessionId);
            Assert.Equal(cloudEvent.OperationId, context.OperationId);
            Assert.Equal(cloudEvent.CorrelationId, context.CorrelationId);

            context.Complete();
        }

        [Fact]
        public async Task ServiceBusTraceContext_RoundTripsTraceAndSafeBaggage()
        {
            var publisher = await AsyncContext.CreateAsync(CreateFunctionContext(
                ("traceparent", "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"),
                ("tracestate", "vendor=value"),
                ("baggage", "api-channel=Bruno%2CAPI,secret=do-not-propagate")));
            var message = new ServiceBusMessage("{}").AddEapiTraceContext(publisher);
            var subscriber = await AsyncContext.CreateAsync(CreateFunctionContext());

            new Dictionary<string, object>(message.ApplicationProperties).RestoreEapiTraceContext(subscriber);

            Assert.Equal(publisher.W3CTraceId, subscriber.W3CTraceId);
            Assert.Equal("vendor=value", subscriber.W3CTraceState);
            Assert.Equal("API", subscriber.GetBaggage()[AsyncContext.APIChannelHeader]);
            Assert.DoesNotContain("secret", message.ApplicationProperties[AsyncContext.W3CBaggageHeader].ToString());

            publisher.Complete();
            subscriber.Complete();
        }

        [Fact]
        public async Task ServiceBusTraceContext_PreservesDiagnosticIdCasingAndIgnoresMalformedProperties()
        {
            var publisher = await AsyncContext.CreateAsync(CreateFunctionContext());
            var subscriber = await AsyncContext.CreateAsync(CreateFunctionContext());
            var applicationProperties = $$"""{ "Diagnostic-Id": "{{publisher.W3CTraceParent}}", "baggage": "api-channel=API,secret=do-not-propagate" }""";

            applicationProperties.RestoreEapiTraceContext(subscriber);

            Assert.Equal(publisher.W3CTraceId, subscriber.W3CTraceId);
            Assert.DoesNotContain("secret", subscriber.GetBaggage().Keys);
            Assert.Null(Record.Exception(() => "{ malformed".RestoreEapiTraceContext(subscriber)));

            publisher.Complete();
            subscriber.Complete();
        }

        [Fact]
        public async Task Middleware_RestoresPreviousContext()
        {
            var previous = new AsyncContext();
            AsyncContext.Current = previous;
            var middleware = new FunctionContextAccessorMiddleware();

            await middleware.Invoke(CreateFunctionContext(), context =>
            {
                Assert.NotNull(AsyncContext.Current);
                Assert.NotSame(previous, AsyncContext.Current);
                return Task.CompletedTask;
            });

            Assert.Same(previous, AsyncContext.Current);
        }

        [Fact]
        public async Task DatadogLogger_EnrichesLogFromExecutionContext()
        {
            var context = await AsyncContext.CreateAsync(CreateFunctionContext(("baggage", "journey=refund,secret=do-not-log")));
            AsyncContext.Current = context;
            var options = new DatadogLoggingOptions
            {
                Endpoint = new Uri("https://http-intake.logs.uk1.datadoghq.com/api/v2/logs"),
                ApiKey = "synthetic-key",
                Service = "test-service",
                BaggageAllowList = ["journey"]
            };
            var queue = new DatadogLogQueue(options);
            var logger = new DatadogLogger("Tests", queue, options, () => new LoggerExternalScopeProvider());

            logger.LogInformation("Processed {Reference}", "synthetic-reference");

            Assert.True(queue.Reader.TryRead(out var entry));
            Assert.Equal("info", entry.Status);
            Assert.Equal(context.W3CTraceId, entry.Properties["w3c.trace_id"]);
            Assert.Equal(Convert.ToUInt64(context.W3CTraceId[^16..], 16).ToString(), entry.Properties["dd.trace_id"]);
            Assert.Equal(Convert.ToUInt64(context.W3CSpanId, 16).ToString(), entry.Properties["dd.span_id"]);
            Assert.Equal("test-service", entry.Properties["dd.service"]);
            Assert.Equal("refund", entry.Properties["baggage.journey"]);
            Assert.DoesNotContain("baggage.secret", entry.Properties.Keys);
            Assert.Equal("synthetic-reference", entry.Properties["Reference"]);
            context.Complete();
        }

        [Fact]
        public async Task DatadogLogger_DoesNotCopyRawBaggageFromScopes()
        {
            var context = await AsyncContext.CreateAsync(CreateFunctionContext(("baggage", "journey=refund,secret=do-not-log")));
            AsyncContext.Current = context;
            var options = CreateDatadogOptions();
            var queue = new DatadogLogQueue(options);
            var scopeProvider = new LoggerExternalScopeProvider();
            var logger = new DatadogLogger("Tests", queue, options, () => scopeProvider);

            using (logger.BeginScope(new Dictionary<string, object> { [AsyncContext.W3CBaggageHeader] = context.W3CBaggage }))
                logger.LogInformation("Scoped log");

            Assert.True(queue.Reader.TryRead(out var entry));
            Assert.DoesNotContain(AsyncContext.W3CBaggageHeader, entry.Properties.Keys);
            Assert.Equal("refund", entry.Properties["baggage.journey"]);
            Assert.DoesNotContain("baggage.secret", entry.Properties.Keys);
            context.Complete();
        }

        [Fact]
        public async Task DatadogCustomEventSink_ForwardsSafeCustomEventWhenDatadogIsTheOnlyTelemetryDestination()
        {
            var context = await AsyncContext.CreateAsync(CreateFunctionContext());
            AsyncContext.Current = context;
            var options = CreateDatadogOptions();
            var queue = new DatadogLogQueue(options);
            var tracker = new DatadogCustomEventSink(queue, options);
            var eventProperties = new Dictionary<string, string>
            {
                ["Message"] = "Sign in started",
                ["Severity"] = "Information",
                ["Handler"] = "AccountSignInHandler",
                ["Email"] = "member@example.test",
                ["InitiateResponse"] = "sensitive-response"
            };

            tracker.TrackEvent("EntraDiags", eventProperties);

            Assert.True(queue.Reader.TryRead(out var entry));
            Assert.Equal("Sign in started", entry.Message);
            Assert.Equal("info", entry.Status);
            Assert.Equal("EntraDiags", entry.Properties["event.name"]);
            Assert.Equal("application.custom_event", entry.Properties["event.source"]);
            Assert.Equal("AccountSignInHandler", entry.Properties["event.handler"]);
            Assert.Equal(context.W3CTraceId, entry.Properties["w3c.trace_id"]);
            Assert.DoesNotContain("Email", entry.Properties.Keys);
            Assert.DoesNotContain("InitiateResponse", entry.Properties.Keys);
            context.Complete();
        }

        [Fact]
        public async Task SharedTelemetry_DoesNotExposeRawBaggage()
        {
            var context = await AsyncContext.CreateAsync(CreateFunctionContext(("baggage", "journey=refund,secret=do-not-log")));
            AsyncContext.Current = context;
            var telemetry = new TraceTelemetry();

            new DefaultTelemetryInitializer().Initialize(telemetry);

            Assert.DoesNotContain(AsyncContext.W3CBaggageHeader, telemetry.Properties.Keys);
            Assert.DoesNotContain(AsyncContext.W3CBaggageHeader, Trace<AsyncContextTests>.DefaultCustomProperties.Keys);
            context.Complete();
        }

        [Fact]
        public async Task DefaultDelegatingHandler_DoesNotReuseInvocationTraceParent()
        {
            var context = await AsyncContext.CreateAsync(CreateFunctionContext());
            AsyncContext.Current = context;
            var captureHandler = new CaptureHttpMessageHandler();
            using var handler = new DefaultDelegatingHandler(captureHandler, NullLogger.Instance);
            using var invoker = new HttpMessageInvoker(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://tempuri.org/test");

            using var response = await invoker.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.DoesNotContain(AsyncContext.W3cTraceContextHeader, captureHandler.HeaderNames);
            context.Complete();
        }

        [Fact]
        public async Task DatadogLogSender_DrainsAndTruncatesOversizedEntryDuringShutdown()
        {
            var options = CreateDatadogOptions(TimeSpan.FromSeconds(30));
            var queue = new DatadogLogQueue(options);
            var captureHandler = new CaptureHttpMessageHandler();
            using var sender = new DatadogLogSender(queue, options, captureHandler);
            queue.TryWrite(new DatadogLogEntry
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Message = "oversized",
                Status = "information",
                Service = options.Service,
                Source = options.Source,
                Properties = new Dictionary<string, object> { ["oversized"] = new string('x', 4_600_000) }
            });

            await sender.StartAsync(TestContext.Current.CancellationToken);
            using var shutdown = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var stopwatch = Stopwatch.StartNew();
            await sender.StopAsync(shutdown.Token);

            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5));
            Assert.Equal(1, captureHandler.RequestCount);
            Assert.Contains("datadog.payload.truncated", captureHandler.Body);
            Assert.DoesNotContain(new string('x', 1000), captureHandler.Body);
        }

        private static DatadogLoggingOptions CreateDatadogOptions(TimeSpan? flushInterval = null) => new()
        {
            Endpoint = new Uri("https://http-intake.logs.uk1.datadoghq.com/api/v2/logs"),
            ApiKey = "synthetic-key",
            Service = "test-service",
            BaggageAllowList = ["journey"],
            FlushInterval = flushInterval ?? TimeSpan.FromSeconds(2)
        };

        private static TestFunctionContext CreateFunctionContext(params (string Name, string Value)[] headers)
        {
            var functionContext = new TestFunctionContext();
            var request = functionContext.Features.Get<HttpRequestData>()!;

            foreach (var header in headers)
                request.Headers.TryAddWithoutValidation(header.Name, header.Value);

            return functionContext;
        }

        private sealed class CaptureHttpMessageHandler : HttpMessageHandler
        {
            public string Body { get; private set; } = string.Empty;
            public HashSet<string> HeaderNames { get; } = new(StringComparer.OrdinalIgnoreCase);
            public int RequestCount { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestCount++;
                HeaderNames.UnionWith(request.Headers.Select(header => header.Key));

                if (request.Content is not null)
                {
                    await using var compressed = await request.Content.ReadAsStreamAsync(cancellationToken);
                    await using var gzip = new GZipStream(compressed, CompressionMode.Decompress);
                    using var reader = new StreamReader(gzip, Encoding.UTF8);
                    Body = await reader.ReadToEndAsync(cancellationToken);
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

    }
}
