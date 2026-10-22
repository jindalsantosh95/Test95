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
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Http
{
    public class LoyaltyMemberManagementController
    {
        private readonly ILogger<LoyaltyMemberManagementController> _logger;
        private readonly IMediator _mediator;

        public LoyaltyMemberManagementController(IMediator mediator, ILogger<LoyaltyMemberManagementController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "GetLoyaltyMember", tags: new[] { "Loyalty Member Management Microservice - MemberAccountDetails" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MemberAccountResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetLoyaltyMember")]
        public async Task<HttpResponseData> GetLoyaltyMember(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetLoyaltyMember/{customerId}")] HttpRequestData req, string customerId)
        {
            _logger.LogInformation("Loyalty Member Management process started");
            var commandOutput = await _mediator.Send(new MemberAccountDetailsQuery { CustomerId = customerId });
            return await ResultExtention<MemberAccountResponse>.ExecuteResultAsync(req, commandOutput);
        }
        [OpenApiOperation(operationId: "PromotionEnrolledMembers", tags: new[] { "Loyalty Member Management Microservice - PromotionEnrolledMembers" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "promotionId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Promotion ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("PromotionEnrolledMembers")]
        public async Task<HttpResponseData> PromotionEnrolledMembers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "PromotionEnrolledMembers/{promotionId}")] HttpRequestData req, string promotionId)
        {
            _logger.LogInformation("Loyalty Member Management - PromotionEnrolledMembers process started");
            var commandOutput = await _mediator.Send(new PromotionEnrolledMemberDetailsQuery { PromotionId = promotionId });
            return await ResultExtention<PromotionEnrolledMemberDetailsResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
