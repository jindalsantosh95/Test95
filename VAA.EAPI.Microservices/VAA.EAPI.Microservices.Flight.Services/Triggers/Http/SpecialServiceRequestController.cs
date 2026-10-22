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
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR;
using VAA.EAPI.Microservices.Flight.Services.Interface;
using VAA.EAPI.Microservices.Flight.Services.Models.SSR;
using MicroserviceSSRSchema = VAA.EAPI.Microservices.Flight.Services.Models.SSR;

namespace VAA.EAPI.Microservices.Flight.Services.Triggers.Http
{
    public class SpecialServiceRequestController(ISpecialServiceRequest SSRService)
    {
        private readonly ISpecialServiceRequest _SSRService = SSRService;

        [OpenApiOperation(operationId: "SSRDetails", tags: new[] { "Retrieve SSR" }, Summary = "Retrieve SSR (Special Service Requests) details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "tripReservationID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Trip Reservation ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DeltaSSRResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Unprocessable Entity")]
        [Function("SsrDetails")]
        public async Task<HttpResponseData> SsrDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ssrs/{tripReservationID}")] HttpRequestData req, string tripReservationID)
        {
            var responseResult = await _SSRService.GetSSRDetailsAsync(tripReservationID);
            return await ResultExtention<PassengerSSRResponse>.ExecuteResultAsync(req, responseResult);
        }

        [OpenApiOperation(operationId: "ManageSSRDetails", tags: new[] { "Add Update or Delete SSR details" }, Summary = "Add Update or Delete SSR (Special Service Requests) details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "tripReservationID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Trip Reservation ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DeltaSSRResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Unprocessable Entity")]
        [Function("ManageSSRDetails")]
        public async Task<HttpResponseData> ManageSSRDetails([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "ssrs/{tripReservationID}")] HttpRequestData req, string tripReservationID)
        {
            var ssrUpsertRequest = Json.Deserialize<MicroserviceSSRSchema.SSRUpsertRequest>(await new StreamReader(req.Body).ReadToEndAsync());
            var responseResult = await _SSRService.ManageSSRDetailsAsync(ssrUpsertRequest, tripReservationID);

            return await ResultExtention<PassengerSSRResponse>.ExecuteResultAsync(req, responseResult);
        }
    }
}