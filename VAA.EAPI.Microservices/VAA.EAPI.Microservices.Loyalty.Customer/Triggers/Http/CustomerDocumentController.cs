using CSharpFunctionalExtensions;
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
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.Account;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http
{
    public class CustomerDocumentController
    {
        private readonly ILogger<CustomerDocumentController> _logger;
        private readonly IMediator _mediator;

        public CustomerDocumentController(IMediator mediator, ILogger<CustomerDocumentController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "GetCustomerDocument", tags: new[] { "Customer Document" }, Summary = "Retrieve details of a customer’s profile based on CustomerId.", Description = "Returns details of a customer.The details includes customer’s basic profile, address details.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "This field specifies the Unique CustomerId.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Customers), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetCustomerDocument")]
        public async Task<HttpResponseData> GetCustomerDocument([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customerdocument/{customerId}")] HttpRequestData req, string customerId)
        {
            Result<Response<Customers>> result = await _mediator.Send(new FetchCustomerDocumentQuery(customerId), CancellationToken.None);
            return await ResultExtention<Customers>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "CustomerDocumentRetrievalByMemberNumber", tags: new[] { "Customer Profile API" }, Summary = "Retrieve customer's details.", Description = "Returns details of a customer’s profile who have a Flying club login. The details includes customer’s basic profile, address details, contact details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "memberNumber", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "This field specifies the Frequent flyer number.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CustomerProfile), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CustomerDocumentByMemberNumber")]

        public async Task<HttpResponseData> GetaccountCustomerProfileAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customerdocument/account/{memberNumber}")] HttpRequestData req, FunctionContext executionContext, string memberNumber)
        {
            Result<Response<Customers>> result = await _mediator.Send(new CustomerDucomentQuery(memberNumber));
            return await ResultExtention<Customers>.ExecuteResultAsync(req, result);
        }
    }
}
