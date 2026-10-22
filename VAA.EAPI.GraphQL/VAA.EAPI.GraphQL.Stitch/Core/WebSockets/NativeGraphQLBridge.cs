using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Stitch.Core.Helpers;
using System.Collections.Generic;
using VAA.EAPI.GraphQL.FunctionHost.WebSockets;

namespace VAA.EAPI.GraphQL.Stitch.Core.WebSockets
{
    public sealed class NativeGraphQLBridge
    {
        private readonly NativeSubscriptionRegistry registry;
        public NativeGraphQLBridge(NativeSubscriptionRegistry registry)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        private static IReadOnlyDictionary<string, object> CaptureHeaders(HttpRequestData request)
        {
            var allowed = new[] { "Accept", "Authorization", "Ocp-Apim-Subscription-Key", "api-channel", "api-correlation-id", "api-session-id", "api-operation-id" };
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in allowed)
            {
                if (request.Headers.TryGetValues(name, out var values))
                {
                    headers[name] = string.Join(",", values);
                }
            }
            return new Dictionary<string, object> { [RemoteSubscriptionEndpoints.HeadersProperty] = headers };
        }
        [Function(nameof(NativeGraphQLBridge))]
        [OpenApiOperation(operationId: nameof(NativeGraphQLBridge), tags: new[] { "Internal" }, Visibility = OpenApiVisibilityType.Internal,
            Description = "Internal native WebSocket callback; requires bridge credential; caller authorization is enforced by GraphQL. Do not publish through APIM.")]
        [OpenApiRequestBody("application/json", typeof(JObject), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(JObject))]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized)]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest)]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound)]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "internal/graphql-websocket")] HttpRequestData request,
            FunctionContext context)
        {
            var response = request.CreateResponse();
            response.Headers.Add("Cache-Control", "no-store");
            response.Headers.Add("Content-Type", "application/json");
            var expected = ConfigReader.Try("NativeWebSocketsBridgeSecret") ?? "";
            var credentials = request.Headers.TryGetValues("X-Native-WebSocket-Bridge", out var values) ? values.ToArray() : Array.Empty<string>();
            var supplied = credentials.Length == 1 ? credentials[0] : "";
            if (expected.Trim().Length < 32 || expected.TrimStart().StartsWith("@Microsoft.KeyVault(", StringComparison.OrdinalIgnoreCase) ||
                !CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(expected)), SHA256.HashData(Encoding.UTF8.GetBytes(supplied))))
            {
                response.StatusCode = HttpStatusCode.Unauthorized;
                return response;
            }
            try
            {
                // Stitch forwards credentials; downstream GraphQL middleware owns authentication.
                System.Security.Claims.ClaimsPrincipal principal = null;
                using var reader = new StreamReader(request.Body);
                var buffer = new char[65537];
                var count = await reader.ReadBlockAsync(buffer, 0, buffer.Length);
                if (count > 65536) { response.StatusCode = HttpStatusCode.BadRequest; return response; }
                var body = JObject.Parse(new string(buffer, 0, count));
                if (!Guid.TryParse(body.Value<string>("connectionId"), out var connectionId) || connectionId == Guid.Empty)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }
                JObject result;
                switch (body.Value<string>("command"))
                {
                    case "init": result = new JObject { ["ok"] = true }; break;
                    case "start": result = await registry.StartAsync(body, principal, connectionId, CaptureHeaders(request)); break;
                    case "read" when Guid.TryParse(body.Value<string>("sessionId"), out var readSessionId) && readSessionId != Guid.Empty:
                        result = await registry.ReadAsync(readSessionId, principal, context.CancellationToken, connectionId);
                        break;
                    case "stop" when Guid.TryParse(body.Value<string>("sessionId"), out var stopSessionId) && stopSessionId != Guid.Empty:
                        await registry.StopAsync(stopSessionId, principal, connectionId);
                        result = new JObject { ["ok"] = true };
                        break;
                    default: response.StatusCode = HttpStatusCode.BadRequest; return response;
                }
                response.StatusCode = HttpStatusCode.OK;
                await response.WriteStringAsync(result.ToString(Newtonsoft.Json.Formatting.None));
            }
            catch (UnauthorizedAccessException) { response.StatusCode = HttpStatusCode.Unauthorized; }
            catch (Newtonsoft.Json.JsonException) { response.StatusCode = HttpStatusCode.BadRequest; }
            catch (FormatException) { response.StatusCode = HttpStatusCode.BadRequest; }
            return response;
        }
    }
}
