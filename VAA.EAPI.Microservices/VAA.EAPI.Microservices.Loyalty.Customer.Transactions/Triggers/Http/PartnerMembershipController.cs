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
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.PartnerMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.PartnerMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http
{
    public class PartnerMembershipController
    {
        private readonly IMediator _mediator;

        public PartnerMembershipController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "PartnerMembershipDocumentByCustomerId", tags: new[] { "Customer Transactions Microservice - Partner Membership Document" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PartnerMembershipResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "CustomerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetPartnerMembershipByCustomerId")]
        public async Task<HttpResponseData> GetPartnerMembershipByCustomerId([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "{CustomerID}/PartnerMembership")] HttpRequestData request, string customerID)
        {
            return await ResultExtention<PartnerMembershipResponse>.ExecuteResultAsync(request, await _mediator.Send(new PartnerMembershipQuery() { CustomerId = customerID, DocType = ConfigReader.Get(EnvironmentSettings.PartnerMembershipDocType) }));
        }

        [OpenApiOperation(operationId: "CreatePartnerMembership", tags: new[] { "Customer Transactions Microservice - Partner Membership" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(PartnerMembership), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CreatePartnerMembership")]
        public async Task<HttpResponseData> CreatePartnerMembership([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{customerID}/createPartnerMembership")] HttpRequestData req, string customerID)
        {
            PartnerMembershipCreateCommand command = await TransactionBaseCommand.MapToCommand<PartnerMembershipCreateCommand>(req, customerID);
            return await ResultExtention<PartnerMembershipCommandResponse>.ExecuteResultAsync(req, await _mediator.Send(command));
        }

        [OpenApiOperation(operationId: "PatchPartnerMembership", tags: new[] { "Customer Transactions Microservice - Partner Membership" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(PartnerMembership), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("PatchPartnerMembership")]
        public async Task<HttpResponseData> PatchPartnerMembership([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "{customerID}/updatePartnerMembership")] HttpRequestData req, string customerID)
        {
            PartnerMembershipPatchCommand command = await TransactionBaseCommand.MapToCommand<PartnerMembershipPatchCommand>(req, customerID);
            return await ResultExtention<PartnerMembershipCommandResponse>.ExecuteResultAsync(req, await _mediator.Send(command));
        }

        [OpenApiOperation(operationId: "GetStatusOfPartnerMembershipCommand", tags: new[] { "Customer Transactions Microservice - Partner Membership" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(PartnerMembership), Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PartnerMembershipCommandQueryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "requestId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Command Correlation ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetStatusOfPartnerMembershipCommand")]
        public async Task<HttpResponseData> GetStatusOfPartnerMembershipCommand([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "PartnerMemberships/status/{requestId}")] HttpRequestData req, string requestId)
        {
            var commandOutput = await _mediator.Send(new PartnerMembershipCommandStatusQuery() { RequestId = requestId });
            return await ResultExtention<PartnerMembershipCommandQueryResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
