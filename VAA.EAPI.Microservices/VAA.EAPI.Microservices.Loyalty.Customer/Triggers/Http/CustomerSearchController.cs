using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http
{
    public class CustomerSearchController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerSearchController> _logger;

        public CustomerSearchController(IMediator mediator, ILogger<CustomerSearchController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "CustomerSearchDocument", tags: new[] { "Customer Microservice - CustomerSearchDocument" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CustomerSearch), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "memberNumber", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CustomerSearchDocumentByMemberNumber")]
        public async Task<HttpResponseData> FetchCustomerSearchDocumentByMemberNumberAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customerSearch/{memberNumber}")] HttpRequestData request, string memberNumber)
        {
            //MemberNumber is Partition key for CustomerSearch
            var commandOutput = await _mediator.Send(new CustomerSearchQuery(memberNumber));
            return await ResultExtention<CustomerSearch>.ExecuteResultAsync(request, commandOutput);
        }


        [OpenApiOperation(operationId: "CustomerSearchByMemberNumber", tags: new[] { "Customer Microservice - CustomerSearchByMemberNumber" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CustomerSearch), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CustomerSearchByMemberNumber")]
        public async Task<HttpResponseData> FetchCustomerSearchByMemberNumberAsync([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "customerSearchByNumber")] HttpRequestData request)
        {
            var commandOutput = await _mediator.Send(new CustomerSearchQuery(request.Headers.GetValues("memberNumber").FirstOrDefault()));
            return await ResultExtention<CustomerSearch>.ExecuteResultAsync(request, commandOutput);
        }

        [OpenApiOperation(operationId: "CustomerTierByEmailId", tags: new[] { "Customer Microservice - CustomerTierByEmailId" }, Summary = "Retrive Tier Code.", Description = "Retirieve Tier Code for all Basic, VAC and FC customers based on Email Address.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CustomerSearchByEmailQuery), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CustomerTierInfo), Summary = "Retrive Tier Code.", Description = "Retrive Tier Code based on EmailId.")]
        [OpenApiParameter(name: "emailId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Email ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CustomerTierByEmailId")]
        public async Task<HttpResponseData> FetchCustomerSearchDocumentByEmailAddressAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customerSearchByEmail/{emailId}")] HttpRequestData request, string emailId)
        {
            return await ResultExtention<CustomerTierInfo>.ExecuteResultAsync(request, await _mediator.Send(new CustomerSearchByEmailQuery(emailId)));
        }
    }
}
