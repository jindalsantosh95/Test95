using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Stitch.Core.Diagnostics.Logging.Datadog
{
    internal sealed class DatadogLogSender : BackgroundService
    {
        private const int MaximumPayloadBytes = 4_500_000;
        private readonly DatadogLogQueue queue;
        private readonly DatadogLoggingOptions options;
        private readonly HttpClient httpClient;

        [ActivatorUtilitiesConstructor]
        public DatadogLogSender(DatadogLogQueue queue, DatadogLoggingOptions options)
            : this(queue, options, new HttpClientHandler())
        {
        }

        internal DatadogLogSender(DatadogLogQueue queue, DatadogLoggingOptions options, HttpMessageHandler handler)
        {
            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            httpClient = new HttpClient(handler ?? throw new ArgumentNullException(nameof(handler)), disposeHandler: true);
            httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!options.IsConfigured)
                return;

            while (!stoppingToken.IsCancellationRequested && await queue.Reader.WaitToReadAsync(stoppingToken).ConfigureAwait(false))
            {
                var batch = new List<DatadogLogEntry>(options.BatchSize);
                if (queue.Reader.TryRead(out var first))
                    batch.Add(first);

                while (batch.Count < options.BatchSize && queue.Reader.TryRead(out var next))
                    batch.Add(next);

                if (batch.Count < options.BatchSize && !queue.Reader.Completion.IsCompleted)
                {
                    var flushDelay = Task.Delay(options.FlushInterval, stoppingToken);
                    var moreDataOrCompletion = queue.Reader.WaitToReadAsync(stoppingToken).AsTask();
                    await Task.WhenAny(flushDelay, moreDataOrCompletion).ConfigureAwait(false);

                    while (batch.Count < options.BatchSize && queue.Reader.TryRead(out var next))
                        batch.Add(next);
                }

                if (batch.Count > 0)
                    await SendBatchAsync(batch, stoppingToken).ConfigureAwait(false);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            queue.Complete();

            if (ExecuteTask is not null)
                await ExecuteTask.WaitAsync(cancellationToken).ConfigureAwait(false);

            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        public override void Dispose()
        {
            httpClient.Dispose();
            base.Dispose();
        }

        private async Task SendBatchAsync(IReadOnlyCollection<DatadogLogEntry> entries, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(entries);
            var bytes = Encoding.UTF8.GetBytes(json);

            if (bytes.Length > MaximumPayloadBytes && entries.Count > 1)
            {
                var midpoint = entries.Count / 2;
                await SendBatchAsync(entries.Take(midpoint).ToArray(), cancellationToken).ConfigureAwait(false);
                await SendBatchAsync(entries.Skip(midpoint).ToArray(), cancellationToken).ConfigureAwait(false);
                return;
            }

            if (bytes.Length > MaximumPayloadBytes)
            {
                var oversized = entries.Single();

                entries =
                [
                    new DatadogLogEntry
                    {
                        Timestamp = oversized.Timestamp,
                        Message = "A Datadog log entry exceeded the maximum payload size and its properties were discarded.",
                        Status = "warning",
                        Service = oversized.Service,
                        Source = oversized.Source,
                        Tags = oversized.Tags,
                        Properties = new Dictionary<string, object>
                        {
                            ["datadog.payload.truncated"] = true,
                            ["datadog.payload.original_bytes"] = bytes.Length
                        }
                    }
                ];

                bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entries));
            }

            var compressed = Compress(bytes);

            for (var attempt = 1; attempt <= 3; attempt++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, options.Endpoint);

                request.Headers.TryAddWithoutValidation("DD-API-KEY", options.ApiKey);
                request.Content = new ByteArrayContent(compressed);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                request.Content.Headers.ContentEncoding.Add("gzip");

                try
                {
                    using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                        return;

                    if (response.StatusCode != HttpStatusCode.TooManyRequests && (int)response.StatusCode < 500)
                        return;
                }
                catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
                {
                    Debug.WriteLine($"Datadog log delivery failed: {exception.GetType().Name}");
                }

                if (attempt < 3)
                    await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken).ConfigureAwait(false);
            }
        }

        private static byte[] Compress(byte[] value)
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Fastest, leaveOpen: true))
                gzip.Write(value, 0, value.Length);
            return output.ToArray();
        }
    }
}
