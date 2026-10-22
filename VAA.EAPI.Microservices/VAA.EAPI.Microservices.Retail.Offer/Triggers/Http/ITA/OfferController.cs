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
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Offer;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reprice;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Ita;

namespace VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Ita
{
    public class OfferController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OfferController> _logger;
        private readonly IItaService _itaService;

        public OfferController(IMediator mediator, ILogger<OfferController> logger, IItaService itaService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _itaService = itaService ?? throw new ArgumentNullException(nameof(itaService));
        }

        [OpenApiOperation(operationId: "FlightOffer", tags: new[] { "Flight Offer" }, Summary = "Return the offer for booking change", Description = "Checks the eligibility of existing booking for change and then return offers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(OfferRequest), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OfferResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FlightOffer")]
        public async Task<HttpResponseData> OrderReshopBundle([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Offers")] HttpRequestData req)
        {
            _logger.LogInformation("flight Offer Request Received");
            var orderReshopBundleQuery = Json.Deserialize<OfferQuery>(await new StreamReader(req.Body).ReadToEndAsync());
            var commandOutput = await _mediator.Send(orderReshopBundleQuery);
            return await ResultExtention<OfferResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "OfferReprice", tags: new[] { "Offer Reprice" }, Summary = "This endpoint reprice the selected offer", Description = "This endpoint reprice the selected offer", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(OfferRepriceQuery), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OfferResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OfferReprice")]
        public async Task<HttpResponseData> OrderReprice([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "offers/price")] HttpRequestData req)
        {
            _logger.LogInformation("Offer Reprice Request Received");
            var orderRepriceQuery = Json.Deserialize<OfferRepriceQuery>(await new StreamReader(req.Body).ReadToEndAsync());
            var commandOutput = await _mediator.Send(orderRepriceQuery, CancellationToken.None);
            return await ResultExtention<OfferResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [Function("ITAFlightOffer")]
        public async Task<HttpResponseData> ITAOffers([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ITAOffers")] HttpRequestData req, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ITA flight Offer Request Received");
            var itaRequest = Json.Deserialize<search>(await new StreamReader(req.Body).ReadToEndAsync(cancellationToken));
            var itaResponse = new Response<result>
            {
                Status = Status.OK,
                Value = await _itaService.GetOffers(itaRequest, cancellationToken)
            };
            return await ResultExtention<result>.ExecuteResultAsync(req, itaResponse);
        }
    }
}
