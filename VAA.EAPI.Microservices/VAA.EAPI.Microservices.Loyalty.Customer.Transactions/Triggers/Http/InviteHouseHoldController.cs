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
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.InviteHouseHold;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.InviteHouseHold;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http
{
    public class InviteHouseHoldController
    {
        private readonly IMediator _mediator;
        public InviteHouseHoldController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "CreateHouseHoldInvite", tags: new[] { "Customer Transactions Microservice - Create HouseHold Invite" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(InviteHouseHoldCreateRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Create HouseHold Invite Document", Description = "Create HouseHold Invite Document during Household Account Creation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CreateHouseHoldInvite")]
        public async Task<HttpResponseData> CreateHouseHoldInviteAsync([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "CreateHouseHoldInvite")] HttpRequestData request)
        {
            var requsest = await new StreamReader(request.Body).ReadToEndAsync();
            return await ResultExtention<string>.ExecuteResultAsync(request, await _mediator.Send(new InviteHouseHoldCommand(Json.Deserialize<InviteHouseHoldCreateRequest>(requsest))));
        }

        [OpenApiOperation(operationId: "DeleteHouseHoldInvite", tags: new[] { "Customer Transactions Microservice - Delete HouseHold Invite" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(DeleteHouseHoldInvite))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Delete HouseHold Invite Document", Description = "Delete HouseHold Invite Document after Household account Creation or Rejection")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("DeleteHouseHoldInvite")]
        public async Task<HttpResponseData> DeleteHouseHoldInviteAsync([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "DeleteHouseHoldInvite")] HttpRequestData request)
        {
            return await ResultExtention<string>.ExecuteResultAsync(request, await _mediator.Send(new DeleteHouseHoldInviteCommand(Json.Deserialize<DeleteHouseHoldInvite>(await new StreamReader(request.Body).ReadToEndAsync()))));
        }

        [OpenApiOperation(operationId: "GetHouseHoldInviter", tags: new[] { "Customer Transactions Microservice - GetHouseHoldInviter" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(InviteHouseHoldResponse), Summary = "Fetch Household Invite Document", Description = "Fetch Household Invite Document with Inviter's member number")]
        [OpenApiParameter(name: "inviterMemberNumber", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "InviterMemberNumber")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetHouseHoldInviter")]
        public async Task<HttpResponseData> GetHouseHoldInviterAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getHouseHoldInviter/{inviterMemberNumber}")] HttpRequestData request, string inviterMemberNumber)
        {
            var commandOutput = await _mediator.Send(new GetHouseHoldInviterQuery(inviterMemberNumber));
            return await ResultExtention<InviteHouseHoldResponse>.ExecuteResultAsync(request, commandOutput);
        }

        [OpenApiOperation(operationId: "GetHouseHoldInvitee", tags: new[] { "Customer Transactions Microservice - GetHouseHoldInvitee" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(InviteHouseHoldResponse), Summary = "Fetch Household Invite Document", Description = "Fetch Household Invite Document with Invitee's member number")]
        [OpenApiParameter(name: "inviteeMemberNumber", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "InviteeMemberNumber")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetHouseHoldInvitee")]
        public async Task<HttpResponseData> GetHouseHoldInviteeAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getHouseHoldInvitee/{inviteeMemberNumber}")] HttpRequestData request, string inviteeMemberNumber)
        {
            var commandOutput = await _mediator.Send(new GetHouseHoldInviteeQuery(inviteeMemberNumber));
            return await ResultExtention<InviteHouseHoldResponse>.ExecuteResultAsync(request, commandOutput);
        }
    }
}
