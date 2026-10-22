using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Trip.Application;
using MSSchema = VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails;
using ErrorDetails = VAA.EAPI.Domain.Entities.Common.ErrorDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Triggers.Http
{
    public class MessageDetailsController
    {
        public readonly IMessageCentreService _messageCentreService;

        public MessageDetailsController(IMessageCentreService messageCentreService)
        {
            _messageCentreService = messageCentreService ?? throw new ArgumentNullException(nameof(messageCentreService));
        }

        [OpenApiOperation(operationId: "MessageDetails", tags: new[] { "Message Details against a trip" }, Summary = "Retrieve Message Details against a trip", Description = "Retrieve Message Details against a trip", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "tripBookingID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "trip identifier id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(MSSchema.MessageResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Unprocessable Entity")]
        [Function("RetrieveMessagedetails")]
        public async Task<HttpResponseData> RetrieveMessageDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "messages/{tripBookingID}")] HttpRequestData req, string tripBookingID)
        {
            var response = await _messageCentreService.GetMessageDetails(tripBookingID);

            return await ResultExtention<MSSchema.MessageResponse>.ExecuteResultAsync(req, response);
        }
    }
}
