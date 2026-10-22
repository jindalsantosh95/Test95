using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Http;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;
using XunitHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.GraphQL.Infrastructure.Tests
{
    public sealed class HttpConnectorStreamingTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public async Task RawResponse_DeliversFirstChunkBeforeProducerCompletes(int overload)
        {
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var producer = new GatedStream("first", "second");
            var uri = new Uri("https://stream.test/" + Guid.NewGuid().ToString("N"));
            var connector = CreateConnector(uri, producer);
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var cookies = new CookieContainer();
            cookies.Add(uri, new Cookie("session", "synthetic"));

            var pending = overload switch
            {
                0 => connector.SendAsync(request, deadline.Token),
                1 => connector.SendAsync(request, deadline.Token, noWait: true),
                2 => connector.SendAsync(request, cookies, deadline.Token),
                3 => connector.SendAsync(request, cookies, deadline.Token, noWait: true),
                4 => connector.SendAsync(uri.ToString(), null, null, HttpMethod.Get, string.Empty),
                _ => connector.SendAsync(uri.ToString(), null, null, HttpMethod.Get, string.Empty, cookies)
            };
            using var response = await pending.WaitAsync(deadline.Token);
            await using var stream = await response.Content.ReadAsStreamAsync(deadline.Token);
            var buffer = new byte[32];
            var count = await stream.ReadAsync(buffer, deadline.Token);
            Assert.Equal("first", Encoding.UTF8.GetString(buffer, 0, count));
            Assert.False(producer.TailRequested.Task.IsCompleted);
            Assert.Null(response.Content.Headers.ContentLength);
            if (overload is 2 or 3 or 5)
            {
                Assert.Equal("dark", cookies.GetCookies(uri)["theme"]?.Value);
                Assert.Contains("session=synthetic", response.RequestMessage!.Headers.GetValues("Cookie"));
            }

            producer.ReleaseTail();
            count = await stream.ReadAsync(buffer, deadline.Token);
            Assert.Equal("second", Encoding.UTF8.GetString(buffer, 0, count));
            Assert.Equal(0, await stream.ReadAsync(buffer, deadline.Token));
        }

        [Fact]
        public async Task PendingBodyRead_CanBeCancelledAndDisposesProducer()
        {
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var readCancellation = new CancellationTokenSource();
            using var producer = new GatedStream("first", "second");
            var uri = new Uri("https://stream.test/" + Guid.NewGuid().ToString("N"));
            var connector = CreateConnector(uri, producer);
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await connector.SendAsync(request, deadline.Token).WaitAsync(deadline.Token);
            await using var stream = await response.Content.ReadAsStreamAsync(deadline.Token);
            var buffer = new byte[32];
            var count = await stream.ReadAsync(buffer, deadline.Token);
            Assert.Equal("first", Encoding.UTF8.GetString(buffer, 0, count));
            var pending = stream.ReadAsync(buffer, readCancellation.Token).AsTask();
            await producer.TailRequested.Task.WaitAsync(deadline.Token);
            readCancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
            await stream.DisposeAsync();
            Assert.Null(response.Content.Headers.ContentLength);
            response.Dispose();
            Assert.True(producer.Disposed);
        }

        [Fact]
        public async Task TypedResponse_WaitsForCompleteJsonAndPreservesPayload()
        {
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var producer = new GatedStream("{\"value\":", "42}");
            var uri = new Uri("https://stream.test/" + Guid.NewGuid().ToString("N"));
            var connector = CreateConnector(uri, producer);
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var pending = connector.SendAsync<Dictionary<string, int>>(request, deadline.Token);
            await producer.TailRequested.Task.WaitAsync(deadline.Token);
            Assert.False(pending.IsCompleted);
            producer.ReleaseTail();
            var result = await pending.WaitAsync(deadline.Token);
            using var response = result.ResponseMessage;
            Assert.Equal(42, result.Response["value"]);
            Assert.Equal("{\"value\":42}", result.RawResponse);
        }

        private static HttpConnector CreateConnector(Uri uri, GatedStream producer)
        {
            var factory = new XunitHttpClientFactory();
            // Static .mock bodies cannot model a producer that remains open between chunks.
            // Keep the real connector, factory and diagnostics handler, replacing only the
            // buffering mock/cache pipeline with a controlled transport for these protocol tests.
            factory.CreateHandler(uri.ToString()).InnerHandler = new CookieDelegatingHandler(
                new StreamingHandler(producer), NullLogger.Instance);
            return new HttpConnector(NullLogger<HttpConnector>.Instance, factory);
        }

        private sealed class StreamingHandler(GatedStream producer) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(producer)
                };
                response.Headers.TryAddWithoutValidation("Set-Cookie", "theme=dark; Path=/; HttpOnly");
                return Task.FromResult(response);
            }
        }

        private sealed class GatedStream(string prefix, string suffix) : Stream
        {
            private readonly byte[] first = Encoding.UTF8.GetBytes(prefix);
            private readonly byte[] last = Encoding.UTF8.GetBytes(suffix);
            private readonly TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private int position;
            public TaskCompletionSource TailRequested { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
            public bool Disposed { get; private set; }
            public void ReleaseTail() => release.TrySetResult();
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => position; set => throw new NotSupportedException(); }
            public override void Flush() { }
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
                ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (position < first.Length)
                {
                    var count = Math.Min(buffer.Length, first.Length - position);
                    first.AsMemory(position, count).CopyTo(buffer);
                    position += count;
                    return count;
                }
                TailRequested.TrySetResult();
                await release.Task.WaitAsync(cancellationToken);
                var remaining = Math.Min(buffer.Length, last.Length - (position - first.Length));
                last.AsMemory(position - first.Length, remaining).CopyTo(buffer);
                position += remaining;
                return remaining;
            }

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
                release.TrySetCanceled();
                base.Dispose(disposing);
            }
        }
    }
}
