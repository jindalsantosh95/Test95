using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.BoardingPass;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.ContactTracing;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.EmergencyContact;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.ReturnDate;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Triggers.Http
{
    public class CheckInController
    {
        private readonly ICheckInService _checkInService;
        public CheckInController(ICheckInService checkInService)
        {
            _checkInService = checkInService;
        }

        [OpenApiOperation(operationId: "InitiateCheckIn", tags: new[] { "CheckIn Microservice" }, Summary = "Initiate a checkin session for a given booking", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CheckInRequest), Description = "Details to start a new checkin session")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CheckInResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("InitiateCheckIn")]
        public async Task<HttpResponseData> InitiateCheckIn([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CheckIn")] HttpRequestData req)
        {
            var result = await _checkInService.InitiateCheckInAsync(Json.Deserialize<CheckInRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<CheckInResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "LogTransactionSummary", tags: new[] { "CheckIn Microservice" }, Summary = "Log an online check-in error", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(TransactionSummaryRequest), Description = "Online check-in error details")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Summary = "OK", Description = "OK")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("LogTransactionSummary")]
        public async Task<HttpResponseData> LogTransactionSummary([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CheckIn/TransactionSummary")] HttpRequestData req)
        {
            TransactionSummaryRequest? model;
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                model = Json.Deserialize<TransactionSummaryRequest>(body);
            }
            catch (System.ArgumentNullException)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            catch (Newtonsoft.Json.JsonException)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            if (model is null || model.TravelPartyId.IsNullOrWhiteSpace())
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var isLogged = await _checkInService.LogTransactionSummaryAsync(model);
            return req.CreateResponse(isLogged ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
        }

        [OpenApiOperation(operationId: "GetCheckIn", tags: new[] { "CheckIn Microservice" }, Summary = "Get the current state of a checkin session", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter("travelPartyId", In = ParameterLocation.Path, Required = true, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CheckInResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GetCheckIn")]
        public async Task<HttpResponseData> GetCheckIn([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "CheckIn/{travelPartyId}")] HttpRequestData req, string travelPartyId)
        {
            var result = await _checkInService.GetCheckInAsync(travelPartyId);
            return await ResultExtention<CheckInResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "SelectPassengers", tags: new[] { "CheckIn Microservice" }, Summary = "Select passenger to initiate checkIn process", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(Domain.SelectPassengerRequest), Description = "passenger details to initiate checkin process")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CheckInResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SelectPassengers")]
        public async Task<HttpResponseData> SelectPassengers([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CheckIn/selectPassengers")] HttpRequestData req)
        {
            var result = await _checkInService.SelectPassengersAsync(Json.Deserialize<Domain.SelectPassengerRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<CheckInResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "ContractTracing", tags: new[] { "CheckIn Microservice" }, Summary = "Capture the contact details during checkin process", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(ContactTracingRequest), Description = "Details of Contact information of passengers during checkin process")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CheckInResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SaveContactTracing")]
        public async Task<HttpResponseData> SaveContactTracing([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CheckIn/SaveContactTracing")] HttpRequestData req)
        {
            var result = await _checkInService.SaveContactTracingAsync(Json.Deserialize<ContactTracingRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<CheckInResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "EmergencyContact", tags: new[] { "CheckIn Microservice" }, Summary = "Capture the Emergency contact details during checkin process", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(EmergencyContactRequest), Description = "Details of Emergency Contact information of passengers during checkin process")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CheckInResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SaveEmergencyContactDetails")]
        public async Task<HttpResponseData> SaveEmergencyContactDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CheckIn/SaveEmergencyContactDetails")] HttpRequestData req)
        {
            var result = await _checkInService.SaveEmergencyContactDetailsAsync(Json.Deserialize<EmergencyContactRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<CheckInResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "SaveReturnDate", tags: new[] { "CheckIn Microservice" }, Summary = "Capture the Return date information during checkin process", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(ReturnDateRequest), Description = "Details of Return Date information of passengers during checkin process")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CheckInResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SaveReturnDate")]
        public async Task<HttpResponseData> SaveReturnDate([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CheckIn/SaveReturnDate")] HttpRequestData req)
        {
            var result = await _checkInService.SaveReturnDateAsync(Json.Deserialize<ReturnDateRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<CheckInResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "GenerateBoardingPasses", tags: new[] { "CheckIn Microservice" }, Summary = "Generates boarding passes for pnr with passenger and flight informations including the QR Code.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(BoardingPassRequest), Description = "Boarding pass Request has TravelPartyId and PNR and Origin Code")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(BoardingPassResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GenerateBoardingPasses")]
        public async Task<HttpResponseData> GenerateBoardingPasses([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CheckIn/BoardingPass")] HttpRequestData req)
        {
            var result = await _checkInService.BoardingPassAsync(Json.Deserialize<BoardingPassRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<BoardingPassResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "RetrieveBoardingPass", tags: new[] { "CheckIn Microservice" }, Summary = "Retrieve boarding passes By PNR and origin code, for pnr with passenger and flight informations including the QR Code.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(BoardingPassRequest), Description = "Boarding pass Request has TravelPartyId and PNR and Origin Code")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(BoardingPassResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("RetrieveBoardingPass")]
        public async Task<HttpResponseData> RetrieveBoardingPass([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CheckIn/RetrieveBoardingPass")] HttpRequestData req)
        {
            var result = await _checkInService.RetrieveBoardingPass(Json.Deserialize<BoardingPassRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<BoardingPassResponse>.ExecuteResultAsync(req, result);
        }
    }
}
