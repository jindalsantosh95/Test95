using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ContactNotes;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.ContactNotes;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ContactNotes;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http
{
    public class ContactNotesController
    {

        private readonly IMediator _mediator;
        
        public ContactNotesController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "GetContactNotesByCustomerID", tags: new[] { "Customer Transactions Microservice - Contact Notes" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ContactNotesResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetContactNotesByCustomerID")]
        public async Task<HttpResponseData> GetContactNotesByCustomerID([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerID}/contactnotes")] HttpRequestData req, string customerID)
        {
            var commandOutput = await _mediator.Send(new ContactNotesQuery() { CustomerId = customerID, DocType = ConfigReader.Get(EnvironmentSettings.ContactNoteDocType) });
            return await ResultExtention<ContactNotesResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "CreateContactNote", tags: new[] { "Customer Transactions Microservice - Contact Notes" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(ContactNote), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CreateContactNote")]
        public async Task<HttpResponseData> CreateContactNote([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{customerID}/contactnotes")] HttpRequestData req, string customerID)
        {
            ContactNoteCreateCommand command = await TransactionBaseCommand.MapToCommand<ContactNoteCreateCommand>(req, customerID);
            return await ResultExtention<ContactNoteCommandResponse>.ExecuteResultAsync(req, await _mediator.Send(command));
        }

        [OpenApiOperation(operationId: "DeleteContactNote", tags: new[] { "Customer Transactions Microservice - Contact Notes" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(ContactNote), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Document id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("DeleteContactNote")]
        public async Task<HttpResponseData> DeleteContactNote([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "{customerID}/contactnotes/{id}")] HttpRequestData req, string customerID, string id)
        {
            ContactNoteDeleteCommand command = await TransactionBaseCommand.MapToCommand<ContactNoteDeleteCommand>(req, customerID);
            command.ContactNote = new ContactNote { Id= id };
            
            return await ResultExtention<ContactNoteCommandResponse>.ExecuteResultAsync(req, await _mediator.Send(command));
        }

        [OpenApiOperation(operationId: "PatchContactNote", tags: new[] { "Customer Transactions Microservice - Contact Notes" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(ContactNote), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("PatchContactNote")]
        public async Task<HttpResponseData> PatchContactNote([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "{customerID}/contactnotes")] HttpRequestData req, string customerID)
        {
            ContactNotePatchCommand command = await TransactionBaseCommand.MapToCommand<ContactNotePatchCommand>(req, customerID);
            return await ResultExtention<ContactNoteCommandResponse>.ExecuteResultAsync(req, await _mediator.Send(command));
        }

        [OpenApiOperation(operationId: "GetStatusOfContactNoteCommand", tags: new[] { "Customer Transactions Microservice - Contact Notes" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ContactNoteCommandQueryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "requestId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Command Correlation ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetStatusOfContactNoteCommand")]
        public async Task<HttpResponseData> GetStatusOfContactNoteCommand([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "contactnotes/status/{requestId}")] HttpRequestData req, string requestId)
        {
            var commandOutput = await _mediator.Send(new ContactNoteCommandStatusQuery() { RequestId = requestId });
            return await ResultExtention<ContactNoteCommandQueryResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}


