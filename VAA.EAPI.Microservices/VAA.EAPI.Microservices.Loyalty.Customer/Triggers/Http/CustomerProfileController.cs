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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.InflightProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.LoyaltyProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Service;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile;


namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http
{
    public class CustomerProfileController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerProfileController> _logger;
        private readonly ICustomerSearchService _customerSearchService;

        public CustomerProfileController(IMediator mediator, ILogger<CustomerProfileController> logger, ICustomerSearchService customerSearchService)
        {
            _mediator = mediator;
            _logger = logger;
            _customerSearchService = customerSearchService;
        }


        [OpenApiOperation(operationId: "CustomerProfile", tags: new[] { "Customer Profile API" }, Summary = "Retrieve details of a customer’s profile based on identity.", Description = "Returns details of a customer’s profile who have a Flying club login or are a VAA staff member.The details includes customer’s basic profile, address details and flags indicating future and historical booking information. It expects a valid B2C claim as an input, through which a unique id, identifying the customer, can be obtained. This interface provides a limited view of information about a customer.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CustomerProfile), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CustomerProfile")]
        public async Task<HttpResponseData> FetchCustomerProfileAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customer/profile")] HttpRequestData req, FunctionContext executionContext)
        {
            executionContext.Items.TryGetValue("CustomerID", out object customerId);
            executionContext.Items.TryGetValue("MemberStatus", out object memberStatus);
            Result<Response<CustomerProfile>> result = await GetMemberByCustID(Convert.ToString(customerId), Convert.ToString(memberStatus));
            return await ResultExtention<CustomerProfile>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "CustomerProfileById", tags: new[] { "Customer Profile API" }, Summary = "Retrieve details of a customer’s profile based on CustomerId.", Description = "Returns details of a customer’s profile who have a Flying club login or are a VAA staff member.The details includes customer’s basic profile, address details.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "This field specifies the Unique CustomerId.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CustomerProfile), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CustomerProfileByID")]
        public async Task<HttpResponseData> GetCustomerProfileAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customer/{customerId}")] HttpRequestData req, FunctionContext executionContext, string customerId)
        {
            Result<Response<CustomerProfile>> result = await GetMemberByCustID(Convert.ToString(customerId), "ACTIVE");
            return await ResultExtention<CustomerProfile>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "CustomerProfileByMemberId", tags: new[] { "Customer Profile API" }, Summary = "Retrieve details of a customer’s profile based on FrequentFlyer number.", Description = "Returns details of a customer’s profile who have a Flying club login or are a VAA staff member.The details includes customer’s basic profile, address details.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "memberID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "This field specifies the Frequent flyer number.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CustomerProfile), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CustomerProfileByLoyaltyID")]
        public async Task<HttpResponseData> GetLoyaltyCustomerProfileAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customer/loyalty/{memberID}")] HttpRequestData req, FunctionContext executionContext, string memberID)
        {
            _logger.LogInformation("Customer Profile API Controller | CustomerProfileByLoyaltyID");
            Result<Response<CustomerProfile>> result = await _mediator.Send(new LoyaltyProfileQuery(memberID));
            return await ResultExtention<CustomerProfile>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "CustomerTierCodeByEmailId", tags: new[] { "Customer Microservice - Flying Club Customer TierCode By EmailId" }, Summary = "Retrieve Tier Code.", Description = "Retrieve Tier Code based on email address.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CustomerSearchByEmailQuery), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CustomerTierInfo), Summary = "Get Tier Code", Description = "Get Tier Code using Email Address")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CustomerTierCodeByEmailId")]
        public async Task<HttpResponseData> FetchCustomerSearchDocumentByEmailId([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "customerSearchByEmail")] HttpRequestData request)
        {
            return await ResultExtention<CustomerTierInfo>.ExecuteResultAsync(request, await _mediator.Send(new CustomerSearchByEmailQuery(request.Headers.GetValues("CustEmail").FirstOrDefault())));
        }

        [OpenApiOperation(operationId: "FetchCustomerProfileDetails", tags: new[] { "Customer Microservice - Fetch Customer details" }, Summary = "Retrieve Customer Details.", Description = "Retrieve Customer Details.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CustomerProfileRequest), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<Infrastructure.Entities.Customers>), Summary = "Get Customer Information", Description = "Get Customer Information")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CustomerProfileDetails")]
        public async Task<HttpResponseData> FetchCustomerProfileDetails([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "customerProfileSearch")] HttpRequestData request)
        {
            return await ResultExtention<IEnumerable<Infrastructure.Entities.Customers>>.ExecuteResultAsync(request, await _customerSearchService.GetCustomerProfileDetails(Json.Deserialize<CustomerProfileRequest>(await new StreamReader(request.Body).ReadToEndAsync())));
        }

        [OpenApiOperation(operationId: "ValidateMemberStatus", tags: new[] { "Customer  Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("ValidateMemberStatus")]
        public async Task<string> FetchCustomerDetails([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "ValidateMemberStatus")] HttpRequestData req, FunctionContext executionContext)
        {
            return await _mediator.Send(new LoginValidationQuery(Json.Deserialize<MemberLoginRequest>(await new StreamReader(req.Body).ReadToEndAsync())), CancellationToken.None);
        }



        private async Task<Result<Response<CustomerProfile>>> GetMemberByCustID(string customerId, string memberStatus)
        {
            return await _mediator.Send(new CustomerProfileCommand(customerId, memberStatus), CancellationToken.None);
        }
    }
}
