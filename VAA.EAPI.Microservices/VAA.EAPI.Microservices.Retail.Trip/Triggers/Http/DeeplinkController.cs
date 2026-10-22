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
using VAA.EAPI.Microservices.Retail.Trip.Application;
using VAA.EAPI.Microservices.Retail.Trip.Domain.DeepLink;
using VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Triggers.Http
{
    public class DeeplinkController
    {
        public readonly IDeeplinkService _linkService;

        public DeeplinkController(IDeeplinkService service)
        {
            _linkService = service ?? throw new ArgumentNullException(nameof(service));
        }

        [OpenApiOperation(operationId: "Deeplink", tags: new[] { "Deeplink Url" }, Summary = "Basis TripReservationId and selection option Get Url is provided for redirect", Description = "Basis TripReservationId and selection option Get Url is provided for redirect", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(DeepLinkRequestPayload), Description = "")]        
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(DeeplinkResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Unprocessable Entity")]
        [Function("GetDeeplinkUrl")]
        public async Task<HttpResponseData> GetDeeplinkUrl([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "deeplink")] HttpRequestData req)
        {
            var response = await _linkService.GetDeeplinkUrlDetails(Json.Deserialize<DeepLinkRequestPayload>(await new StreamReader(req.Body).ReadToEndAsync()));

            return await ResultExtention<DeeplinkResponse>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "DeeplinkdetailForProxy", tags: new[] { "Deeplink Details For Proxy Page" }, Summary = "Basis CacheId provided in the URL, this endpoint provides the post parameter Details", Description = "Basis TripReservationId and selection option Get Url is provided for redirect", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "cacheid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "cache Id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(DeeplinkEndpointDetails), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Unprocessable Entity")]
        [Function("GetDeeplinkDetailsForProxy")]
        public async Task<HttpResponseData> GetDeeplinkDetailsForProxy([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "deeplinkdetail/{cacheid}")] HttpRequestData req, string cacheid)
        {
            var response = await _linkService.GetDeeplinkDetailsForProxy(cacheid);

            return await ResultExtention<DeeplinkEndpointDetails>.ExecuteResultAsync(req, response);
        }
    }
}
