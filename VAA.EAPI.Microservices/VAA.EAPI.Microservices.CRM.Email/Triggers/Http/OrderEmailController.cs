using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.CRM.Email.Application.Command;
using VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM;

namespace VAA.EAPI.Microservices.CRM.Email.Triggers.Http
{
    public class OrderEmailController
    {
        private readonly IMediator _mediator;

        public OrderEmailController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("OrderEmail")]
        [OpenApiOperation(operationId: "OrderEmail", tags: new[] { "Email Microservice" }, Summary = "Sends order email to agents", Description = "Sends an order email to agents", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(AdobeMemberInviteEmailCommand), Description = "Sends order email to agents")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: "application/json", bodyType: typeof(string), Summary = "Request accepted", Description = "Sends order email to agents")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to perform this operation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "The request data failed business validation rules")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "An unexpected error occurred while processing the request")]
        public async Task OrderEmail([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "OrderEmail")] HttpRequestData req)
        {
            await _mediator.Send(new OrderEmailCommand() { Email = req.Headers?.GetValues("Email").FirstOrDefault(), RecordLocator = req.Headers?.GetValues("RecordLocator").FirstOrDefault() }, CancellationToken.None);
        }
    }
}
