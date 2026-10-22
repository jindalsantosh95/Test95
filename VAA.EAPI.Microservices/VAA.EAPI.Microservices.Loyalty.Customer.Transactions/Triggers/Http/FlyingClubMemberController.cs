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
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http
{
    public class FlyingClubMemberController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FlyingClubMemberController> _logger;
        public FlyingClubMemberController(IMediator mediator, ILogger<FlyingClubMemberController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "FlyingClubMemberDocument", tags: new[] { "Customer Transactions Microservice - FlyingClubMemberDocument" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(FlyingClubMemberResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("FlyingClubMemberDocumentByCustomerID")]
        public async Task<HttpResponseData> FetchFlyingClubMemberDocumentByCustomerIdAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "flyingClubMember/{customerID}")] HttpRequestData request, string customerID)
        {
            _logger.LogInformation("Request to Get Flying Club Member Details");
            var commandOutput = await _mediator.Send(new FlyingClubMemberQuery() { CustomerId = customerID, DocType = ConfigReader.Get(EnvironmentSettings.FlyingClubMemberDocType) });
            return await ResultExtention<FlyingClubMemberResponse>.ExecuteResultAsync(request, commandOutput);
        }

        [OpenApiOperation(operationId: "FCMDocBySSAMemberNumber", tags: new[] { "Customer Transactions Microservice - FlyingClubMemberDocument" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(FlyingClubMemberResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "ssaMemberNumber", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "SSAMemberNumber")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("FCMDocBySSAMemberNumber")]
        public async Task<HttpResponseData> FCMDocBySSAMemberNumberAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "flyingClubMember/supplementarySilverAccount/{ssaMemberNumber}")] HttpRequestData request, string ssaMemberNumber)
        {
            var commandOutput = await _mediator.Send(new FCMDocBySSAMemberNumberQuery(ssaMemberNumber));
            return await ResultExtention<FlyingClubMemberResponse>.ExecuteResultAsync(request, commandOutput);
        }

        [OpenApiOperation(operationId: "PatchFlyingClubMember", tags: new[] { "Customer Transactions Microservice - FlyingClubMemberDocument" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(FlyingClubMember), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("PatchFlyingClubMember")]
        public async Task<HttpResponseData> PatchFlyingClubMember([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "{customerID}/flyingClubMember")] HttpRequestData request, string customerID)
        {
            FlyingClubMemberPatchCommand command = await TransactionBaseCommand.MapToCommand<FlyingClubMemberPatchCommand>(request, customerID);

            return await ResultExtention<FlyingClubMemberCommandResponse>.ExecuteResultAsync(request, await _mediator.Send(command));
        }

        [OpenApiOperation(operationId: "GetStatusOfFlyingClubMemberCommand", tags: new[] { "Customer Transactions Microservice - FlyingClubMemberDocument" }, Summary = "Get flying club member document event response", Description = "It returns flying club member document event response and status against requested correlationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(FlyingClubMemberCommandQueryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "requestId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Command Correlation ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetStatusOfFlyingClubMemberCommand")]
        public async Task<HttpResponseData> GetStatusOfFlyingClubMemberCommand([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "FlyingClubMembers/status/{requestId}")] HttpRequestData req, string requestId)
        {
            var commandOutput = await _mediator.Send(new FlyingClubMemberCommandStatusQuery() { RequestId = requestId });

            return await ResultExtention<FlyingClubMemberCommandQueryResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}