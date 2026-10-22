using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Common.OfferSearch;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Common
{
    public class OfferSearchController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OfferSearchController> _logger;

        public OfferSearchController(IMediator mediator, ILogger<OfferSearchController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "OfferSearch", tags: new[] { "Offer Search" }, Summary = "This endpoint search the selected offer details from OfferDB", Description = "This endpoint search the selected offer details from OfferDB", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "channelId", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "offerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Offer id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OfferResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OfferSearch")]
        public async Task<HttpResponseData> OfferSearch([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "offers/{offerId}")] HttpRequestData req, string offerId)
        {
            var channelId = req?.Headers != null && (bool)req?.Headers.Contains("channelId") ? req?.Headers.GetValues("channelId").FirstOrDefault() : null;
            _logger.LogInformation($"OfferSearch Controller | ChannelId : {channelId} | OfferId : {offerId}");

            var offerSearchQuery = new OfferSearchQuery(offerID: offerId);

            var commandOutput = await _mediator.Send(offerSearchQuery, CancellationToken.None);
            return await ResultExtention<OfferResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
