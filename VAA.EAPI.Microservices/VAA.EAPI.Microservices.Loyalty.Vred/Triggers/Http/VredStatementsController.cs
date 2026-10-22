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
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredStatements;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Triggers.Http
{
    public class VredStatementsController
    {
        private readonly ILogger<VredStatementsController> logger;
        private readonly IMediator _mediator;

        public VredStatementsController(IMediator mediator, ILogger<VredStatementsController> logger)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "GetVredStatementsByMemberNumber", tags: new[] { "VRED Microservice" }, Summary = "Retrieve Vred statements.", Description = "Returns Vred statements of customer when logged in based on the FC Number.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "memberNumber", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "This field specifies the Frequent flyer number.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(VredStatementResponse), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        [Function("GetVredStatementsByMember")]
        public async Task<HttpResponseData> GetVirginPointsByMemberID([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "VredStmts/{memberNumber}")] HttpRequestData req, string memberNumber)
        {
            logger.LogInformation($"Get Vred Statements for a given Member: {memberNumber}");

            return await ResultExtention<VredStatementResponse>.ExecuteResultAsync(req, await _mediator.Send(new VredStatementsQuery(memberNumber), CancellationToken.None));
        }

    }
}
