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
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.FlexiDateSearch;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch;


namespace VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Ita
{
    public class FlexibleDatesSearchController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FlexibleDatesSearchController> _logger;

        public FlexibleDatesSearchController(IMediator mediator, ILogger<FlexibleDatesSearchController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "FlexibleDatesSearch", tags: new[] { "Flight Offer" }, Summary = "Gets Flight results flex view", Description = "Gets Flight results flex view", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(OfferRequest), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(FlexiDateSearchResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FlexibleDatesSearch")]
        public async Task<HttpResponseData> FlexibleDatesSearch([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "FlexibleDatesSearch")] HttpRequestData req)
        {
            var commandOutput = await _mediator.Send(Json.Deserialize<FlexiCalendarSearchQuery>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<FlexiDateSearchResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
