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
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http
{
    public class CustomerProfileController
    {
        private readonly IMediator _mediator;
        private readonly ICustomerProfileUpdatePreferencesCommand _customerProfileUpdatePreferencesCommand;

        public CustomerProfileController(IMediator mediator, ICustomerProfileUpdatePreferencesCommand customerProfileUpdatePreferencesCommand)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _customerProfileUpdatePreferencesCommand = customerProfileUpdatePreferencesCommand ?? throw new ArgumentNullException(nameof(customerProfileUpdatePreferencesCommand));
        }

        [OpenApiOperation(operationId: "CustomerProfileDocumentFetch", tags: new[] { "Customer Transactions Microservice - CustomerProfile" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CustomerProfileDocResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "CustomerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CustomerProfileDocumentByCustomerId")]
        public async Task<HttpResponseData> CustomerProfileDocumentByCustomerIdAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{CustomerId}/customerProfile")] HttpRequestData request, string customerId)
        {
            var result = await _mediator.Send(new CustomerProfileQuery() { CustomerId = customerId, DocType = ConfigReader.Get(EnvironmentSettings.CustomerProfileDocType) });
            return await ResultExtention<CustomerProfileDocResponse>.ExecuteResultAsync(request, result);
        }

        [OpenApiOperation(operationId: "UpdateMarketingPreferences", tags: new[] { "Customer Transactions Microservice - CustomerProfile" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(UpdateCustomerProfilePreference), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("UpdateMarketingPreferences")]
        public async Task<UpdateMarketingPreferenceResponse> UpdateCustomerProfilePreferences([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "UpdateMarketingPreferences")] HttpRequestData req)
        {
            return await _mediator.Send((CustomerProfileCommand)Json.Deserialize<UpdateCustomerProfilePreference>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None);
        }

        [OpenApiOperation(operationId: "UpdatePreferences", tags: new[] { "Customer Transactions Microservice - CustomerProfile" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(UpdateCustomerProfilePreference), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UpdateMarketingPreferenceResponse), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("UpdatePreferences")]
        public async Task<UpdateMarketingPreferenceResponse> UpdateCustomerPreferences([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "UpdatePreferences")] HttpRequestData req)
        {
            return await _customerProfileUpdatePreferencesCommand.Handle((CustomerProfileCommand)Json.Deserialize<UpdateCustomerProfilePreference>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None);
        }
    }
}