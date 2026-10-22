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
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.CalendarSearch;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FareRules;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Domain.CalendarSearch;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FareRules;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Delta.FlexiDateSearch;

namespace VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Delta
{
    public class FlightOfferController
    {
        private readonly IMediator _mediator;

        public FlightOfferController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "FlightOfferSearch", tags: new[] { "Flight Offer - Details" }, Summary = "Search for flight prices on a single date, returning details of each flight in the slice.", Description = "Checks the eligibility of existing booking for change and then return offers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(FlightOfferRequest), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(FlightOfferResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FlightOfferSearch")]
        public async Task<HttpResponseData> FlightOfferSearch([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Offers/search")] HttpRequestData req)
        {
            var request = Json.Deserialize<FlightOfferRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            var commandOutput = await _mediator.Send(new FlightOfferQuery { Request = request, Type = Constants.Application.FlightOfferEventSubject });
            return await ResultExtention<FlightOfferResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "FlightOfferFlexiSearch", tags: new[] { "Flight Offer - Flexi Search" }, Summary = "Find the cheapest prices for both outbound and inbound slices, +/- 3 days.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(FlexiDateOfferRequest), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(FlexiDateSearchResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FlightOfferFlexiSearch")]
        public async Task<HttpResponseData> FlexiSearch([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Offers/FlexiSearch")] HttpRequestData req)
        {
            var request = Json.Deserialize<FlexiDateOfferRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            var commandOutput = await _mediator.Send(new FlexiDateSearchQuery(request));
            return await ResultExtention<FlexiDateSearchResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "FlightOfferCalendarSearch", tags: new[] { "Flight Offer - Calendar Search" }, Summary = "Find the cheapest prices for one slice over a full month", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(CalendarOfferRequest), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(CalendarSearchResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FlightOfferCalendarSearch")]
        public async Task<HttpResponseData> FlightOfferCalendarSearch([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Offers/CalendarSearch")] HttpRequestData req)
        {
            var request = Json.Deserialize<CalendarOfferRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            var commandOutput = await _mediator.Send(new CalendarSearchQuery(request));
            return await ResultExtention<CalendarSearchResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "FlightOfferFareRules", tags: new[] { "Flight Offer Fare Rules" }, Summary = "Returns the Fare rules info of an Offered flight", Description = "Returns the Fare rules info of an Offered flight", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OfferFareRules), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiParameter(name: "offerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "offer id", Visibility = OpenApiVisibilityType.Important)]
        [Function("FareRules")]
        public async Task<HttpResponseData> FlightOfferFareRules([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Offers/{offerId}/farerules")] HttpRequestData req, string offerId)
        {
            var commandOutput = await _mediator.Send(new FareRuleQuery { OfferId = offerId, Type = Constants.Application.FareRulesEventSubject });
            return await ResultExtention<OfferFareRules>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
