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
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Reference.Application.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Application.Query;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Triggers
{
    public class SvcReferenceDataController
    {
        private readonly IMediator _mediator;
        private readonly IDistanceAndCountryCodeQueryHandler _distanceAndCountryCodeQueryHandler;

        public SvcReferenceDataController(IMediator mediator, IDistanceAndCountryCodeQueryHandler distanceAndCountryCodeQueryHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _distanceAndCountryCodeQueryHandler = distanceAndCountryCodeQueryHandler ?? throw new ArgumentNullException(nameof(distanceAndCountryCodeQueryHandler));
        }

        [OpenApiOperation(operationId: "ProductDetails", tags: new[] { "Reference Microservice" }, Summary = "Fetch svc reference data for product details", Description = "Endpoint to fetch reference data of product details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<ProductDetails>), Summary = "Fetch VAA product details", Description = "Fetch product details for transaction creation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("ProductDetails")]
        public async Task<IEnumerable<ProductDetails>> ProductDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Products")] HttpRequestData req)
        {
            return await _mediator.Send(new ProductDataQuery());

        }

        [OpenApiOperation(operationId: "PartnerProfiles", tags: new[] { "Reference Microservice" }, Summary = "Fetch svc reference data for partner profile", Description = "Endpoint to fetch reference data of partner profile", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<PartnerProfileDetails>), Summary = "Fetch VAA partnerdetails", Description = "Fetch partner details for transaction creation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("PartnerProfile")]
        public async Task<IEnumerable<PartnerProfileDetails>> PartnerProfile([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "PartnerProfile")] HttpRequestData req)
        {

            return await _mediator.Send(new PartnerProfileQuery());
        }

        [OpenApiOperation(operationId: "ThresholdValues", tags: new[] { "Reference Microservice" }, Summary = "Fetch svc reference SP data for tier threshold values", Description = "Endpoint to fetch reference data of tier threshold values", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<TierThresholdValues>), Summary = "Fetch Threshold Tier points ", Description = "Fetch threshold tier point value for each tier code")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("ThresholdValues")]
        public async Task<IEnumerable<TierThresholdValues>> ThresholdValues([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ThresholdValues")] HttpRequestData req)
        {

            return await _mediator.Send(new TierThresholdQuery());
        }

        [OpenApiOperation(operationId: "GetBookingDetails", tags: new[] { "Reference Microservice" }, Summary = "Fetch svc reference SP data for booking detail values", Description = "Endpoint to fetch reference data of Booking details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<BookingDetails>), Summary = "Fetch Flight Booking Details", Description = "Fetch flight booking details duing missing point claim")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(FlownDetails), Required = true, Description = "")]
        [Function("GetBookingDetails")]
        public async Task<HttpResponseData> GetBookingDetails([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "GetBookingDetails")] HttpRequestData req)
        {
            return await ResultExtention<IEnumerable<BookingDetails>>.ExecuteResultAsync(req, await _mediator.Send(Json.Deserialize<GetBookingDetailsQuery>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [OpenApiOperation(operationId: "GetBookingDetailsV2", tags: new[] { "Reference Microservice" }, Summary = "Fetch svc reference SP data for booking detail values and validation values", Description = "Endpoint to fetch reference data of Booking details and validation data", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PreValidationPartnerData), Summary = "Fetch booking detail values and validation values", Description = "Fetch booking detail values and validation value")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(FlownDetails), Required = true, Description = "")]
        [Function("GetBookingDetailsV2")]
        public async Task<HttpResponseData> GetBookingDetailsV2([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "GetBookingDetailsV2")] HttpRequestData req)
        {
            return await ResultExtention<PreValidationPartnerData>.ExecuteResultAsync(req, await _mediator.Send(Json.Deserialize<GetBookingDetailsQueryV2>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [OpenApiOperation(operationId: "GetSegmentDistance", tags: new[] { "Reference Microservice" }, Summary = "Fetch svc reference SP data for Distance", Description = "Endpoint to fetch reference data of Distance", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "OriginCountryCode", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "This field specifies Origin country code.")]
        [OpenApiParameter(name: "DestinationCountryCode", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "This field specifies the destination country code.")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Get flight segment distance", Description = "Calculate flight segment distance for missing points claim")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GetSegmentDistance")]
        public async Task<HttpResponseData> GetSegmentDistance([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetSegmentDistance/{OriginCountryCode}/{DestinationCountryCode}")] HttpRequestData req, string originCountryCode, string destinationCountryCode)
        {
            return await ResultExtention<string>.ExecuteResultAsync(req, await _mediator.Send(new DistanceQuery() { OriginCountryCode = originCountryCode, DestinationCountryCode = destinationCountryCode }, CancellationToken.None));
        }              

        [OpenApiOperation(operationId: "GetCountryDetails", tags: new[] { "Reference Microservice" }, Summary = "Fetch svc reference SP data for Country Details", Description = "Endpoint to fetch reference data for Country Details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<CountryDetails>), Summary = "Get  Country Details", Description = "Get Country Details for metro area mapping")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GetCountryDetails")]
        public async Task<HttpResponseData> GetCountryDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetCountryDetails")] HttpRequestData req, string countryCode)
        {
            return await ResultExtention<IEnumerable<CountryDetails>>.ExecuteResultAsync(req, await _mediator.Send(new GetCountryDetailsQuery(), CancellationToken.None));
        }

        [OpenApiOperation(operationId: "GetCabinClassDetailsForNonVSRetro", tags: new[] { "Reference Microservice" }, Summary = "Fetch svc reference SP data for cabin class detail values", Description = "Endpoint to fetch reference data of Cabin class details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(NonVSRetroCabinClassDetails), Summary = "Get Cabin Class Details", Description = "Get cabin class details for missing point claim")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(NonVSRetroFlightDetails), Required = true, Description = "")]
        [Function("GetCabinClassDetailsForNonVSRetro")]
        public async Task<HttpResponseData> GetCabinClassDetailsForNonVSRetro([HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "GetCabinClassForNonVSRetro")] HttpRequestData req)
        {
            return await ResultExtention<NonVSRetroCabinClassDetails>.ExecuteResultAsync(req, await _mediator.Send(Json.Deserialize<GetCabinClassDetailsForNonVSRetroQuery>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [OpenApiOperation(operationId: "GetSegmentDistanceAndCountry", tags: new[] { "Reference Microservice" }, Summary = "Fetch svc reference SP data for Distance and Country Code", Description = "Endpoint to fetch reference data of Distance and Country Code", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "OriginCountryCode", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "This field specifies Origin country code.")]
        [OpenApiParameter(name: "DestinationCountryCode", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "This field specifies the destination country code.")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Get flight segment distance", Description = "Calculate flight segment distance for missing points claim")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GetSegmentDistanceAndCountry")]
        public async Task<HttpResponseData> GetSegmentDistanceAndCountry([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetSegmentDistanceAndCountry/{OriginCountryCode}/{DestinationCountryCode}")] HttpRequestData req, string originCountryCode, string destinationCountryCode)
        {
            return await ResultExtention<DistanceAndCountryCodeResponse>.ExecuteResultAsync(req, await _distanceAndCountryCodeQueryHandler.Handle(new DistanceAndCountryCodeQuery() { OriginAirportCode = originCountryCode, DestinationAirportCode = destinationCountryCode }, CancellationToken.None));
        }
    }
}
