using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
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
using VAA.EAPI.Infrastructure.Entities;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http
{
    public class CustomerAssignAwardControllerV2
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerAssignAwardControllerV2> _logger;

        public CustomerAssignAwardControllerV2(IMediator mediator, ILogger<CustomerAssignAwardControllerV2> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "AssignAwardV2", tags: new[] { "Customer Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(LoyaltyCustomerAssignAwardCommand), Description = "")]
        [OpenApiParameter(name: "memberid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Member id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [RoleAuthorization("Super", "Intermediate", "Light")]
        [Function("LoyaltyCustomerAssignAward")]
        public async Task<HttpResponseData> LoyaltyCustomerAssignAward([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customer/loyalty/{memberid}/assignaward")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var assignAwardRequest = Json.Deserialize<LoyaltyCustomerAssignAwardCommand>(request);
            if (!string.Equals(assignAwardRequest.ProductName, Constants.Application.TopMember))
            {
                return await ResultExtention<string>.ExecuteResultAsync(req, new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.TopMemberErrorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)

                });
            }
            return await ResultExtention<string>.ExecuteResultAsync(req, await _mediator.Send(assignAwardRequest));
        }

    }
}
