using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.ContactHistoryDocument;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ContactHistoryDocument;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http
{
    public class ContactHistoryController
    {
        private readonly IMediator _mediator;

        public ContactHistoryController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "ContactHistoryDocument", tags: new[] { "Customer Microservice - ContactHistoryDocument" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ContactHistoryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("ContactHistoryDocumentsByCustomerId")]
        public async Task<HttpResponseData> FetchContactHistoryDocumentsByCustomerIdAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerId}/contactHistory")] HttpRequestData request, string customerId)
        {
            var result = await _mediator.Send(new ContactHistoryQuery(customerId), cancellationToken: CancellationToken.None);
            return await ResultExtention<ContactHistoryResponse>.ExecuteResultAsync(request,
                Result.Success(new Response<ContactHistoryResponse>()
                {
                    Status = Status.OK,
                    Value = new ContactHistoryResponse(result)
                }));
        }
    }
}