using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Application.Query.VSRetro;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.VSRetro;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Triggers.Http
{
    public class PartnerVSRetroController
    {
        private readonly IMediator _mediator;

        public PartnerVSRetroController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "GetVSFlownDetails", tags: new[] { "Partner data Management Microservice - Get VS Flown Details by Ticket Number" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "ticketNumber", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "TicketNumber")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(VSFlownDetailsResponse), Summary = "Get VS Flown Details", Description = "Get VS Flown Details by Ticket Number for missing points claim")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        [Function("GetVSFlownDetails")]
        public async Task<HttpResponseData> GetVSFlownDetailsAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getVSFlownDetails/{ticketNumber}")] HttpRequestData request, string ticketNumber)
        {
            var commandOutput = await _mediator.Send(new VSRetroFlownQuery(ticketNumber));
            return await ResultExtention<VSFlownDetailsResponse>.ExecuteResultAsync(request, commandOutput);
        }

        [OpenApiOperation(operationId: "GetVSRetroFlownDetails", tags: new[] { "Partner data Management Microservice - Get VS Retro Flown Details by ticketNumber, pnr, flightNumber" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(VSRetroSearchRequest), Required = true, Description = "Provide either ticketNumber OR (pnr + flightNumber).")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(VSFlownDetailsResponse), Summary = "Get VS Retro Flown Details", Description = "Get VS Retro Flown Details by ticketNumber, pnr, flightNumber for missing points claim")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetVSRetroFlownDetails")]
        public async Task<HttpResponseData> GetVSRetroFlownDetailsAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "getVSRetroFlownDetails")] HttpRequestData request)
        {
            var requestBody = await new StreamReader(request.Body).ReadToEndAsync();

            VSRetroSearchRequest searchRequest;
            if (requestBody.IsNullOrWhiteSpace())
            {
                searchRequest = new VSRetroSearchRequest();
            }
            else
            {
                try
                {
                    searchRequest = Json.Deserialize<VSRetroSearchRequest>(requestBody) ?? new VSRetroSearchRequest();
                }
                catch
                {
                    searchRequest = new VSRetroSearchRequest();
                }
            }

            var commandOutput = await _mediator.Send(new VSRetroFlownDetailsQuery(searchRequest));
            return await ResultExtention<VSFlownDetailsResponse>.ExecuteResultAsync(request, commandOutput);
        }

        [OpenApiOperation(operationId: "UpdateTicketStatus", tags: new[] { "Partner data Management Microservice - Update Ticket status from FREE to Allocate" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(TicketStatusRequest), Description = "this parameters to update ticket status")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Update Ticket status", Description = "Update Ticket status from FREE to Allocate after successful point claim")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("UpdateTicketStatus")]
        public async Task<HttpResponseData> UpdateTicketStatus([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "UpdateTicketStatus")] HttpRequestData request)
        {
            var commandOutput = await _mediator.Send(new UpdateVATicketStatusCommand(Json.Deserialize<TicketStatusRequest>(await new StreamReader(request.Body).ReadToEndAsync())));

            return await ResultExtention<string>.ExecuteResultAsync(request, commandOutput);
        }

        [OpenApiOperation(operationId: "UpdateVSRetroFlownDetails", tags: new[] { "Partner data Management Microservice - Update VS Retro flown details" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(UpdateVSFlownDetailsRequest), Required = true, Description = "Origin, destination, ticket, status, and loyalty details to update.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UpdateVSFlownDetailsResponse), Summary = "Update VS Retro flown details", Description = "Updates the matching VS Retro flown record.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Invalid request", Description = "One or more required inputs are missing.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("UpdateVSRetroFlownDetails")]
        public async Task<HttpResponseData> UpdateVSRetroFlownDetailsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "updateVSRetroFlownDetails")] HttpRequestData request,
            CancellationToken cancellationToken)
        {
            var updateRequest = await request.ParseAndValidateRequestAsync<UpdateVSFlownDetailsRequest>(true);

            var commandOutput = await _mediator.Send(new UpdateVSRetroFlownDetailsCommand(updateRequest), cancellationToken);

            return await ResultExtention<UpdateVSFlownDetailsResponse>.ExecuteResultAsync(request, commandOutput);
        }

        [OpenApiOperation(operationId: "ValidateVSClaimInputs", tags: new[] { "Partner data Management Microservice - Validate VS claim inputs" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(ValidateVSClaimInputsRequest), Required = true, Description = "Ticket designator, fare basis, and passenger type to validate for a VS retro claim.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ValidateVSClaimInputsResponse), Summary = "Validate VS claim inputs", Description = "Reports whether the VS claim inputs are eligible for accrual.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Summary = "Internal Server Error", Description = "Unable to load validation codes from storage.")]
        [Function("ValidateVSClaimInputs")]
        public async Task<HttpResponseData> ValidateVSClaimInputsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ValidateVSClaimInputs")] HttpRequestData request,
            CancellationToken cancellationToken)
        {
            var requestBody = await new StreamReader(request.Body).ReadToEndAsync(cancellationToken);
            ValidateVSClaimInputsRequest validationRequest;

            try
            {
                validationRequest = requestBody.IsNullOrWhiteSpace()
                    ? new ValidateVSClaimInputsRequest()
                    : Json.Deserialize<ValidateVSClaimInputsRequest>(requestBody) ?? new ValidateVSClaimInputsRequest();
            }
            catch
            {
                validationRequest = new ValidateVSClaimInputsRequest();
            }

            var commandOutput = await _mediator.Send(new ValidateVSClaimInputsCommand(validationRequest), cancellationToken);
            return await ResultExtention<ValidateVSClaimInputsResponse>.ExecuteResultAsync(request, commandOutput);
        }
    }
}
