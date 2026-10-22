using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Diagnostics.Logging.Datadog;
using VAA.EAPI.GraphQL.Common.Diagnostics.Telemetry;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using Xunit;

namespace VAA.EAPI.GraphQL.Common.Tests;

public sealed class DatadogCustomEventSinkTests
{
    [Fact]
    public void TrackEventForwardsOnlyTheFixedEventEnvelopeAndCorrelation()
    {
        var context = new AsyncContext();
        context.SetSessionId(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        context.SetOperationId(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        context.SetCorrelationId(Guid.Parse("33333333-3333-3333-3333-333333333333"));
        context.SetChannel(["graphql"]);
        context.SetTraceHeaders();
        AsyncContext.Current = context;

        using var activity = new Activity("DatadogCustomEventSinkTests").SetIdFormat(ActivityIdFormat.W3C).Start();
        var queue = new DatadogLogQueue(CreateOptions());
        var sink = new DatadogCustomEventSink(queue, CreateOptions());

        try
        {
            sink.TrackEvent("CHECK_IN_COMPLETED", new Dictionary<string, string>
            {
                ["Id"] = "event-id",
                ["Source"] = "GraphQL.Tests",
                ["TimestampUtc"] = "2026-09-17T12:00:00.0000000Z",
                ["Severity"] = "Information",
                ["PnrLocator"] = "not-for-datadog",
                ["PassengerId"] = "not-for-datadog",
                ["BoardingPasses"] = "not-for-datadog"
            });

            Assert.True(queue.Reader.TryRead(out var entry));
            Assert.Equal("CHECK_IN_COMPLETED", entry.Message);
            Assert.Equal("event-id", entry.Properties["event.id"]);
            Assert.Equal("GraphQL.Tests", entry.Properties["event.origin"]);
            Assert.Equal("2026-09-17T12:00:00.0000000Z", entry.Properties["event.timestamp"]);
            Assert.Equal("Information", entry.Properties["event.severity"]);
            Assert.Equal("33333333333333333333333333333333", entry.Properties[AsyncContext.APICorrelationIdHeader]);
            Assert.Equal(activity.TraceId.ToHexString(), entry.Properties["w3c.trace_id"]);
            Assert.DoesNotContain(entry.Properties.Keys, key => key.Contains("Pnr", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(entry.Properties.Keys, key => key.Contains("Passenger", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(entry.Properties.Keys, key => key.Contains("Boarding", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            AsyncContext.Current = null;
        }
    }

    [Theory]
    [InlineData("https://http-intake.logs.datadoghq.com/api/v2/logs", true)]
    [InlineData("https://http-intake.logs.uk1.datadoghq.com/api/v2/logs", true)]
    [InlineData("https://http-intake.logs.datadoghq.com/api/v1/input", false)]
    [InlineData("https://http-intake.logs.datadoghq.com/api/v2/logs?redirect=example.test", false)]
    [InlineData("https://intake.example.test/api/v2/logs", false)]
    [InlineData("http://http-intake.logs.datadoghq.com/api/v2/logs", false)]
    public void IsConfiguredRequiresAnApprovedIntakeEndpoint(string endpoint, bool expected)
    {
        var options = CreateOptions(new Uri(endpoint));

        Assert.Equal(expected, options.IsConfigured);
    }

    [Fact]
    public void AddDefaultServicesRegistersTheDatadogSinkOnlyWithValidConfiguration()
    {
        ConfigureDatadog("Datadog", "https://http-intake.logs.datadoghq.com/api/v2/logs");
        using var validProvider = new ServiceCollection()
            .AddLogging()
            .AddDefaultServices()
            .BuildServiceProvider();

        Assert.IsType<CustomEventTracker>(validProvider.GetRequiredService<ICustomEventTracker>());
        Assert.IsType<DatadogCustomEventSink>(Assert.Single(validProvider.GetServices<ICustomEventSink>()));

        ConfigureDatadog("Datadog", "https://intake.example.test/api/v2/logs");
        using var invalidProvider = new ServiceCollection()
            .AddLogging()
            .AddDefaultServices()
            .BuildServiceProvider();

        Assert.IsType<CustomEventTracker>(invalidProvider.GetRequiredService<ICustomEventTracker>());
        Assert.Empty(invalidProvider.GetServices<ICustomEventSink>());
    }

    [Fact]
    public void DatadogLogQueueDropsWithoutBlockingWhenFull()
    {
        var queue = new DatadogLogQueue(CreateOptions(queueCapacity: 1));

        Assert.True(queue.TryWrite(CreateEntry()));
        Assert.False(queue.TryWrite(CreateEntry()));
    }

    [Fact]
    public void TrackEventDropsInformationalEventsBelowTheConfiguredMinimumLevel()
    {
        var queue = new DatadogLogQueue(CreateOptions(minimumLevel: LogLevel.Warning));
        var sink = new DatadogCustomEventSink(queue, CreateOptions(minimumLevel: LogLevel.Warning));

        sink.TrackEvent("CHECK_IN_COMPLETED", new Dictionary<string, string>());

        Assert.False(queue.Reader.TryRead(out _));
    }

    private static DatadogLoggingOptions CreateOptions(Uri? endpoint = null, int queueCapacity = 100, LogLevel minimumLevel = LogLevel.Information) => new()
    {
        Endpoint = endpoint ?? new Uri("https://http-intake.logs.datadoghq.com/api/v2/logs"),
        ApiKey = "synthetic-key",
        Service = "graphql-test-service",
        QueueCapacity = queueCapacity,
        MinimumLevel = minimumLevel
    };

    private static DatadogLogEntry CreateEntry() => new()
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        Message = "test",
        Status = "info",
        Service = "graphql-test-service",
        Source = "csharp",
        Properties = new Dictionary<string, object>()
    };

    private static void ConfigureDatadog(string loggingProvider, string endpoint)
    {
        ConfigReader.Set("LoggingProvider", loggingProvider);
        ConfigReader.Set("DatadogLogsEndpoint", endpoint);
        ConfigReader.Set("DatadogApiKey", "synthetic-key");
        ConfigReader.Set("DatadogService", "graphql-test-service");
    }
}
