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
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Triggers.Http
{
    public sealed class CheckAircraftCategoryController(ICheckAircraftCategoryService checkAircraftCategoryService)
    {
        private readonly ICheckAircraftCategoryService _checkAircraftCategoryService = checkAircraftCategoryService ?? throw new ArgumentNullException(nameof(checkAircraftCategoryService));

        [OpenApiOperation(operationId: "CheckAircraftCategory", tags: new[] { "Flights Microservice" }, Summary = "Check aircraft category by tail number", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CheckAircraftCategoryRequest), Description = "Request to check a category index against aircraft tail numbers")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CheckAircraftCategoryResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CheckAircraftCategory")]
        public async Task<HttpResponseData> CheckAircraftCategory([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "checkaircraftcategory")] HttpRequestData req)
        {
            var result = await _checkAircraftCategoryService.CheckAircraftCategoryAsync(Json.Deserialize<CheckAircraftCategoryRequest>(await new StreamReader(req.Body).ReadToEndAsync()), req.FunctionContext.CancellationToken);

            return await ResultExtention<CheckAircraftCategoryResponse>.ExecuteResultAsync(req, result);
        }
    }
}
