using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.BaggageAllowance;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.CheckPaymentEligibility;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.ConfirmBaggage;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.SaveBags;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.UpdateBags;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Triggers.Http
{
    public class BaggageController
    {
        private readonly IBaggageService _baggageService;

        public BaggageController(IBaggageService baggageService)
        {
            _baggageService = baggageService ?? throw new ArgumentNullException(nameof(baggageService));
        }

        [OpenApiOperation(operationId: "GetBaggage", tags: new[] { "Baggage Microservice" }, Summary = "Get the baggage allowance for all passenger during check in", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter("travelPartyId", In = ParameterLocation.Path, Required = true, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("hidePaidBaggage", In = ParameterLocation.Query, Required = false, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(BaggageAllowanceResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GetBaggage")]
        public async Task<HttpResponseData> GetBaggage([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetBaggage/{travelPartyId}")] HttpRequestData req, string travelPartyId, bool hidePaidBaggage)
        {
            var result = await _baggageService.GetBaggageAllowance(travelPartyId, hidePaidBaggage);
            return await ResultExtention<BaggageAllowanceResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "SaveBaggage", tags: new[] { "Baggage Microservice" }, Summary = "Save the bagagge info for all passenger during check in", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(SaveBaggageRequest), Description = "Save bagagge request with travelparty Id, passenger and baggage offer informations.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SaveBaggageResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SaveBaggage")]
        public async Task<HttpResponseData> SaveBaggage([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SaveBaggage")] HttpRequestData req)
        {
            var result = await _baggageService.SaveBagsAsync(Json.Deserialize<SaveBaggageRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<SaveBaggageResponse>.ExecuteResultAsync(req, result);
        }


        [OpenApiOperation(operationId: "BaggagePaymentCheck", tags: new[] { "Baggage Microservice" }, Summary = "Check payment eligibility for baggage during check in", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CheckPaymentRequest), Description = "Check payment eligibility for baggage using travelpartyid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CheckPaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CheckPayment")]
        public async Task<HttpResponseData> CheckPayment([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CheckPayment")] HttpRequestData req)
        {
            var result = await _baggageService.CheckPaymentAsync(Json.Deserialize<CheckPaymentRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<CheckPaymentResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "ConfirmBaggage", tags: new[] { "Baggage Microservice" }, Summary = "Confirm the Baggage allowences after the payment", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(ConfirmBaggageRequest), Description = "Confirm the Baggage allowences after the payment using purchase receipt")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ConfirmBaggageResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("ConfirmBaggage")]
        public async Task<HttpResponseData> ConfirmBaggage([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ConfirmBaggage")] HttpRequestData req)
        {
            var result = await _baggageService.ConfirmBaggageAsync(Json.Deserialize<ConfirmBaggageRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<ConfirmBaggageResponse>.ExecuteResultAsync(req, result);
        }
    }
}