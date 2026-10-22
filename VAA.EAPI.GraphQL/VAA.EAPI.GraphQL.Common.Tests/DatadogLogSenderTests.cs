using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Diagnostics.Logging.Datadog;
using Xunit;

namespace VAA.EAPI.GraphQL.Common.Tests;

public sealed class DatadogLogSenderTests
{
    [Fact]
    public void CreateHttpClientHandlerDisablesAutomaticRedirects()
    {
        using var handler = DatadogLogSender.CreateHttpClientHandler();

        Assert.False(handler.AllowAutoRedirect);
    }

    [Fact]
    public async Task StopAsyncDoesNotRetryPermanentClientFailures()
    {
        var queue = new DatadogLogQueue(CreateOptions());
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.BadRequest);
        using var sender = new DatadogLogSender(queue, CreateOptions(), handler);
        queue.TryWrite(CreateEntry());

        await SendQueuedEntryAsync(sender);

        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task StopAsyncDoesNotSendToAnUnapprovedEndpoint()
    {
        var options = CreateOptions(new Uri("https://intake.example.test/api/v2/logs"));
        var queue = new DatadogLogQueue(options);
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        using var sender = new DatadogLogSender(queue, options, handler);
        queue.TryWrite(CreateEntry());

        await SendQueuedEntryAsync(sender);

        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task StopAsyncRetriesTransientFailuresUntilDeliverySucceeds()
    {
        var queue = new DatadogLogQueue(CreateOptions());
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.InternalServerError, HttpStatusCode.OK);
        using var sender = new DatadogLogSender(queue, CreateOptions(), handler);
        queue.TryWrite(CreateEntry());

        await SendQueuedEntryAsync(sender);

        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task StopAsyncRetriesTransientFailuresThreeTimes()
    {
        var queue = new DatadogLogQueue(CreateOptions());
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.InternalServerError);
        using var sender = new DatadogLogSender(queue, CreateOptions(), handler);
        queue.TryWrite(CreateEntry());

        await SendQueuedEntryAsync(sender);

        Assert.Equal(3, handler.RequestCount);
    }

    [Fact]
    public async Task StopAsyncReplacesAnOversizedEntryWithASafeDiagnostic()
    {
        var options = CreateOptions();
        var queue = new DatadogLogQueue(options);
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        using var sender = new DatadogLogSender(queue, options, handler);
        queue.TryWrite(CreateEntry(new Dictionary<string, object> { ["oversized"] = new string('x', 4_600_000) }));

        await SendQueuedEntryAsync(sender);

        Assert.Equal(1, handler.RequestCount);
        Assert.Contains("datadog.payload.truncated", handler.RequestBody);
        Assert.DoesNotContain(new string('x', 1000), handler.RequestBody);
    }

    private static async Task SendQueuedEntryAsync(DatadogLogSender sender)
    {
        await sender.StartAsync(CancellationToken.None);
        using var shutdown = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await sender.StopAsync(shutdown.Token);
    }

    private static DatadogLoggingOptions CreateOptions(Uri? endpoint = null) => new()
    {
        Endpoint = endpoint ?? new Uri("https://http-intake.logs.datadoghq.com/api/v2/logs"),
        ApiKey = "synthetic-key",
        Service = "graphql-test-service",
        BatchSize = 1,
        FlushInterval = TimeSpan.FromMilliseconds(100)
    };

    private static DatadogLogEntry CreateEntry(IDictionary<string, object>? properties = null) => new()
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        Message = "test",
        Status = "info",
        Service = "graphql-test-service",
        Source = "csharp",
        Properties = properties ?? new Dictionary<string, object>()
    };

    // The sender owns a direct gzip transport rather than IHttpConnector, so this focused stub
    // is used to capture that transport boundary without introducing a live network dependency.
    private sealed class RecordingHttpMessageHandler(params HttpStatusCode[] statusCodes) : HttpMessageHandler
    {
        private readonly Queue<HttpStatusCode> responses = new(statusCodes);
        private readonly HttpStatusCode fallbackStatusCode = statusCodes.Length > 0 ? statusCodes[^1] : HttpStatusCode.OK;

        public int RequestCount { get; private set; }
        public string RequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;

            if (request.Content is not null)
            {
                await using var compressed = await request.Content.ReadAsStreamAsync(cancellationToken);
                await using var gzip = new GZipStream(compressed, CompressionMode.Decompress);
                using var reader = new StreamReader(gzip, Encoding.UTF8);
                RequestBody = await reader.ReadToEndAsync(cancellationToken);
            }

            var statusCode = responses.Count > 0 ? responses.Dequeue() : fallbackStatusCode;
            return new HttpResponseMessage(statusCode);
        }
    }
}
