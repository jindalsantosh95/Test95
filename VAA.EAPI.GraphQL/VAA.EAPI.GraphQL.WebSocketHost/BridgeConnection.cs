using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;

namespace VAA.EAPI.GraphQL.WebSocketHost
{
    public sealed class BridgeConnection
    {
        private readonly WebSocket socket;
        private readonly HttpContext context;
        private readonly RequestDelegate dispatch;
        private readonly TransportOptions options;
        private readonly SemaphoreSlim sendLock = new(1, 1);
        private readonly ConcurrentDictionary<string, Operation> operations = new();
        private readonly Guid connectionId = Guid.NewGuid();
        private string? authorization;
        private bool initialized;

        public BridgeConnection(WebSocket socket, HttpContext context, RequestDelegate dispatch, TransportOptions options)
        {
            this.socket = socket;
            this.context = context;
            this.dispatch = dispatch;
            this.options = options;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            using var connectionLifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var initializeBy = DateTimeOffset.UtcNow.AddSeconds(10);
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    using var receiveTimeout = CancellationTokenSource.CreateLinkedTokenSource(connectionLifetime.Token);
                    if (!initialized)
                    {
                        var remaining = initializeBy - DateTimeOffset.UtcNow;
                        receiveTimeout.CancelAfter(remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero);
                    }
                    var message = await ReceiveAsync(receiveTimeout.Token);
                    if (message is null) { break; }
                    var type = message.Value<string>("type");
                    var id = message.Value<string>("id");
                    switch (type)
                    {
                        case "connection_init":
                            if (initialized) { await CloseAsync(4429, "Already initialized"); return; }
                            if (message["payload"] is JToken initializationPayload && initializationPayload.Type is not (JTokenType.Object or JTokenType.Null))
                            {
                                await CloseAsync(4400, "Initialization payload must be an object");
                                return;
                            }
                            var suppliedAuthorization = context.Request.Headers.ContainsKey("Authorization");
                            authorization = suppliedAuthorization ? context.Request.Headers.Authorization.ToString() : null;
                            if (!suppliedAuthorization && message["payload"] is JObject initialization)
                            {
                                var credential = initialization.Property("Authorization") ?? initialization.Property("authorization");
                                suppliedAuthorization = credential is not null;
                                authorization = credential?.Value.Type == JTokenType.String ? credential.Value.Value<string>() : null;
                            }
                            if (suppliedAuthorization && (authorization is null || authorization.Trim().Length == 0 ||
                                !authorization.Split(',').All(value => AuthenticationHeaderValue.TryParse(value, out _))))
                            {
                                await CloseAsync(4403, "Authentication rejected");
                                return;
                            }
                            await CallAsync(new JObject { ["command"] = "init" }, cancellationToken);
                            initialized = true;
                            await SendAsync(new JObject { ["type"] = "connection_ack" }, cancellationToken);
                            break;
                        case "ping":
                            await SendAsync(new JObject { ["type"] = "pong", ["payload"] = message["payload"]?.DeepClone() }, cancellationToken);
                            break;
                        case "pong":
                            break;
                        case "subscribe":
                            if (!initialized) { await CloseAsync(4401, "Initialize first"); return; }
                            if (id is null || id.Length == 0 || id.Length > 128 || message["payload"] is not JObject payload)
                            {
                                await CloseAsync(4400, "Invalid subscribe message"); return;
                            }
                            if (operations.ContainsKey(id)) { await CloseAsync(4409, "Duplicate operation ID"); return; }
                            if (operations.Count >= 8) { await CloseAsync(4400, "Maximum eight active operations"); return; }
                            var start = (JObject)payload.DeepClone();
                            start["command"] = "start";
                            var response = await CallAsync(start, cancellationToken);
                            if (response["errors"] is JArray errors)
                            {
                                await SendAsync(new JObject { ["id"] = id, ["type"] = "error", ["payload"] = errors }, cancellationToken);
                                break;
                            }
                            var operation = new Operation(response.Value<string>("sessionId")!, connectionLifetime.Token);
                            operations[id] = operation;
                            operation.Pump = PumpAsync(id, operation);
                            break;
                        case "complete":
                            if (id is not null && operations.TryRemove(id, out var complete))
                            {
                                complete.Lifetime.Cancel();
                                await complete.Pump;
                            }
                            break;
                        default:
                            await CloseAsync(4400, "Unsupported message"); return;
                    }
                }
            }
            catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                await CloseAsync(4403, "Authentication rejected");
            }
            catch (JsonException)
            {
                await CloseAsync(4400, "Invalid JSON message");
            }
            catch (OperationCanceledException)
            {
                await CloseAsync(initialized ? 1000 : 4408, initialized ? "Connection lifetime ended" : "Initialization timeout");
            }
            finally
            {
                connectionLifetime.Cancel();
                await Task.WhenAll(operations.Values.Select(x => x.Pump));
                sendLock.Dispose();
            }
        }

        private async Task PumpAsync(string id, Operation operation)
        {
            try
            {
                while (!operation.Lifetime.IsCancellationRequested)
                {
                    var reply = await CallAsync(new JObject { ["command"] = "read", ["sessionId"] = operation.SessionId }, operation.Lifetime.Token);
                    if (reply.Value<bool>("complete"))
                    {
                        await SendAsync(new JObject { ["type"] = "complete", ["id"] = id }, operation.Lifetime.Token);
                        break;
                    }
                    if (reply["result"] is JToken result)
                    {
                        await SendAsync(new JObject { ["type"] = "next", ["id"] = id, ["payload"] = result }, operation.Lifetime.Token);
                    }
                }
            }
            catch (OperationCanceledException) when (operation.Lifetime.IsCancellationRequested) { }
            catch (Exception)
            {
                if (!operation.Lifetime.IsCancellationRequested)
                {
                    try
                    {
                        await SendAsync(new JObject
                        {
                            ["type"] = "error",
                            ["id"] = id,
                            ["payload"] = new JArray(new JObject { ["message"] = "Subscription ended; reconnect and subscribe again." })
                        }, operation.Lifetime.Token);
                    }
                    catch (Exception) when (socket.State != WebSocketState.Open || operation.Lifetime.IsCancellationRequested) { }
                }
            }
            finally
            {
                using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                try { await CallAsync(new JObject { ["command"] = "stop", ["sessionId"] = operation.SessionId }, cleanup.Token); }
                catch (Exception) { /* Worker-side expiry reclaims the stream if the callback is unavailable. */ }
                operations.TryRemove(id, out _);
                operation.Lifetime.Dispose();
            }
        }

        private async Task<JObject?> ReceiveAsync(CancellationToken cancellationToken)
        {
            using var buffer = new MemoryStream();
            var segment = new byte[4096];
            WebSocketReceiveResult frame;
            do
            {
                frame = await socket.ReceiveAsync(new ArraySegment<byte>(segment), cancellationToken);
                if (frame.MessageType == WebSocketMessageType.Close)
                {
                    await CloseAsync((int)(frame.CloseStatus ?? WebSocketCloseStatus.NormalClosure), frame.CloseStatusDescription ?? string.Empty);
                    return null;
                }
                if (frame.MessageType != WebSocketMessageType.Text || buffer.Length + frame.Count > 65536)
                {
                    await CloseAsync(4400, "Text messages up to 64 KiB only");
                    return null;
                }
                buffer.Write(segment, 0, frame.Count);
            } while (!frame.EndOfMessage);
            return JObject.Parse(Encoding.UTF8.GetString(buffer.ToArray()));
        }

        private async Task SendAsync(JObject message, CancellationToken cancellationToken)
        {
            var bytes = Encoding.UTF8.GetBytes(message.ToString(Formatting.None));
            if (bytes.Length > 1024 * 1024) { throw new InvalidDataException("Result exceeds one MiB."); }
            await sendLock.WaitAsync(cancellationToken);
            try { await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken); }
            finally { sendLock.Release(); }
        }

        private Task<JObject> CallAsync(JObject body, CancellationToken cancellationToken) =>
            new SubscriptionBridgeClient(context, dispatch, options, connectionId, authorization).CallAsync(body, cancellationToken);

        private async Task CloseAsync(int code, string reason)
        {
            if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                try { await socket.CloseOutputAsync((WebSocketCloseStatus)code, reason, timeout.Token); }
                catch (Exception) { socket.Abort(); }
            }
        }

        private sealed class Operation
        {
            public Operation(string sessionId, CancellationToken token)
            {
                SessionId = sessionId;
                Lifetime = CancellationTokenSource.CreateLinkedTokenSource(token);
            }
            public string SessionId { get; }
            public CancellationTokenSource Lifetime { get; }
            public Task Pump { get; set; } = Task.CompletedTask;
        }
    }
}
