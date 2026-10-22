using FluentValidation;
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
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.AddressHistory;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http
{
    public class AddressHistoryController
    {
        private readonly IMediator _mediator;

        public AddressHistoryController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "AddressHistoryDocument", tags: new[] { "Customer Microservice - AddressHistoryDocument" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CustomerSearch), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("RetrieveAddressHistory")]
        public async Task<HttpResponseData> FetchAddressHistory([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerId}/addressHistory")] HttpRequestData request, string customerId)
        {
            try
            {
                var commandOutput = await _mediator.Send(new AddressHistoryQuery(customerId));

                return await ResultExtention<IEnumerable<AddressHistory>>.ExecuteResultAsync(request, new Response<IEnumerable<AddressHistory>>()
                {
                    Status = Status.OK,
                    Value = commandOutput
                });
            }
            catch (ValidationException)
            {
                return await ResultExtention<IEnumerable<AddressHistory>>.ExecuteResultAsync(request, new Response<IEnumerable<AddressHistory>>()
                {
                    Status = Status.BusinessException,
                    Value = null,
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                });
            }
        }
    }
}
