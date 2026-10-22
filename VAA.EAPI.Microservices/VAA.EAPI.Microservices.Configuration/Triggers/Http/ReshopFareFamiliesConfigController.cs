using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Configuration.Application.QueriesAndHandlers;
using VAA.EAPI.Microservices.Configuration.Domain;

namespace VAA.EAPI.Microservices.Configuration.Triggers.Http
{
    public class ReshopFareFamiliesConfigController
    {
        private readonly IMediator _mediator;        

        public ReshopFareFamiliesConfigController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));            
        }

        [OpenApiOperation(operationId: "Reshop FareFamilies Lookup", tags: new[] { "Reshop FareFamilies Lookup" }, Summary = "This endpoint search the reshop fare families associated with existing fare family", Description = "This endpoint search the reshop fare families associated with existing fare family", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<Reshop_FareFamily>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("ReshopFareFamiliesConfigController")]
        public async Task<HttpResponseData> ReshopFareFamilies([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Configurations/ReshopFareFamily")] HttpRequestData req)
        {            
            var commandOutput = await _mediator.Send(new ReshopFareFamilyDBQuery(), CancellationToken.None);
            return await ResultExtention<IEnumerable<Reshop_FareFamily>>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
