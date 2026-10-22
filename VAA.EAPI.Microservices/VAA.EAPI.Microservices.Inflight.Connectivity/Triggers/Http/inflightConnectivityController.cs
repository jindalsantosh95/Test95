using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Microservices.Inflight.Connectivity.Domain;
using VAA.EAPI.Microservices.Inflight.Connectivity.Interfaces;

namespace VAA.EAPI.Microservices.Inflight.Connectivity.Triggers.Http
{
    public sealed class InflightConnectivityController
    {
        private readonly IInflightConnectivityHandler _inflightConnectivityHandler;

        public InflightConnectivityController(IInflightConnectivityHandler inflightConnectivityHandler)
        {
            _inflightConnectivityHandler = inflightConnectivityHandler ?? throw new ArgumentNullException(nameof(inflightConnectivityHandler));
        }

        [OpenApiOperation(operationId: "EnableWifi", tags: new[] { "EnableInflightWifi" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("application/json", typeof(InflightConnectivityRequest), Required = true, Description = "Inflight connectivity request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Client unsandbox", Description = "Unsandbox Client in Starlink")]
        [Function("EnableInflightWifi")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "connectivity/EnableInflightWifi")] HttpRequestData req,
            CancellationToken cancellationToken)
        {
            var request = await req.ReadFromJsonAsync<InflightConnectivityRequest>(cancellationToken);
            if (request is null || string.IsNullOrWhiteSpace(request.DeviceId) || string.IsNullOrWhiteSpace(request.SandboxId))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("deviceId and sandboxId are required", cancellationToken);
                return badResponse;
            }

            var result = await _inflightConnectivityHandler.RegisterClientAsync(request, cancellationToken);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result, cancellationToken);

            return response;
        }
    }
}