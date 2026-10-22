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
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.GroupDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Group;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http
{
    public class GroupDocumentController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GroupDocumentController> _logger;
        public GroupDocumentController(IMediator mediator,
            ILogger<GroupDocumentController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "GroupByGroupID", tags: new[] { "Customer Transactions Microservice - GroupDocument" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(GroupDocumentResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "groupId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Group ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GroupByGroupID")]
        public async Task<HttpResponseData> GroupDocumentByGroupIdAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "group/{groupId}")] HttpRequestData request, string groupId)
        {
            _logger.LogInformation("GroupDocumentController - GroupDocumentByGroupIdAsync - Start ");
            try
            {
                var commandOutput = await _mediator.Send(new GroupDocQuery(groupId));
                return await ResultExtention<GroupDocumentResponse>.ExecuteResultAsync(request, commandOutput);
            }
            finally
            {
                _logger.LogInformation("GroupDocumentController - GroupDocumentByGroupIdAsync - End ");
            }
        }

        [OpenApiOperation(operationId: "GroupbyCustomerId", tags: new[] { "Customer Transactions Microservice - GroupDocument" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(GroupDocumentResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Head of the Group Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GroupbyCustomerId")]
        public async Task<HttpResponseData> GroupDocumentsByHeadCustomerIdAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "group/customer/{customerId}")] HttpRequestData request, string customerId)
        {
            _logger.LogInformation("GroupDocumentController - GroupDocumentsByHeadCustomerId - Start ");
            try
            {
                var commandOutput = await _mediator.Send(new GroupDocByHeadCustomerIdQuery(customerId));
                return await ResultExtention<GroupDocumentResponse>.ExecuteResultAsync(request, commandOutput);
            }
            finally
            {
                _logger.LogInformation("GroupDocumentController - GroupDocumentsByHeadCustomerId - End ");
            }
        }

        [OpenApiOperation(operationId: "GroupHistory", tags: new[] { "Customer Transactions Microservice - GroupDocument" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(GroupDocumentResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GroupHistory")]
        public async Task<HttpResponseData> GroupDocumentFromGroupHistoryCustomerIdAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "groupHistory/{customerId}")] HttpRequestData request, string customerId)
        {
            _logger.LogInformation("GroupDocumentController - GroupDocumentsFromGroupHistoryCustomerId - Start ");
            try
            {
                var commandOutput = await _mediator.Send(new GroupDocFromHistoryByCustomerIdQuery(customerId));
                return await ResultExtention<GroupDocumentResponse>.ExecuteResultAsync(request, commandOutput);
            }
            finally
            {
                _logger.LogInformation("GroupDocumentController - GroupDocumentsFromGroupHistoryCustomerId - End ");
            }
        }
    }
}
