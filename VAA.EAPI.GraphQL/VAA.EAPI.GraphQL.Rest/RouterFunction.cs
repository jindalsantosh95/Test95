using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Rest.FunctionHost;

namespace VAA.EAPI.GraphQL.Rest
{
    public sealed class RouterFunction(Router router)
    {
        private readonly Router _router = router ?? throw new ArgumentNullException(nameof(router));

        [OpenApiOperation(operationId: "RouteGet", tags: new[] { "Route" }, Visibility = OpenApiVisibilityType.Important, Description = "Routes the request to the appropriate handler.")]
        [OpenApiParameter("queryMutationName", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The name of the query or mutation to execute.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [Function("RouteGet")]
        public async Task<HttpResponseData> RouteGet([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/{queryMutationName}")] HttpRequestData req, string queryMutationName)
            => await _router.Route(req, queryMutationName);

        [OpenApiOperation(operationId: "RoutePost", tags: new[] { "Route" }, Visibility = OpenApiVisibilityType.Important, Description = "Routes the request to the appropriate handler.")]
        [OpenApiParameter("queryMutationName", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The name of the query or mutation to execute.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [Function("RoutePost")]
        public async Task<HttpResponseData> RoutePost([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/{queryMutationName}")] HttpRequestData req, string queryMutationName)
            => await _router.Route(req, queryMutationName);

        [OpenApiOperation(operationId: "RouteOptionsIndex", tags: new[] { "Route" }, Visibility = OpenApiVisibilityType.Important, Description = "Returns the generated markdown capability index for AI consumption.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/markdown", bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [Function("RouteOptionsIndex")]
        public async Task<HttpResponseData> RouteOptionsIndex([HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "v1")] HttpRequestData req)
            => await _router.Route(req, null);

        [OpenApiOperation(operationId: "RouteOptionsByName", tags: new[] { "Route" }, Visibility = OpenApiVisibilityType.Important, Description = "Returns generated markdown documentation for the specified query or mutation.")]
        [OpenApiParameter("queryMutationName", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The name of the query or mutation to document.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/markdown", bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [Function("RouteOptionsByName")]
        public async Task<HttpResponseData> RouteOptionsByName([HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "v1/{queryMutationName}")] HttpRequestData req, string queryMutationName)
            => await _router.Route(req, queryMutationName);

        [OpenApiOperation(operationId: "ModelContextProtocolPost", tags: new[] { "ModelContextProtocol" }, Visibility = OpenApiVisibilityType.Important, Description = "Handles the Model Context Protocol requests.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [Function("ModelContextProtocolPost")]
        public async Task<HttpResponseData> ModelContextProtocolPost([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp")] HttpRequestData req)
            => await ModelContextProtocolHandler(req);

        private async Task<HttpResponseData> ModelContextProtocolHandler(HttpRequestData req)
        {
            var mcpResponse = await _router.Mcp(req);

            var response = req.CreateResponse(mcpResponse.Error is null ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest);

            response.Headers.AddOrUpdate("content-type", "application/json");

            response.WriteString(Json.Serialize(mcpResponse));

            return response;
        }
    }
}
