using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Reference.Application.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Triggers.Http
{
    public sealed class MobileFeaturesController
    {
        private readonly IMobileFeaturesQueryHandler _handler;

        public MobileFeaturesController(IMobileFeaturesQueryHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        [OpenApiOperation(operationId: "MobileFeatures", tags: new[] { "Reference data Microservice" }, Summary = "Fetch reference data for MobileFeatures", Description = "Endpoint to fetch reference data of MobileFeatures", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(MobileFeatures), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("MobileFeatures")]
        public async Task<HttpResponseData> MobileFeatures([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "MobileFeatures")] HttpRequestData req, CancellationToken cancellationToken)
        {
            return await ResultExtention<MobileFeatures>.ExecuteResultAsync(req, Result.Success(new Response<MobileFeatures>
            {
                Status = Status.OK,
                Value = await _handler.HandleAsync(cancellationToken),
            }));
        }
    }
}
