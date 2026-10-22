using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
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
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Delta
{
    public class SeatAvailabilityController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SeatAvailabilityController> _logger;

        public SeatAvailabilityController(IMediator mediator, ILogger<SeatAvailabilityController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "RetrieveSeatMap", tags: new[] { "Retrieve SeatMap" }, Summary = "Fetch the seatmap for VS segments", Description = "Check the segment details and fetch all cabin seatmap for particular VS flight", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(SearchCriteria), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(SeatAvailabilityResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("RetrieveSeatMap")]
        public async Task<HttpResponseData> RetrieveSeatMap([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "RetrieveSeatMap")] HttpRequestData req)
        {
            var seatAvailabilityQuery = Json.Deserialize<SeatAvailabilityQuery>(await new StreamReader(req.Body).ReadToEndAsync());
            var commandOutput = await _mediator.Send(seatAvailabilityQuery);

            return await ResultExtention<SeatAvailabilityResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
