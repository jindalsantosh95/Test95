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
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.TargetedMemberships;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.TargetedMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.TargetedMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http
{
    public class TargetedMembershipController
    {

        private readonly IMediator _mediator;
        private readonly ILogger<TargetedMembershipController> _logger;
        public TargetedMembershipController(IMediator mediator, ILogger<TargetedMembershipController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "GetTargetedMembershipByCustomerID", tags: new[] { "Customer Transactions Microservice - Targeted Membership" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TargetedMembershipResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetTargetedMembershipByCustomerID")]
        public async Task<HttpResponseData> GetTargetedMembershipByCustomerID([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerID}/targetedmemberships")] HttpRequestData req, string customerID)
        {
            _logger.LogInformation("Request to Get Targeted Membership documents");
            var commandOutput = await _mediator.Send(new TargetedMembershipQuery() { CustomerId = customerID, DocType = ConfigReader.Get(EnvironmentSettings.TargetedMembershipDocType) });
            return await ResultExtention<TargetedMembershipResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "CreateTargetedMembership", tags: new[] { "Customer Transactions Microservice - Targeted Membership" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(TargetedMembership), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CreateTargetedMembership")]
        public async Task<HttpResponseData> CreateTargetedMembership([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{customerID}/targetedmemberships")] HttpRequestData req, string customerID)
        {
            _logger.LogInformation("Request to Create a Targeted Membership document");
            TargetedMembershipCreateCommand command = await TransactionBaseCommand.MapToCommand<TargetedMembershipCreateCommand>(req, customerID);
            return await ResultExtention<TargetedMembershipCommandResponse>.ExecuteResultAsync(req, await _mediator.Send(command));
        }

        [OpenApiOperation(operationId: "DeleteTargetedMembership", tags: new[] { "Customer Transactions Microservice - Targeted Membership" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(TargetedMembership), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("DeleteTargetedMembership")]
        public async Task<HttpResponseData> DeleteTargetedMembership([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "{customerID}/targetedmemberships")] HttpRequestData req, string customerID)
        {
            _logger.LogInformation("Request to Delete a Targeted Membership document");
            TargetedMembershipDeleteCommand command = await TransactionBaseCommand.MapToCommand<TargetedMembershipDeleteCommand>(req, customerID);
            return await ResultExtention<TargetedMembershipCommandResponse>.ExecuteResultAsync(req, await _mediator.Send(command));
        }

        [OpenApiOperation(operationId: "PatchTargetedMembership", tags: new[] { "Customer Transactions Microservice - Targeted Membership" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(TargetedMembership), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("PatchTargetedMembership")]
        public async Task<HttpResponseData> PatchTargetedMembership([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "{customerID}/targetedmemberships")] HttpRequestData req, string customerID)
        {
            _logger.LogInformation("Request to Patch a Targeted Membership document");
            TargetedMembershipPatchCommand command = await TransactionBaseCommand.MapToCommand<TargetedMembershipPatchCommand>(req, customerID);
            return await ResultExtention<TargetedMembershipCommandResponse>.ExecuteResultAsync(req, await _mediator.Send(command));
        }


        [OpenApiOperation(operationId: "GetStatusOfTargetedMembershipCommand", tags: new[] { "Customer Transactions Microservice - Targeted Membership" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TargetedMembershipCommandQueryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "requestId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Command Correlation ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetStatusOfTargetedMembershipCommand")]
        public async Task<HttpResponseData> GetStatusOfTargetedMembershipCommand([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "targetedmemberships/status/{requestId}")] HttpRequestData req, string requestId)
        {
            _logger.LogInformation($"Contact Note Command Status Request {requestId}");
            var commandOutput = await _mediator.Send(new TargetedMembershipCommandStatusQuery() { RequestId = requestId });
            return await ResultExtention<TargetedMembershipCommandQueryResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}


