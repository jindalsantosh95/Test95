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
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.CustomerDetail;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Enum;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Interface;
namespace VAA.EAPI.Microservices.Loyalty.Vred.Triggers.Http
{
    public class UpdateVredCustomerDetailsController
    {

        private readonly IVredCustomerDetailsResponseQuery _vredCustomerDetailsResponseQuery;

        public UpdateVredCustomerDetailsController(IVredCustomerDetailsResponseQuery vredCustomerDetailsResponseQuery)
        {


            _vredCustomerDetailsResponseQuery = vredCustomerDetailsResponseQuery ?? throw new ArgumentNullException(nameof(vredCustomerDetailsResponseQuery));

        }
        [OpenApiOperation(operationId: "UpdateVredCustomerDetailsResponse", tags: new[] { "VRED Microservice" }, Summary = "Get update customer details event response", Description = "It returns update customer details event response and status against requested correlationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UpdateVredCustomerDetailsQueryResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdateVredCustomerDetailsResponse")]
        public async Task<HttpResponseData> UpdateVredCustomerDetailsResponseAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "updatevredcustomerdetails/response/{correlationid}")] HttpRequestData req, string correlationid)
        {
            Result<Response<UpdateVredCustomerDetailsQueryResponse>> commandOutput = await _vredCustomerDetailsResponseQuery.Handle(new UpdateVredCustomerDetailsResponseQuery { CorrelationId = correlationid, Type = EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.UpdateVredCustomerDetailEvent.ToString())) }, CancellationToken.None);
            return await ResultExtention<UpdateVredCustomerDetailsQueryResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
