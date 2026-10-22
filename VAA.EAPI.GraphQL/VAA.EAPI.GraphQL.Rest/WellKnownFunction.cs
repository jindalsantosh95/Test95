using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Newtonsoft.Json.Linq;
using System.Net;

namespace VAA.EAPI.GraphQL.Rest
{
    public sealed class WellKnownFunction
    {
        [OpenApiOperation(operationId: "ProtectedResourceMetadata", tags: new[] { "ProtectedResourceMetadata" }, Visibility = OpenApiVisibilityType.Important, Description = "Provides metadata about the protected resource.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(JObject), Summary = "OK", Description = "OK")]
        [Function("ProtectedResourceMetadata")]
        public HttpResponseData GetProtectedResourceMetadata([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ".well-known/oauth-protected-resource/public/mcp")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);

            response.Headers.Add("content-type", "application/json");

            response.WriteString("""
            {
                "resource": "https://your-server.com/mcp",
                "authorization_server": "https://auth.your-server.com",
                "scopes_supported": ["mcp:tools", "mcp:resources"]
            }
            """);

            return response;
        }

        [OpenApiOperation(operationId: "AuthorizationServerMetadata", tags: new[] { "AuthorizationServerMetadata" }, Visibility = OpenApiVisibilityType.Important, Description = "Provides metadata about the authorization server.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(JObject), Summary = "OK", Description = "OK")]
        [Function("AuthorizationServerMetadata")]
        public HttpResponseData GetAuthorizationServerMetadata([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ".well-known/oauth-authorization-server")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);

            response.Headers.Add("content-type", "application/json");

            response.WriteString("""
            {
                "issuer": "https://auth.your-server.com",
                "authorization_endpoint": "https://auth.your-server.com/authorize",
                "token_endpoint": "https://auth.your-server.com/token",
                "code_challenge_methods_supported": ["S256"]
            }
            """);

            return response;
        }
    }
}
