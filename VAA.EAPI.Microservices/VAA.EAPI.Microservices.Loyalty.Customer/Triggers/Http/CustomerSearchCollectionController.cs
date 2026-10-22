using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearchCollection;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities.CustomerSearchCollection;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http
{
    public class CustomerSearchCollectionController
    {
        private readonly IMediator _mediator;

        public CustomerSearchCollectionController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "CustomerSearchByPhoneNumber", tags: new[] { "Customer Microservice - CustomerSearchByPhoneNumber" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<CustomerSearch>), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "phoneNumber", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Phone Number")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CustomerSearchByPhoneNumber")]
        public async Task<HttpResponseData> FetchCustomerSearchByPhoneNumberAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customerSearchByPhone/{phoneNumber}")] HttpRequestData request, string phoneNumber)
        {
            return await ResultExtention<IEnumerable<CustomerSearch>>.ExecuteResultAsync(request, await _mediator.Send(new CustomerSearchByPhoneQuery(phoneNumber)));
        }
    }
}
