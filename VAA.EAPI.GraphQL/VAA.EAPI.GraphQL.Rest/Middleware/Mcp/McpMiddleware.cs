using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Rest.FunctionHost;
using VAA.EAPI.GraphQL.Rest.Middleware;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{
    public sealed class McpMiddleware(Router router, IHttpConnector httpConnector, ILogger<McpMiddleware> logger) : IFunctionsWorkerMiddleware
    {
        public const string Ping = "ping";
        public const string Initialize = "initialize";
        public const string Initialized = "initialized";
        public const string ToolsList = "tools/list";
        public const string ToolsCall = "tools/call";
        public const string OAuth2PkceAuthorize = "oauth2/pkce/authorize";
        public const string OAuth2PkceToken = "oauth2/pkce/token";

        private readonly Router _router = router ?? throw new ArgumentNullException(nameof(router));
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly ILogger<McpMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var httpRequest = await context.GetHttpRequestDataAsync();

            if (httpRequest == null)
            {
                await next(context);

                return;
            }

            var isMcp = httpRequest.Method.Is(HttpMethod.Post.Method) &&
                        (httpRequest.Headers.TryGetValues("x-mcp-request", out _) ||
                         httpRequest.Url.AbsolutePath.StartsWith("/api/mcp", StringComparison.OrdinalIgnoreCase));

            if (!isMcp)
            {
                await next(context);

                return;
            }

            var body = await httpRequest.ReadAsStringAsync();

            if (httpRequest.Body.CanSeek)
                httpRequest.Body.Position = 0;

            Request mcpRequest = null;

            try
            {
                mcpRequest = JsonConvert.DeserializeObject<Request>(body);
            }
            catch (JsonException)
            {
                var errorResponse = CreateJsonRpcErrorResponse(null, -32700, "Parse error", null, httpRequest);

                context.GetInvocationResult().Value = errorResponse;

                return;
            }

            if (mcpRequest == null || mcpRequest.Jsonrpc != "2.0" || mcpRequest.Method.IsNullOrWhiteSpace())
            {
                var errorResponse = CreateJsonRpcErrorResponse(mcpRequest?.Id, -32600, "Invalid Request", null, httpRequest);

                context.GetInvocationResult().Value = errorResponse;

                return;
            }

            HttpResponseData result = null;

            switch (mcpRequest.Method.ToLowerInvariant())
            {
                // Life-cycle
                case Initialize:
                    var clientParams = mcpRequest.Params?.ToObject<InitializeParams>();

                    var initializeResult = new InitializeResult
                    {
                        ProtocolVersion = "2025-11-25",
                        Capabilities = new()
                        {
                            Tools = new() { ListChanged = false },
                            Resources = new() { ListChanged = false, Subscribe = false },
                            Logging = null,
                            Prompts = new() { ListChanged = false },
                            Experimental = new Dictionary<string, object>
                            {
                                { "Sampling", new SamplingCapability { CreateMessage = false } },
                                { "Roots", new RootsCapability { ListChanged = false } }
                            }
                        },
                        ServerInfo = new()
                        {
                            Name = "VAA-MCP-Server",
                            Version = "1.0.0",
                            Title = "Virgin Atlantic MCP Server",
                            Description = "Implements MCP for GraphQL REST API",
                            WebsiteUrl = "https://your-server-url",
                            Icons = null
                        },
                        Instructions = "Use this server to access tools and resources via MCP.",
                        Meta = null
                    };

                    result = CreateJsonRpcSuccessfulResponse(mcpRequest.Id, initializeResult, httpRequest);

                    break;
                case Initialized:

                    _logger.LogInformation("MCP Client successfully initialized. Connection established.");

                    result = httpRequest.CreateResponse(HttpStatusCode.NoContent);

                    break;
                case Ping:

                    result = CreateJsonRpcSuccessfulResponse(mcpRequest.Id, JToken.FromObject("pong"), httpRequest);

                    break;

                // Tools
                case ToolsList:

                    var principal = context.Features.Get<JwtPrincipalFeature>()?.Principal;
                    var callerContext = context.Features.Get<McpCallerContextFeature>()?.Principal;
                    result = CreateJsonRpcSuccessfulResponse(mcpRequest.Id, (ListToolsResult)_router.Tools.Where(tool =>
                        tool.CanAccess(principal) && tool.HasRequiredCallerContext(principal, callerContext)).ToArray(), httpRequest);

                    break;
                case ToolsCall:
                    {
                        var toolName = mcpRequest.Params?["name"]?.Value<string>();
                        var tool = _router.Tools.FirstOrDefault(candidate => candidate.Name.Is(toolName));
                        if (tool is not null && !tool.HasRequiredCallerContext(
                            context.Features.Get<JwtPrincipalFeature>()?.Principal,
                            context.Features.Get<McpCallerContextFeature>()?.Principal))
                        {
                            result = CreateJsonRpcUnauthorizedResponse(mcpRequest.Id, httpRequest);
                            break;
                        }
                        await next(context);
                    }
                    break;

                // For future implementation
                /*
                // Resources
                case "resources/list":
                    break;
                case "resources/read":
                    break;
                case "resources/subscribe":
                    break;
                case "resources/unsubscribe":
                    break;

                // Prompts
                case "prompts/list":
                    break;
                case "prompts/get":
                    break;

                // Sampling (Server-to-Client)
                case "sampling/createMessage":
                    break;

                // Roots
                case "roots/list":
                    break;

                // Logging & Notifications
                case "logging/setLevel":
                    break;
                case "notifications/message":
                    break;
                case "notifications/tools/list_changed":
                case "notifications/resources/list_changed":
                case "notifications/prompts/list_changed":
                    break;
                */

                // OAuth2 PKCE
                case OAuth2PkceAuthorize:
                    {
                        var codeVerifier = GenerateCodeVerifier();
                        var codeChallenge = GenerateCodeChallenge(codeVerifier);
                        var authorizeUrl = mcpRequest.Params?["authorize_url"]?.ToString();

                        if (authorizeUrl.IsNullOrWhiteSpace())
                        {
                            result = CreateJsonRpcErrorResponse(mcpRequest.Id, -32602, "Missing 'authorize_url' parameter", null, httpRequest);
                            break;
                        }

                        var clientId = mcpRequest.Params?["client_id"]?.ToString();
                        var redirectUri = mcpRequest.Params?["redirect_uri"]?.ToString();
                        var scope = mcpRequest.Params?["scope"]?.ToString() ?? "openid profile";
                        var state = Guid.NewGuid().ToString();
                        var uriBuilder = new UriBuilder(authorizeUrl);
                        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                        query.Add("response_type", "code");
                        query.Add("client_id", clientId);
                        query.Add("redirect_uri", redirectUri);
                        query.Add("scope", scope);
                        query.Add("state", state);
                        query.Add("code_challenge", codeChallenge);
                        query.Add("code_challenge_method", "S256");
                        uriBuilder.Query = query.ToString();

                        result = CreateJsonRpcSuccessfulResponse(mcpRequest.Id, new
                        {
                            authorize_url = uriBuilder.ToString(),
                            code_verifier = codeVerifier,
                            state
                        }, httpRequest);
                        break;
                    }
                case OAuth2PkceToken:
                    {
                        var tokenUrl = mcpRequest.Params?["token_url"]?.ToString();

                        if (tokenUrl.IsNullOrWhiteSpace())
                        {
                            result = CreateJsonRpcErrorResponse(mcpRequest.Id, -32602, "Missing 'token_url' parameter", null, httpRequest);
                            break;
                        }

                        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
                        {
                            Content = new FormUrlEncodedContent(
                            [
                                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                                new KeyValuePair<string, string>("code", mcpRequest.Params?["code"]?.ToString()),
                                new KeyValuePair<string, string>("redirect_uri", mcpRequest.Params?["redirect_uri"]?.ToString()),
                                new KeyValuePair<string, string>("client_id", mcpRequest.Params?["client_id"]?.ToString()),
                                new KeyValuePair<string, string>("code_verifier", mcpRequest.Params?["code_verifier"]?.ToString())
                            ])
                        };

                        var tokenResponse = await _httpConnector.SendAsync(httpRequestMessage, CancellationToken.None);
                        var responseBody = await tokenResponse.Content.ReadAsStringAsync();
                        result = tokenResponse.IsSuccessStatusCode
                            ? CreateJsonRpcSuccessfulResponse(mcpRequest.Id, JsonConvert.DeserializeObject<JObject>(responseBody), httpRequest)
                            : CreateJsonRpcErrorResponse(mcpRequest.Id, (int)tokenResponse.StatusCode, "Token exchange failed", JToken.FromObject(responseBody), httpRequest);
                        break;
                    }

                default:

                    result = CreateJsonRpcErrorResponse(mcpRequest.Id, -32601, "Method not found", null, httpRequest);

                    break;
            }

            if (result is not null)
                context.GetInvocationResult().Value = result;
        }

        private static string GenerateCodeVerifier()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Base64UrlEncode(bytes);
        }

        private static string GenerateCodeChallenge(string codeVerifier) =>
            Base64UrlEncode(SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier)));

        private static string Base64UrlEncode(byte[] input) =>
            Convert.ToBase64String(input).Replace("+", "-").Replace("/", "_").Replace("=", "");

        private static HttpResponseData CreateJsonRpcErrorResponse(JToken id, int code, string message, JToken data, HttpRequestData httpRequest)
        {
            var errorResponse = new Response
            {
                Jsonrpc = "2.0",
                Error = new Error
                {
                    Code = code,
                    Message = message,
                    Data = data
                },
                Id = id
            };

            var response = httpRequest.CreateResponse(HttpStatusCode.BadRequest);

            response.Headers.AddOrUpdate("content-type", "application/json");

            response.WriteString(Json.Serialize(errorResponse));

            return response;
        }

        private static HttpResponseData CreateJsonRpcUnauthorizedResponse(JToken id, HttpRequestData httpRequest)
        {
            var response = CreateJsonRpcErrorResponse(id, (int)HttpStatusCode.Unauthorized, "MCP caller context is missing or invalid", null, httpRequest);
            response.StatusCode = HttpStatusCode.Unauthorized;
            return response;
        }

        private static HttpResponseData CreateJsonRpcSuccessfulResponse(JToken id, object result, HttpRequestData httpRequest)
        {
            var toolResponse = new Response
            {
                Jsonrpc = "2.0",
                Result = JToken.FromObject(result),
                Id = id
            };
            var response = httpRequest.CreateResponse(HttpStatusCode.OK);

            response.Headers.AddOrUpdate("content-type", "application/json");

            response.WriteString(Json.Serialize(toolResponse));

            return response;
        }

    }
}
