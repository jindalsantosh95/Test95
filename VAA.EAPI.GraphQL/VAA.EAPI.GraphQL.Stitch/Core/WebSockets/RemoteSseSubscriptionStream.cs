using HotChocolate;
using HotChocolate.Execution;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace VAA.EAPI.GraphQL.Stitch.Core.WebSockets
{
    internal sealed class RemoteSseSubscriptionStream : ISourceStream
    {
        // Streaming needs headers-only completion and must never redirect caller credentials.
        private static readonly HttpClient Client = new(new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        })
        { Timeout = Timeout.InfiniteTimeSpan };
        private readonly CancellationTokenSource lifetime;
        private HttpResponseMessage response;
        private StreamReader reader;
        private int disposed;

        private RemoteSseSubscriptionStream(CancellationToken token)
        {
            lifetime = CancellationTokenSource.CreateLinkedTokenSource(token);
            lifetime.CancelAfter(TimeSpan.FromMinutes(5));
        }

        public static async Task<RemoteSseSubscriptionStream> ConnectAsync(Uri endpoint, JObject payload,
            IReadOnlyDictionary<string, string> headers, CancellationToken token)
        {
            var stream = new RemoteSseSubscriptionStream(token);
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                foreach (var header in headers.Where(x => !x.Key.Equals("Accept", StringComparison.OrdinalIgnoreCase)))
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
                request.Headers.Accept.ParseAdd("text/event-stream");
                request.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stream.lifetime.Token);
                timeout.CancelAfter(TimeSpan.FromSeconds(30));
                stream.response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                stream.response.EnsureSuccessStatusCode();
                if (stream.response.Content.Headers.ContentType?.MediaType != "text/event-stream")
                {
                    throw new GraphQLException("Remote subscription did not return an SSE stream.");
                }
                stream.reader = new StreamReader(await stream.response.Content.ReadAsStreamAsync(timeout.Token), Encoding.UTF8);
                return stream;
            }
            catch { await stream.DisposeAsync(); throw; }
        }

        public IAsyncEnumerable<object> ReadEventsAsync() => ReadAsync(lifetime.Token);

        private async IAsyncEnumerable<object> ReadAsync([EnumeratorCancellation] CancellationToken token)
        {
            var line = new StringBuilder();
            var data = new StringBuilder();
            var name = "";
            var character = new char[1];
            var size = 0;
            var carriageReturn = false;
            while (await reader.ReadAsync(character.AsMemory(), token) != 0)
            {
                var current = character[0];
                if (carriageReturn && current == '\n') { carriageReturn = false; continue; }
                carriageReturn = current == '\r';
                if (++size > 1024 * 1024) { throw new GraphQLException("Remote SSE event exceeded the size limit."); }
                if (current is not ('\r' or '\n')) { line.Append(current); continue; }
                var text = line.ToString();
                line.Clear();
                if (text.Length == 0)
                {
                    if (name == "complete") { yield break; }
                    if (data.Length > 0)
                    {
                        if (name != "next") { throw new GraphQLException("Invalid remote SSE event."); }
                        using var json = new JsonTextReader(new StringReader(data.ToString())) { MaxDepth = 64, DateParseHandling = DateParseHandling.None };
                        var payload = JObject.Load(json);
                        yield return new RemoteSubscriptionEvent(
                            payload["data"] is JObject result ? (IReadOnlyDictionary<string, object>)RemoteSubscriptionStream.ToValue(result) : new Dictionary<string, object>(),
                            RemoteSubscriptionStream.ReadErrors(payload["errors"]));
                    }
                    data.Clear();
                    name = "";
                    size = 0;
                    continue;
                }
                var colon = text.IndexOf(':');
                var field = colon < 0 ? text : text[..colon];
                var value = colon < 0 ? "" : text[(colon + 1)..];
                if (value.StartsWith(' ')) { value = value[1..]; }
                if (field == "event") { name = value; }
                else if (field == "data") { data.Append(value).Append('\n'); }
            }
            throw new GraphQLException("Remote SSE connection ended before completion; reconnect and subscribe again.");
        }

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
            {
                lifetime.Cancel();
                reader?.Dispose();
                response?.Dispose();
                lifetime.Dispose();
            }
            return ValueTask.CompletedTask;
        }
    }
}
