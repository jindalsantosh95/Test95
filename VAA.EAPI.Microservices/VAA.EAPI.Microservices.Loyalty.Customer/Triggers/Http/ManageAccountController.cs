using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerDocCommand;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http
{
    public class ManageAccountController
    {
        private readonly IMediator _mediator;
        private readonly IUpdateMemberStatusCommand _updateMemberStatusCommand;
        private readonly CancellationToken _cancellationToken;
               
        public ManageAccountController(IMediator mediator, IUpdateMemberStatusCommand updateMemberStatusCommand)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _updateMemberStatusCommand = updateMemberStatusCommand ?? throw new ArgumentNullException(nameof(updateMemberStatusCommand));
            _cancellationToken = new CancellationToken();
        }

        [OpenApiOperation(operationId: "UpdateEmailAddress", tags: new[] { "Customer Profile API" }, Summary = "Update Customer's email Address.", Description = "Update Email address in OCDB, Azure B2C, VRED and Loops database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(UpdateEmailRequest), Required = true)]
        [OpenApiParameter(name: "memberNumber", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer's Flying Club Number.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Update Email Address", Description = "Update Email Address for Manage Account")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("UpdateEmailAddress")]
        public async Task<HttpResponseData> UpdateMembersEmailAddress([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "{memberNumber}/UpdateEmailAddress")] HttpRequestData req, string memberNumber)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var commandOutput = await _mediator.Send(new UpdateEmailCommand { MemberNumber = memberNumber, NewEmailAddress = Json.Deserialize<UpdateEmailRequest>(request).NewEmailAddress });

            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "UpdateMemberAddress", tags: new[] { "Customer Microservice" }, Summary = "Update Customer's Address.", Description = "Update Customer's Address.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(UpdateAddressRequest), Required = true)]
        [OpenApiParameter(name: "CustomerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Member's CustomerID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Update Address", Description = "Update Address of customer and  household linked Members if Leader")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        [Function("UpdateMemberAddress")]
        public async Task<HttpResponseData> UpdateMemberAddress([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "{CustomerId}/UpdateMemberAddress")] HttpRequestData req, string customerId)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();

            var commandOutput = await _mediator.Send(new UpdateAddressEventCommand { MemberId = customerId, UpdateAddressRequest = Json.Deserialize<UpdateAddressRequest>(request) });
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "UpdateMemberPhoneNumber", tags: new[] { "Customer Profile API" }, Summary = "Update Customer's Phone Number.", Description = "Update Customer's Phone Number in Loops database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(UpdatePhoneNumber), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Update Customer's Contact Number", Description = "Update Customer's Contact Number for Manage Account ")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("UpdateMemberPhoneNumber")]
        public async Task<HttpResponseData> UpdateMemberPhoneNumber([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "UpdateMemberPhoneNumber")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            return await ResultExtention<string>.ExecuteResultAsync(req, await _mediator.Send((UpdatePhoneNumberEventCommand)Json.Deserialize<UpdatePhoneNumber>(request)));
        }

        [OpenApiOperation(operationId: "BulkMemberStatusUpdate", tags: new[] { "Customer Profile API" }, Summary = "Update MembersStatusNotes", Description = "Update Customer's StatusNotes in Loops database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(BulkMemberStatusUpdateRequest), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Update MembersStatusNotes", Description = "Update MembersStatusNotes for Manage Account ")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("BulkMemberStatusUpdate")]
        public async Task<HttpResponseData> UpdateMemberStatus([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "BulkMemberStatusUpdate")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var bulkMemberStatusUpdateRequest = Json.Deserialize<BulkMemberStatusUpdateRequest>(request);
            var commandOutput = await _updateMemberStatusCommand.Handle(bulkMemberStatusUpdateRequest, _cancellationToken);
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);

    }


        [OpenApiOperation(operationId: "UpdateVerify", tags: new[] { "Customer Microservice" }, Summary = "Retrieve Manage Account Update Response.", Description = "Retrieve Manage Account Update Response.", Visibility = OpenApiVisibilityType.Important)]
[OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
[OpenApiParameter(name: "CorrelationId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation Id")]
[OpenApiParameter(name: "UpdateType", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "UpdateType")]
[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ManageAccountUpdateResponse), Summary = "Fetch Response for Manage Account Updates", Description = "Fetch Response for Manage Account Updates")]
[OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
[OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
[OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
[OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

[Function("UpdateResponse")]
public async Task<HttpResponseData> VerifyUpdate([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "UpdateResponse/{CorrelationId}/{UpdateType}")] HttpRequestData req, string CorrelationId, string UpdateType)
{
    Result<Response<ManageAccountUpdateResponse>> result = await _mediator.Send(new ManageAccountUpdateResponseCommand(CorrelationId, UpdateType), CancellationToken.None);
    return await ResultExtention<ManageAccountUpdateResponse>.ExecuteResultAsync(req, result);
}


[OpenApiOperation(operationId: "UpdateVirginAccountCustomer", tags: new[] { "Customer Profile API" }, Summary = "Update Virgin Account Customer's Personal Info.", Description = "Update Virgin Account Customer's Personal infomation in Loops database.", Visibility = OpenApiVisibilityType.Important)]
[OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
[OpenApiRequestBody("application/json", typeof(CustomerPersonalInfoUpdateCommand), Required = true)]
[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Update VAC member's account details", Description = "Update VAC member's account details for Manage Account")]
[OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
[OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
[OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
[OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
[Function("UpdateVirginAccountCustomer")]
public async Task<HttpResponseData> UpdateVirginAccountCustomer([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "UpdateVirginAccountCustomer")] HttpRequestData req)
{
    var request = await new StreamReader(req.Body).ReadToEndAsync();

    return await ResultExtention<string>.ExecuteResultAsync(req, await _mediator.Send(Json.Deserialize<CustomerPersonalInfoUpdateCommand>(request)));
}
    }
}