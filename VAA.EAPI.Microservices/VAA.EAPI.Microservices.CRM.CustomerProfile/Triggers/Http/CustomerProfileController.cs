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
using VAA.EAPI.Microservices.CRM.CustomerProfile.Application.Query.Interface;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Triggers.Http
{
    public class CustomerProfileController
    {
        
        private readonly IAmperityHandler _amperityHandler;   

        public CustomerProfileController(ILogger<CustomerProfileController> logger, IAmperityHandler amperityHandler)
        {
            _amperityHandler = amperityHandler ?? throw new ArgumentNullException(nameof(amperityHandler));
        }

        [Function("GetAmperityDetails")]
        [OpenApiOperation(operationId: "GetAmperityDetails", tags: new[] { "Customer Profile Microservice" }, Summary = "Fetch amperity details for the hashed loyalty id", Description = "Fetch amperity details for the hashed loyalty id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "loyaltyToken", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "This field specifies the hashed LoyaltyId.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AmperityDetails), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> GetAmperityDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customerprofile/{loyaltyToken}")] HttpRequestData req, FunctionContext executionContext, string loyaltyToken)
        {            
            return await ResultExtention<AmperityDetails>.ExecuteResultAsync(req, await _amperityHandler.GetAmperityDetails(loyaltyToken));
        }        
    }
}
