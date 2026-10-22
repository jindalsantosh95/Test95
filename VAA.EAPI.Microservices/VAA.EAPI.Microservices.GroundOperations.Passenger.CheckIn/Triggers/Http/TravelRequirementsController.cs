using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements;


namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Triggers.Http
{
    public class TravelRequirementsController(ITravelRequirementsService travelRequirementsService)
    {
        private readonly ITravelRequirementsService _travelRequirementsService = travelRequirementsService;

        [OpenApiOperation(operationId: "TravelRequirements", tags: ["CheckIn Microservice"], Summary = "Get travel requirements for a given booking", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(TravelRequirementsRequest), Description = "Details to retrieve travel document requirements")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TravelRequirementsResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Travel requirements error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GetTravelRequirements")]
        public async Task<HttpResponseData> GetTravelRequirements([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "TravelRequirements")] HttpRequestData req)
        {
            var request = await req.ParseAndValidateRequestAsync<TravelRequirementsRequest>(true);
            var result = await _travelRequirementsService.GetTravelRequirementsAsync(request);
            return await ResultExtention<TravelRequirementsResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "TravelRequirementsSavePassport", tags: ["CheckIn Microservice"], Summary = "Save passport information for a given booking for Travel Requirements", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(SavePassportRequest), Description = "Save Passport details per passenger to TRM for Travel Requirements")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TravelRequirementsResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request")]
        [Function("TravelRequirementsSavePassport")]
        public async Task<HttpResponseData> SavePassport([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "TravelRequirements/SavePassport")] HttpRequestData req)
        {
            var request = await req.ParseAndValidateRequestAsync<SavePassportRequest>(true);
            var result = await _travelRequirementsService.SavePassportAsync(request);
            return await ResultExtention<TravelRequirementsResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "TravelRequirementsSaveReturnDate", tags: ["CheckIn Microservice"], Summary = "Save return date information for a given passenger for Travel Requirements", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(SaveReturnDateRequest), Description = "Save Return Date details per passenger to TRM for Travel Requirements")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TravelRequirementsResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business validation error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request")]
        [Function("TravelRequirementsSaveReturnDate")]
        public async Task<HttpResponseData> SaveReturnDate([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "TravelRequirements/SaveReturnDate")] HttpRequestData req)
        {
            var request = await req.ParseAndValidateRequestAsync<SaveReturnDateRequest>(true);
            var result = await _travelRequirementsService.SaveReturnDateAsync(request);
            return await ResultExtention<TravelRequirementsResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "TravelRequirementsSaveEmergencyContact", tags: ["CheckIn Microservice"], Summary = "Save optional emergency contact information for a given passenger for Travel Requirements", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(SaveEmergencyContactRequest), Description = "Save optional emergency contact details per passenger to TRM for Travel Requirements", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TravelRequirementsResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business validation error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request")]
        [Function("TravelRequirementsSaveEmergencyContact")]
        public async Task<HttpResponseData> SaveEmergencyContact([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "TravelRequirements/SaveEmergencyContact")] HttpRequestData req)
        {
            var request = await req.ParseAndValidateRequestAsync<SaveEmergencyContactRequest>(true);
            var result = await _travelRequirementsService.SaveEmergencyContactAsync(request);
            return await ResultExtention<TravelRequirementsResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "TravelRequirementsSaveContactTracing", tags: ["CheckIn Microservice"], Summary = "Save US destination address/contact tracing information for a given passenger for Travel Requirements", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(SaveContactTracingRequest), Description = "Save US Destination Address/contact tracing details per passenger to TRM for Travel Requirements")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TravelRequirementsResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business validation error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request")]
        [Function("TravelRequirementsSaveContactTracing")]
        public async Task<HttpResponseData> SaveContactTracing([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "TravelRequirements/SaveContactTracing")] HttpRequestData req)
        {
            var request = await req.ParseAndValidateRequestAsync<SaveContactTracingRequest>(true);
            var result = await _travelRequirementsService.SaveContactTracingAsync(request);
            return await ResultExtention<TravelRequirementsResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "TravelRequirementsSaveGovernmentFormsAttestation", tags: ["CheckIn Microservice"], Summary = "Submit completion of a selected Government Form for a passenger and slice", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(SaveGovernmentFormsAttestationRequest), Description = "Passenger, slice, and client-selected Government Form data", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TravelRequirementsResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "Refreshed Travel Requirements state")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business validation error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request")]
        [Function("TravelRequirementsSaveGovernmentFormsAttestation")]
        public async Task<HttpResponseData> SaveGovernmentFormsAttestation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "TravelRequirements/SaveGovernmentFormsAttestation")] HttpRequestData req)
        {
            var request = await req.ParseAndValidateRequestAsync<SaveGovernmentFormsAttestationRequest>(true);
            var result = await _travelRequirementsService.SaveGovernmentFormsAttestationAsync(request);
            return await ResultExtention<TravelRequirementsResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "TravelRequirementsSaveSecondaryDocumentAttestation", tags: ["CheckIn Microservice"], Summary = "Submit completion of a selected secondary travel document for a passenger and slice", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(SaveSecondaryDocumentAttestationRequest), Description = "Passenger, slice, and client-selected secondary travel document data", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TravelRequirementsResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "Refreshed Travel Requirements state")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business validation error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request")]
        [Function("TravelRequirementsSaveSecondaryDocumentAttestation")]
        public async Task<HttpResponseData> SaveSecondaryDocumentAttestation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "TravelRequirements/SaveSecondaryDocumentAttestation")] HttpRequestData req)
        {
            var request = await req.ParseAndValidateRequestAsync<SaveSecondaryDocumentAttestationRequest>(true);
            var result = await _travelRequirementsService.SaveSecondaryDocumentAttestationAsync(request);
            return await ResultExtention<TravelRequirementsResponse>.ExecuteResultAsync(req, result);
        }
    }
}
