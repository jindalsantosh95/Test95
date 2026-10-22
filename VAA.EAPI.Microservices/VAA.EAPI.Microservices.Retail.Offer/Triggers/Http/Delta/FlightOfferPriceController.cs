using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
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
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightOfferPrice;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;

namespace VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Delta
{
    public class FlightOfferPriceController
    {
        private readonly IMediator _mediator;

        public FlightOfferPriceController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "FlightOfferPrice", tags: new[] { "Flight Offer Price" }, Summary = "This endpoint reprice the selected offer", Description = "This endpoint returns more details for the selected offer", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(FlightOfferRequest), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OfferResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FlightOfferPrice")]
        public async Task<HttpResponseData> FlightOfferPrice([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Offers/Search/Price")] HttpRequestData req)
        {
            var commandOutput = await _mediator.Send(new OfferPriceQuery
            {
                Request = Json.Deserialize<FlightOfferRequest>(await new StreamReader(req.Body).ReadToEndAsync()),
                Type = Constants.Application.OfferPriceEventSubject
            });
            return await ResultExtention<FlightOfferResponse>.ExecuteResultAsync(req, commandOutput);
        }


        [OpenApiOperation(operationId: "OfferPriceDeepLink", tags: new[] { "Offer Price DeepLink" }, Summary = "This endpoint performs offer price for the selected flights and fares from Metas", Description = "This endpoint performs offr price for the selected flights and fares from Metas", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(DeepLinkSummaryRequest), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OfferResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OfferPriceDeepLink")]
        public async Task<HttpResponseData> OfferPriceDeepLink([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Offers/Search/OfferPriceDeepLink")] HttpRequestData req)
        {
            var commandOutput = await _mediator.Send(new OfferPriceDeeplinkQuery
            {
                Request = Json.Deserialize<DeepLinkSummaryRequest>(await new StreamReader(req.Body).ReadToEndAsync())

            });
            return await ResultExtention<FlightOfferResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
