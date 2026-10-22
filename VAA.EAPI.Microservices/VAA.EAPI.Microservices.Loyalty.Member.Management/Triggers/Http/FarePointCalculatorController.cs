using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using Microsoft.Azure.Functions.Worker.Http;
using System.IO;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FarePointCalc;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Http
{
    public class FarePointCalculatorController
    {
        private readonly IMediator _mediator;

        public FarePointCalculatorController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("RewardBookingPointCalculation")]
        [OpenApiOperation(operationId: "PointCalculation", tags: new[] { "Loyalty Member Management Microservice - Calculate reward booking points from PNR Details" }, Summary = "A new query for reward booking point calulation", Description = "http endpoint to Calculate reward booking points from PNR Details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(FareCalculationRequest), Description = "Link Account Details")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(FareCalculationResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        public async Task<HttpResponseData> RewardBookingPointCalculation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "RewardBookingPointCalculation")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();

            var commandOutput = await _mediator.Send(new FareCalculationQuery { FareCalculationRequest = Json.Deserialize<FareCalculationRequest>(request) });

            return await ResultExtention<FareCalculationResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
