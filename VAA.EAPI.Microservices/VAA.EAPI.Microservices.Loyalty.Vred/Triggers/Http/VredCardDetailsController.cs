using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
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
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredCardDetails;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Triggers.Http
{
    public class VredCardDetailsController
    {
        private readonly IMediator _mediator;

        public VredCardDetailsController(IMediator mediator)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "GetVredCardLinksByMemberNumber", tags: new[] { "VRED Microservice" }, Summary = "Retrieve Vred Card Links.", Description = "Returns Vred Card Links of customer based on the Member Number.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "memberNumber", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "This field specifies the member number.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(VredCardDetailsResponse), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        [Function("GetVredCardLinksByMemberNumber")]
        public async Task<HttpResponseData> GetVredCardDetailsByMemberID([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "VredCardLinks/{memberNumber}")] HttpRequestData req, string memberNumber)
        {
            return await ResultExtention<VredCardDetailsResponse>.ExecuteResultAsync(req, await _mediator.Send(new VredCardDetailsQuery(memberNumber), CancellationToken.None));
        }
    }
}
