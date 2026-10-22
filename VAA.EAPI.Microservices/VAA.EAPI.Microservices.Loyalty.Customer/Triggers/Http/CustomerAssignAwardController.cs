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
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Infrastructure.Entities;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerAssignAward;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http
{
    public class CustomerAssignAwardController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerAssignAwardController> _logger;

        public CustomerAssignAwardController(IMediator mediator, ILogger<CustomerAssignAwardController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "AssignAward", tags: new[] { "Customer Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CustomerAssignAwardCommand), Description = "")]
        [OpenApiParameter(name: "memberid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Member id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [RoleAuthorization("Super", "Intermediate", "Light")]
        [Function("CustomerAssignAward")]
        public async Task<HttpResponseData> CustomerAssignAward([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customer/assignaward/{memberid}")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var cloudEvent = Json.Deserialize<CloudEvent>(request);
            var data = Json.Deserialize<AssignAwardRequestData>(Convert.ToString(cloudEvent.Data));
            if (!string.Equals(data.ProductName, Constants.Application.TopMember))
            {
                return await ResultExtention<string>.ExecuteResultAsync(req, new Response<string>()
                {
                    Status = Status.BusinessException,
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.TopMemberErrorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)

                });
            }
            return await ResultExtention<string>.ExecuteResultAsync(req, await _mediator.Send(new CustomerAssignAwardCommand { CloudEvent = cloudEvent }));
        }

    }
}
