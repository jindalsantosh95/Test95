using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MediatR;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.LoungeManagement.SkyTeam.Application.Query;
using VAA.EAPI.LoungeManagement.SkyTeam.Domain;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Net.Mime;
using System.Net;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.LoungeManagement.SkyTeam.Validations;

namespace VAA.EAPI.LoungeManagement.SkyTeam.Trigger.Http
{
    public class SkyTeamLoungeController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SkyTeamLoungeController> _logger;

        public SkyTeamLoungeController(IMediator mediator, ILogger<SkyTeamLoungeController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [OpenApiOperation(operationId: "getticket", tags: new[] { "Order ticket µService" }, Summary = "Provides ability to Retrieve tickets", Description = "Http endpoint to ticket PNR.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(RetrieveTicketDetailsRequest), Required = true, Description = "This should contain the request information for the Ticket Details.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RetrieveTicketDetailsResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]

        [Function("getticket")]
        public async Task<HttpResponseData> SkyTeamFetchTicketData([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "getticket")] HttpRequestData req, FunctionContext executionContext)
        {

            var skyTeamMemberTicketDetailsRequest = Json.Deserialize<RetrieveTicketDetailsRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            var ticketDetailsRequestValidation = new TicketDetailsRequestValidator().Validate(skyTeamMemberTicketDetailsRequest);

            if (ticketDetailsRequestValidation.IsValid)
            {
                try
                {
                    var result = await _mediator.Send(new RetriveTicketQuery() { RetrieveTicketDetailsRequest = skyTeamMemberTicketDetailsRequest }, CancellationToken.None);
                    return await ResultExtention<RetrieveTicketDetailsResponse>.ExecuteResultAsync(req, result);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, $"SkyTeamFetchTicketData- failed");
                    throw;
                }
            }
            else
            {
                var errorList = new List<ErrorDetails>();
                foreach (var item in ticketDetailsRequestValidation.Errors)
                {
                    errorList.Add(new ErrorDetails { Message = item.ErrorMessage, ErrorSource = item.PropertyName });

                }
                var errorResponse = new Response<RetrieveTicketDetailsResponse>()
                {
                    Status = Status.BadRequest,
                    Errors = errorList,
                };

                return await ResultExtention<RetrieveTicketDetailsResponse>.ExecuteResultAsync(req, errorResponse);

            }
        }
    }
}
