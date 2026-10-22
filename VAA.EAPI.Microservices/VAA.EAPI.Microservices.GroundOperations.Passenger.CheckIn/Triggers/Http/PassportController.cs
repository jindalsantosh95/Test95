using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Triggers.Http
{
    public class PassportController
    {
        private readonly ICheckInService _checkInService;

        public PassportController(ICheckInService checkInService)
        {
            _checkInService = checkInService;
        }

        [OpenApiOperation(operationId: "SavePassport", tags: new[] { "CheckIn Microservice" }, Summary = "Saves the passport details for the given passengers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(SavePassportRequest), Description = "Saves the passport details for the given passengers as part of checkin process")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SavePassportResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request")]
        [Function("SavePassport")]
        public async Task<HttpResponseData> SavePassport([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CheckIn/SavePassport")] HttpRequestData req)
        {
            var request = Json.Deserialize<SavePassportRequest>(await new StreamReader(req.Body).ReadToEndAsync());
            Validator.ValidateObject(request, new ValidationContext(request), true);

            var result = await _checkInService.SavePassportAsync(request);
            return await ResultExtention<CheckInResponse>.ExecuteResultAsync(req, result);
        }
    }
}
