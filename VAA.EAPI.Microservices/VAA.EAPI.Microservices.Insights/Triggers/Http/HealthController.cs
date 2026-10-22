using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Microservices.Insights.Application.Query;
using VAA.EAPI.Microservices.Insights.Domain;

namespace VAA.EAPI.Microservices.Insights.Triggers.Http
{
    public sealed class HealthController
    {
        private readonly HealthCheckService healthCheckService;
        private readonly IMediator mediator;
        private readonly ILogger<HealthController> logger;

        public HealthController(HealthCheckService healthCheckService, IMediator mediator, ILogger<HealthController> logger)
        {
            this.healthCheckService = healthCheckService;
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "GetCatalogue", tags: new[] { "HealthCheck" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ICollection<Endpoint>), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetCatalogue")]
        public async Task<HttpResponseData> GetCatalogue(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "catalogue")] HttpRequestData req)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string? environment = query["environment"];

            var response = await mediator.Send(new CatalogueRequest()
            {
                SubscriptionId = ConfigReader.Try<Guid>(Constants.Environment.ApimSubscriptionId),
                ApiVersion = ConfigReader.Try(Constants.Environment.ApimApiVersion, Constants.Application.APIMApiVersion),
                Environment = environment
            });

            return await ResultExtention<ICollection<Endpoint>>.ExecuteResultAsync(req, response);
        }


        [OpenApiOperation(operationId: "Heartbeat", tags: new[] { "HealthCheck" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HealthReport), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("Heartbeat")]
        public async Task<HttpResponseData> Heartbeat([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "heartbeat")] HttpRequestData req)
        {
            var healthReport = await healthCheckService.CheckHealthAsync();

            return await ResultExtention<HealthReport>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<HealthReport>()
            {
                Status = Status.OK,
                Value = healthReport,
            }));
        }

        [Function(nameof(Warmup))]
        public static void Warmup([WarmupTrigger] object warmupContext, FunctionContext context)
        {
        }
    }
}
