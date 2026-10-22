using HotChocolate;
using HotChocolate.Execution;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace VAA.EAPI.GraphQL.Stitch.Core.WebSockets
{
    public sealed record RemoteSubscriptionEvent(IReadOnlyDictionary<string, object> Data, IReadOnlyList<IError> Errors);

    internal sealed class RemoteSubscriptionStream : ISourceStream
    {
        private readonly ClientWebSocket socket = new();
        private readonly CancellationTokenSource lifetime;
        private readonly SemaphoreSlim sendLock = new(1, 1);
        private int disposed;

        private RemoteSubscriptionStream(CancellationToken cancellationToken)
        {
            lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            lifetime.CancelAfter(TimeSpan.FromMinutes(5));
        }

        public static async Task<RemoteSubscriptionStream> ConnectAsync(Uri endpoint, JObject payload,
            IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            var stream = new RemoteSubscriptionStream(cancellationToken);
            try
            {
                stream.socket.Options.AddSubProtocol("graphql-transport-ws");
                foreach (var header in headers)
                {
                    stream.socket.Options.SetRequestHeader(header.Key, header.Value);
                }
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stream.lifetime.Token);
                timeout.CancelAfter(TimeSpan.FromSeconds(10));
                await stream.socket.ConnectAsync(endpoint, timeout.Token);
                if (stream.socket.SubProtocol != "graphql-transport-ws")
                {
                    throw new GraphQLException("The remote subscription endpoint did not negotiate graphql-transport-ws.");
                }
                await stream.SendAsync(new JObject { ["type"] = "connection_init" }, timeout.Token);
                while (true)
                {
                    var reply = await stream.ReceiveAsync(timeout.Token);
                    if (reply.Value<string>("type") == "connection_ack") { break; }
                    if (reply.Value<string>("type") != "ping")
                    {
                        throw new GraphQLException("The remote subscription endpoint rejected initialization.");
                    }
                    await stream.SendAsync(new JObject { ["type"] = "pong", ["payload"] = reply["payload"] }, timeout.Token);
                }
                await stream.SendAsync(new JObject { ["id"] = "1", ["type"] = "subscribe", ["payload"] = payload }, timeout.Token);
                return stream;
            }
            catch
            {
                await stream.DisposeAsync();
                throw;
            }
        }

        public IAsyncEnumerable<object> ReadEventsAsync() => ReadAsync(lifetime.Token);

        private async IAsyncEnumerable<object> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await ReceiveAsync(cancellationToken);
                switch (message.Value<string>("type"))
                {
                    case "ping":
                        await SendAsync(new JObject { ["type"] = "pong", ["payload"] = message["payload"] }, cancellationToken);
                        break;
                    case "pong": break;
                    case "complete" when message.Value<string>("id") == "1": yield break;
                    case "error" when message.Value<string>("id") == "1":
                        throw new GraphQLException(ReadErrors(message["payload"]));
                    case "next" when message.Value<string>("id") == "1" && message["payload"] is JObject payload:
                        yield return new RemoteSubscriptionEvent(
                            payload["data"] is JObject data ? (IReadOnlyDictionary<string, object>)ToValue(data) : new Dictionary<string, object>(),
                            ReadErrors(payload["errors"]));
                        break;
                    default: throw new GraphQLException("Invalid remote subscription message.");
                }
            }
        }

        internal static IReadOnlyList<IError> ReadErrors(JToken errors)
        {
            if (errors is not JArray array) { return Array.Empty<IError>(); }
            return array.Select(error =>
            {
                var builder = ErrorBuilder.New().SetMessage(error.Value<string>("message") ?? "Remote subscription failed.");
                if (error["extensions"] is JObject extensions)
                {
                    foreach (var property in extensions.Properties()) { builder.SetExtension(property.Name, ToValue(property.Value)); }
                    if (extensions.Value<string>("code") is string code) { builder.SetCode(code); }
                }
                if (error["path"] is JArray segments)
                {
                    var path = HotChocolate.Path.Root;
                    foreach (var segment in segments)
                    {
                        path = segment.Type == JTokenType.Integer ? path.Append(segment.Value<int>()) : path.Append(segment.Value<string>());
                    }
                    builder.SetPath(path);
                }
                return builder.Build();
            }).ToArray();
        }

        internal static object ToValue(JToken token) => token switch
        {
            JObject obj => obj.Properties().ToDictionary(x => x.Name, x => ToValue(x.Value)),
            JArray array => array.Select(ToValue).ToList(),
            JValue value => value.Value,
            _ => null
        };

        private async Task<JObject> ReceiveAsync(CancellationToken cancellationToken)
        {
            using var buffer = new MemoryStream();
            var bytes = new byte[8192];
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(bytes), cancellationToken);
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    throw new GraphQLException("The remote subscription connection ended or returned a non-text message.");
                }
                if (buffer.Length + result.Count > 1024 * 1024)
                {
                    throw new GraphQLException("The remote subscription event exceeded 1 MiB.");
                }
                buffer.Write(bytes, 0, result.Count);
            } while (!result.EndOfMessage);
            using var reader = new JsonTextReader(new StreamReader(new MemoryStream(buffer.ToArray()), Encoding.UTF8))
            {
                DateParseHandling = DateParseHandling.None,
                MaxDepth = 64
            };
            return JObject.Load(reader);
        }

        private async Task SendAsync(JObject message, CancellationToken cancellationToken)
        {
            await sendLock.WaitAsync(cancellationToken);
            try
            {
                await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message.ToString(Formatting.None))),
                    WebSocketMessageType.Text, true, cancellationToken);
            }
            finally { sendLock.Release(); }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0) { return; }
            try
            {
                // Request completion before aborting any pending receive. This also releases downstream topics.
                if (socket.State == WebSocketState.Open)
                {
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    await SendAsync(new JObject { ["id"] = "1", ["type"] = "complete" }, timeout.Token);
                }
            }
            catch (Exception exception) when (exception is WebSocketException or OperationCanceledException) { }
            finally
            {
                lifetime.Cancel();
                socket.Abort();
                socket.Dispose();
                lifetime.Dispose();
            }
        }
    }
}
