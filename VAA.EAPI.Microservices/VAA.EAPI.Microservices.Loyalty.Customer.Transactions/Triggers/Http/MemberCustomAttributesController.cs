using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.MemberBenefit;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.MemberBenefit;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MemberBenefit;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http
{
    public class MemberCustomAttributesController
    {
        private readonly IMemberCustomAttributesQueryHandler _memberCustomAttributesQueryHandler;
        private readonly IMemberCustomAttributesCreateCommandHandler _memberCustomAttributesCreateCommandHandler;
        private readonly IMemberCustomAttributesPatchCommandHandler _memberCustomAttributesPatchCommandHandler;
        private readonly IMemberCustomAttributesStatusQueryHandler _memberCustomAttributesStatusQueryHandler;

        public MemberCustomAttributesController(IMemberCustomAttributesQueryHandler memberCustomAttributesQueryHandler, IMemberCustomAttributesCreateCommandHandler memberCustomAttributesCreateCommandHandler, IMemberCustomAttributesPatchCommandHandler memberCustomAttributesPatchCommandHandler, IMemberCustomAttributesStatusQueryHandler memberCustomAttributesStatusQueryHandler)
        {
            _memberCustomAttributesQueryHandler = memberCustomAttributesQueryHandler ?? throw new System.ArgumentNullException(nameof(memberCustomAttributesQueryHandler));
            _memberCustomAttributesCreateCommandHandler = memberCustomAttributesCreateCommandHandler ?? throw new System.ArgumentNullException(nameof(memberCustomAttributesCreateCommandHandler));
            _memberCustomAttributesPatchCommandHandler = memberCustomAttributesPatchCommandHandler ?? throw new System.ArgumentNullException(nameof(memberCustomAttributesPatchCommandHandler));
            _memberCustomAttributesStatusQueryHandler = memberCustomAttributesStatusQueryHandler ?? throw new System.ArgumentNullException(nameof(memberCustomAttributesStatusQueryHandler)); ;
        }

        [OpenApiOperation(operationId: "GetMemberCustomAttributes", tags: new[] { "Customer Transactions Microservice - Member Custom Attributes Document" }, Summary = "Retrieve MemberCustomAttributes", Description = "Http enpoint to retrieve MemberCustomAttributes for Member", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MemberCustomAttributesResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetMemberCustomAttributes")]
        public async Task<HttpResponseData> GetMemberCustomAttributes([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerID}/memberCustomAttributes")] HttpRequestData req, string customerID, CancellationToken cancellationToken)
        {
            var commandOutput = await _memberCustomAttributesQueryHandler.Handle(new MemberCustomAttributesQuery() { CustomerId = customerID, DocType = ConfigReader.Get(EnvironmentSettings.MemberCustomAttributesDocType) }, cancellationToken);
            return await ResultExtention<MemberCustomAttributesResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "CreateMemberCustomAttributes", tags: new[] { "Customer Transactions Microservice - Member Custom Attributes Document" }, Summary = "Create MemberCustomAttributes Document", Description = "Http enpoint to create memberCustomAttributes for Member", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(TransactionDocument), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CreateMemberCustomAttributes")]
        public async Task<HttpResponseData> CreateMemberCustomAttributes([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{customerID}/memberCustomAttributes")] HttpRequestData req, string customerID, CancellationToken cancellationToken)
        {
            MemberCustomAttributesCreateCommand command = await TransactionBaseCommand.MapToCommand<MemberCustomAttributesCreateCommand>(req, customerID);
            return await ResultExtention<MemberCustomAttributesCommandResponse>.ExecuteResultAsync(req, await _memberCustomAttributesCreateCommandHandler.Handle(command, cancellationToken));
        }

        [OpenApiOperation(operationId: "UpdateMemberCustomAttributes", tags: new[] { "Customer Transactions Microservice - Member Custom Attributes Document" }, Summary = "Update MemberCustomAttributes Document", Description = "Http enpoint to update memberCustomAttributes for Member", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(TransactionDocument), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("UpdateMemberCustomAttributes")]
        public async Task<HttpResponseData> UpdateMemberCustomAttributes([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "{customerID}/memberCustomAttributes")] HttpRequestData req, string customerID, CancellationToken cancellationToken)
        {
            MemberCustomAttributesPatchCommand command = await TransactionBaseCommand.MapToCommand<MemberCustomAttributesPatchCommand>(req, customerID);
            return await ResultExtention<MemberCustomAttributesCommandResponse>.ExecuteResultAsync(req, await _memberCustomAttributesPatchCommandHandler.Handle(command, cancellationToken));
        }

        [OpenApiOperation(operationId: "GetStatusOfMemberCustomAttributesCommand", tags: new[] { "Customer Transactions Microservice - Member Custom Attributes Document" }, Summary = "Get member custom attributes document event response", Description = "It returns member custom attributes document event response and status against requested correlationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MemberCustomAttributesCommandQueryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "requestId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Command Correlation ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetStatusOfMemberCustomAttributesCommand")]
        public async Task<HttpResponseData> GetStatusOfMemberCustomAttributesCommand([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "memberCustomAttributess/status/{requestId}")] HttpRequestData req, string requestId, CancellationToken cancellationToken)
        {
            var commandOutput = await _memberCustomAttributesStatusQueryHandler.Handle(new MemberCustomAttributesStatusQuery() { RequestId = requestId }, cancellationToken);

            return await ResultExtention<MemberCustomAttributesCommandQueryResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}