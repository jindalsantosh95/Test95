using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.FlightOperations.Catering.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Catering.Application.Models;
using VAA.EAPI.Microservices.FlightOperations.Catering.Infrastructure.Models.Entity;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Triggers.Http
{
    public class CateringController(ICateringService cateringService)
    {
        private readonly ICateringService _cateringService = cateringService ?? throw new ArgumentNullException(nameof(cateringService));

        [OpenApiOperation(operationId: "ValidateSegmentsMealEligibility", tags: new[] { "Catering Microservice" }, Summary = "Check segments eligibility to get flight meals", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(BookingInfo), Description = "Request to return meal eligible booking information")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(BookingInfo), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("ValidateSegmentsMealEligibility")]
        public async Task<HttpResponseData> ValidateSegmentsMealEligibility([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ValidateSegmentsMealEligibility")] HttpRequestData req)
        {
            var bookingInfo = _cateringService.ValidateSegmentsMealEligibility(Json.Deserialize<BookingInfo>(await new StreamReader(req.Body).ReadToEndAsync()));

            return await ResultExtention<BookingInfo>.ExecuteResultAsync(req, bookingInfo);
        }

        [OpenApiOperation(operationId: "GetFlightMeals", tags: new[] { "Catering Microservice" }, Summary = "Retrieve flight Meals", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(MealsRequest), Description = "Request to return flight meals")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<Meal>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GetFlightMeals")]
        public async Task<HttpResponseData> GetFlightMeals([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "GetFlightMeals")] HttpRequestData req)
        {
            var mealInfo = await _cateringService.GetFlightMealsAsync(Json.Deserialize<MealsRequest>(await new StreamReader(req.Body).ReadToEndAsync()));

            return await ResultExtention<IEnumerable<Meal>>.ExecuteResultAsync(req, mealInfo);
        }

        [OpenApiOperation(operationId: "CreateMealOrder", tags: new[] { "Catering Microservice" }, Summary = "Create Meal Order data in db ", Description = "http endpoint to submit a event for Meal Order create which allow asynchronous processing of create Meal Order request in background. The status information is returned in the response.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(MealOrders), Description = "Request to add Meal Order")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CreateMealOrder")]
        public async Task<HttpResponseData> CreateMealOrder([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateMealOrder")] HttpRequestData req)
        {
            var response = await _cateringService.AddMealOrderAsync(Json.Deserialize<MealOrders>(await new StreamReader(req.Body).ReadToEndAsync()));
           
            return await ResultExtention<string>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "UpdateMealOrder", tags: new[] { "Catering Microservice" }, Summary = "Submit a new event for Meal Order update", Description = "http endpoint to submit a event for Meal Order update which allow asynchronous processing of create Meal Order request in background. The status information is returned in the response.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(MealOrders), Description = "Request to update Meal Order")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdateMealOrder")]
        public async Task<HttpResponseData> UpdateMealOrder([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UpdateMealOrder")] HttpRequestData req)
        {
            var request = Json.Deserialize<MealOrders>(await new StreamReader(req.Body).ReadToEndAsync());
            var response = await _cateringService.UpdateMealOrderAsync(request);
           
            return await ResultExtention<string>.ExecuteResultAsync(req, response);
        }

    }
}