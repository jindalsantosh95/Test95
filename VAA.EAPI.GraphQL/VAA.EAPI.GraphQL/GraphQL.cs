using HotChocolate.Execution;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL
{
    public sealed class GraphQL
    {
        private readonly Gateway _gateway;
        private readonly IRequestExecutorResolver _resolver;
        private readonly ILogger<GraphQL> _logger;

        public GraphQL(Gateway gateway, IRequestExecutorResolver resolver, ILogger<GraphQL> logger)
        {
            _gateway = gateway ?? throw new System.ArgumentNullException(nameof(gateway));
            _resolver = resolver;
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "GetSchema", tags: new[] { "GetSchema" }, Visibility = OpenApiVisibilityType.Important, Description = "Retrieves the GraphQL schema.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(JObject), Summary = "OK", Description = "OK")]
        [Function("GetSchema")]
        public async Task<HttpResponseData> GetSchema([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/GraphQL/Schema")] HttpRequestData req, FunctionContext executionContext)
        {
            var executor = await _resolver.GetRequestExecutorAsync();
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

            response.Headers.Add("Content-Type", "application/text");
            response.WriteString(executor.Schema.ToString(), System.Text.Encoding.UTF8);

            return response;
        }

        [OpenApiOperation(operationId: "GetData", tags: new[] { "GetData" }, Visibility = OpenApiVisibilityType.Important, Description = "Executes a GraphQL query and returns the result.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(JObject), Summary = "OK", Description = "OK")]
        [Function("GetData")]
        public async Task<HttpResponseData> GetData([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/GraphQL")] HttpRequestData req, FunctionContext executionContext) => await _gateway.ExecuteQueryAsync(req).ConfigureAwait(false);
    }
}
