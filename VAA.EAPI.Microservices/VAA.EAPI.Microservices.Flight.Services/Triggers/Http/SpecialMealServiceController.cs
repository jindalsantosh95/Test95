using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Flight.Services.Application.Command;
using VAA.EAPI.Microservices.Flight.Services.Domain.PopsMeal;
using VAA.EAPI.Microservices.Flight.Services.Interface;

namespace VAA.EAPI.Microservices.Flight.Services.Triggers.Http
{
    public class SpecialMealServiceController(ISpecialServiceRequest SSRService, IMealService mealService)
    {
        private readonly IMealService _mealService = mealService;


        [OpenApiOperation(operationId: "AddMeal", tags: new[] { "Meal Management" }, Summary = "Add meal to reservation", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(MealOrderRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MealOperationResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("AddMeal")]
        public async Task<HttpResponseData> AddMeal([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "meals/add")] HttpRequestData req)
        {
            var mealRequest = Json.Deserialize<MealOrderRequest>(await new StreamReader(req.Body).ReadToEndAsync());
            var result = await _mealService.AddMealAsync(mealRequest);
            return await ResultExtention<MealOperationResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "CancelMeal", tags: new[] { "Meal Management" }, Summary = "Cancel meal from reservation", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(MealCancelRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MealOperationResponse), Summary = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CancelMeal")]
        public async Task<HttpResponseData> CancelMeal([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "meals/cancel")] HttpRequestData req)
        {
            var mealRequest = Json.Deserialize<MealCancelRequest>(await new StreamReader(req.Body).ReadToEndAsync());
            var result = await _mealService.CancelMealAsync(mealRequest);
            return await ResultExtention<MealOperationResponse>.ExecuteResultAsync(req, result);
        }
    }
}