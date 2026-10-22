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
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Common;
using VAA.EAPI.Web.Common.Entities;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Exceptions;
using VAA.EAPI.Web.Common.Serialisation;
using VAA.EAPI.Web.Search.Helpers;
using VAA.EAPI.Web.Search.Models.Amenity;
using VAA.EAPI.Web.Search.Models.CarbonIndex;
using VAA.EAPI.Web.Search.Models.Common;
using VAA.EAPI.Web.Search.Models.FlightSummary;
using VAA.EAPI.Web.Search.Models.Loyalty;
using VAA.EAPI.Web.Search.Models.Search.Request;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus;

namespace VAA.EAPI.Web.Search.Trigger.Http
{
    public sealed class SearchController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SearchController> _logger;


        public SearchController(IMediator mediator, ILogger<SearchController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "Search", tags: new[] { "Search" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Initiate search and return first slice results",
            Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SearchResults), Summary = "OK", Description = "OK")]

        [Function("Search")]
        public async Task<HttpResponseData> Search([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "search")] HttpRequestData req)
        {
            var searchReq = (SearchRequestV2)req.Body;

            var token = Utility.TryGetAuthorizationToken(req.Headers);
            searchReq.authToken = token;

            if (searchReq.Ids is null)
            {
                try
                {
                    var result = await _mediator.Send(searchReq);
                    if (result.Result != null)
                    {
                        result.Result.Slice.Id = Guid.NewGuid().ToString();
                    }

                    return await ResultExtention<SearchResultsResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<SearchResultsResponse>()
                    {
                        Status = Status.OK,
                        Value = result,
                        HeaderParameters = new()
                        {
                            new ResponseHeader()
                            {
                                Key = Constants.Application.BasketIdHeaderName,
                                Value = GetBasketId().ToString()
                            }
                        },
                    }));
                }
                catch (GraphQLException ex)
                {
                    _logger.LogError(ex, $"Exception occurred for Request {Json.Serialize(searchReq)}");

                    return await ResultExtention<SearchResultsResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<SearchResultsResponse>()
                    {
                        Status = Status.BusinessException,
                        Errors = Json.Deserialize<List<ErrorDetails>>(ex.Errors.First().Extensions.Reason),
                        IgnoreNullValues = true
                    }));
                }
            }

            else
            {
                var basketId = searchReq.Ids.Id;
                Match match = Helpers.Utility.GetSliceIDOfferID(searchReq.Ids.FareId);
                var sliceId = match.Groups["sliceId"].Value;
                var offerId = match.Groups["offerId"].Value;
                try
                {
                    var result = await _mediator.Send(new SelectAndNextRequest()
                    {
                        BasketId = basketId,
                        OfferId = offerId,
                        SliceId = sliceId,
                        slices = searchReq.slices
                    });

                    return await ResultExtention<Result>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<Result>()
                    {
                        Status = Status.OK,
                        Value = result,
                        HeaderParameters = new()
                    {
                        new ResponseHeader()
                        {
                            Key = Constants.Application.BasketIdHeaderName,
                            Value = basketId
                        }
                    },
                    }));
                }
                catch (GraphQLException ex)
                {
                    return await ResultExtention<SearchResultsResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<SearchResultsResponse>()
                    {
                        Status = Status.BusinessException,
                        Errors = Json.Deserialize<List<ErrorDetails>>(ex.Errors.First().Extensions.Reason)
                    }));
                }
            }
        }

        [OpenApiOperation(operationId: "SelectAndNext", tags: new[] { "SelectAndNext" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Select a flight and return next slice search results",
            Description = "Adds the selected flight to the 'basket'. If the search request has multiple slices, and there are more slices to select, the next slice search results are returned.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Result), Summary = "OK", Description = "OK")]
        [OpenApiParameter("basketId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("offerId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("sliceId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]

        [Function("SelectAndNextRD")]
        public async Task<HttpResponseData> SelectAndNext([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "search/{basketId}/{sliceId}/{offerId}")] HttpRequestData req, string basketId, string offerId, string sliceId)
        {
            Result result = new()
            {
                Slice = new()
                {
                    Id = basketId,
                    FareId = string.Join('|', sliceId, offerId)
                }
            };

            return await ResultExtention<Result>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<Result>()
            {
                Status = Status.OK,
                Value = result,
            }));
        }

        [OpenApiOperation(operationId: "SelectAndSummary", tags: new[] { "SelectAndSummary" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Select a flight and return the flight summary",
            Description = "Adds the selected flight to the 'basket'. If the search request has multiple slices, and there are no more slices to select, the flight summary is returned.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Models.Search.Response.TravelPlusSummary.Summary), Summary = "OK", Description = "OK")]
        [OpenApiParameter("basketId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("offerId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("sliceId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]

        [Function("SelectAndSummary")]
        public async Task<HttpResponseData> SelectAndSummary([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{basketId}/summary/{sliceId}/{offerId}")] HttpRequestData req, string basketId, string offerId, string sliceId)
        {

            var sliceData = new SliceData(sliceId);

            Models.Search.Response.TravelPlusSummary.Summary summary = new()
            {
                Id = basketId,
                FareId = string.Join('|', sliceId, offerId),
                Links = GetSummaryLinks(sliceData.TotalSlices, sliceData.CurrentSlice, basketId, offerId)
            };

            return await ResultExtention<Models.Search.Response.TravelPlusSummary.Summary>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<Models.Search.Response.TravelPlusSummary.Summary>()
            {
                Status = Status.OK,
                Value = summary,
            }));
        }

        [OpenApiOperation(operationId: "Summary", tags: new[] { "Summary" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Select a flight and return the flight summary",
            Description = "Adds the selected flight to the 'basket'. If the search request has multiple slices, and there are no more slices to select, the flight summary is returned.")]
        [OpenApiParameter(name: "origin", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "origin", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "destination", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "destination", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "departure", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "departure", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "passengers", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "passengers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Models.Search.Response.TravelPlusSummary.Summary), Summary = "OK", Description = "OK")]
        [OpenApiParameter("id", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("fareId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]

        [Function("Summary")]
        public async Task<HttpResponseData> Summary([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "summary/{id}/{fareId}")] HttpRequestData req, string id, string fareId)
        {
            var token = Utility.TryGetAuthorizationToken(req.Headers);

            var basketId = id;
            var match = Helpers.Utility.GetSliceIDOfferID(fareId);
            var sliceId = match.Groups["sliceId"].Value;
            var offerId = match.Groups["offerId"].Value;

            var result = await _mediator.Send(new SelectAndSummaryRequest() { BasketId = basketId, OfferId = offerId, SliceId = sliceId, AuthToken = token });

            return await ResultExtention<Models.Search.Response.TravelPlusSummary.Summary>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<Models.Search.Response.TravelPlusSummary.Summary>()
            {
                Status = Status.OK,
                Value = result,
                HeaderParameters = new()
                {
                    new ResponseHeader()
                    {
                        Key = Constants.Application.BasketIdHeaderName,
                        Value = basketId
                    }
                },
            }));
        }


        [OpenApiOperation(operationId: "SummaryV2", tags: new[] { "SummaryV2" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Select a flight and return the flight summary",
            Description = "Adds the selected flight to the 'basket'. If the search request has multiple slices, and there are no more slices to select, the flight summary is returned.")]
        [OpenApiParameter(name: "origin", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "origin", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "destination", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "destination", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "departure", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "departure", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "passengers", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "passengers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Models.Search.Response.TravelPlusSummary.Summary), Summary = "OK", Description = "OK")]
        [OpenApiParameter("id", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("fareId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]

        [Function("SummaryV2")]
        public async Task<HttpResponseData> SummaryV2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "summary/{id}/{fareId}")] HttpRequestData req, string id, string fareId)
        {
            var token = Utility.TryGetAuthorizationToken(req.Headers);
            var summaryReqeust = (SummaryRequest)req.Body;

            var basketId = id;
            var match = Helpers.Utility.GetSliceIDOfferID(fareId);
            var sliceId = match.Groups["sliceId"].Value;
            var offerId = match.Groups["offerId"].Value;

            var result = await _mediator.Send(new SelectAndSummaryRequest() { BasketId = basketId, OfferId = offerId, SliceId = sliceId, AuthToken = token, SummaryLink = summaryReqeust.SummaryLink });

            return await ResultExtention<Models.Search.Response.TravelPlusSummary.Summary>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<Models.Search.Response.TravelPlusSummary.Summary>()
            {
                Status = Status.OK,
                Value = result,
                HeaderParameters = new()
                {
                    new ResponseHeader()
                    {
                        Key = Constants.Application.BasketIdHeaderName,
                        Value = basketId
                    }
                },
            }));
        }

        [OpenApiOperation(operationId: "SummaryDeeplink", tags: new[] { "SummaryDeeplink" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Deeplink straight to the flight summary",
            Description = "Given an itinerary, add the selected flight to the 'basket' and returns the flight summary.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Models.Search.Response.TravelPlusSummary.Summary), Summary = "OK", Description = "OK")]
        [Function("SummaryDeeplink")]
        public async Task<HttpResponseData> SummaryDeeplink([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "summary-deeplink")] HttpRequestData req, string id, string fareId)
        {
            var summaryReqeust = (SummaryDeeplinkRequest)req.Body;

            var result = await _mediator.Send(summaryReqeust);

            return await ResultExtention<Models.Search.Response.TravelPlusSummary.Summary>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<Models.Search.Response.TravelPlusSummary.Summary>()
            {
                Status = Status.OK,
                Value = result
            }));
        }

        [OpenApiOperation(operationId: "UpgradeSummary", tags: new[] { "UpgradeSummary" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Returns upsell cabin summary",
            Description = "Adds the selected flight to the 'basket'. Upgrades the selected flight to higher cabin and performs summary.")]
        [OpenApiParameter(name: "origin", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "origin", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "destination", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "destination", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "departure", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "departure", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "passengers", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "passengers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Models.Search.Response.TravelPlusSummary.Summary), Summary = "OK", Description = "OK")]
        [OpenApiParameter("id", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("sliceId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("offerId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]

        [Function("UpgradeSummary")]
        public async Task<HttpResponseData> UpgradeSummary([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "summary/{id}/{sliceId}/{offerId}")] HttpRequestData req, string id, string sliceId, string offerId)
        {
            var token = Utility.TryGetAuthorizationToken(req.Headers);

            var result = await _mediator.Send(new SelectAndSummaryRequest() { BasketId = id, OfferId = offerId, SliceId = sliceId, AuthToken = token });

            return await ResultExtention<Models.Search.Response.TravelPlusSummary.Summary>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<Models.Search.Response.TravelPlusSummary.Summary>()
            {
                Status = Status.OK,
                Value = result,
                HeaderParameters = new()
                {
                    new ResponseHeader()
                    {
                        Key = Constants.Application.BasketIdHeaderName,
                        Value = id
                    }
                },
            }));
        }

        [OpenApiOperation(operationId: "UpgradeSummaryV2", tags: new[] { "UpgradeSummaryV2" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Returns upsell cabin summary",
            Description = "Adds the selected flight to the 'basket'. Upgrades the selected flight to higher cabin and performs summary.")]
        [OpenApiParameter(name: "origin", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "origin", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "destination", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "destination", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "departure", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "departure", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "passengers", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "passengers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Models.Search.Response.TravelPlusSummary.Summary), Summary = "OK", Description = "OK")]
        [OpenApiParameter("id", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("sliceId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("offerId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]

        [Function("UpgradeSummaryV2")]
        public async Task<HttpResponseData> UpgradeSummaryV2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "summary/{id}/{sliceId}/{offerId}")] HttpRequestData req, string id, string sliceId, string offerId)
        {
            var token = Utility.TryGetAuthorizationToken(req.Headers);
            var summaryReqeust = (SummaryRequest)req.Body;

            var result = await _mediator.Send(new SelectAndSummaryRequest() { BasketId = id, OfferId = offerId, SliceId = sliceId, AuthToken = token, SummaryLink = summaryReqeust.SummaryLink });

            return await ResultExtention<Models.Search.Response.TravelPlusSummary.Summary>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<Models.Search.Response.TravelPlusSummary.Summary>()
            {
                Status = Status.OK,
                Value = result,
                HeaderParameters = new()
                {
                    new ResponseHeader()
                    {
                        Key = Constants.Application.BasketIdHeaderName,
                        Value = id
                    }
                },
            }));
        }

        [OpenApiOperation(operationId: "FareInformation", tags: new[] { "FareInformation" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Provides the Fare Information for the select flights.",
            Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(FlightInformation), Summary = "OK", Description = "OK")]
        [OpenApiParameter("offerId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]

        [Function("FareInformation")]
        public async Task<HttpResponseData> FareInformation([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "information/{offerId}")] HttpRequestData req, string offerId)
        {
            try
            {
                var result = await _mediator.Send(new FareInfoRequest() { OfferId = offerId });
                return await ResultExtention<FlightInformation>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<FlightInformation>()
                {
                    Status = Status.OK,
                    Value = result,
                }));
            }
            catch (GraphQLException ex)
            {
                if (ex.Errors.Any(x => x.Extensions.Code == ((int)HttpStatusCode.NoContent).ToString()))
                {
                    HttpResponseData httpResponseData = HttpResponseData.CreateResponse(req);
                    httpResponseData.StatusCode = HttpStatusCode.NoContent;
                    return httpResponseData;
                }

                throw;
            }
        }

        [OpenApiOperation(operationId: "BookingSessionQuery", tags: new[] { "BookingSessionQuery" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Check to see if a booking session has been created.",
            Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Booking), Summary = "OK", Description = "OK")]
        [OpenApiParameter("basketId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("fareId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]

        [Function("BookingSessionQuery")]
        public async Task<HttpResponseData> BookingSessionQuery([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{basketId}/booking/{fareId}")] HttpRequestData req, string basketId, string fareId)
        {
            var match = Helpers.Utility.GetSliceIDOfferID(fareId);
            var sliceId = match.Groups["sliceId"].Value;
            var offerId = match.Groups["offerId"].Value;

            var result = await _mediator.Send(new BookingSessionRequest() { BasketId = basketId, SliceId = sliceId, OfferId = offerId });

            return await ResultExtention<Booking>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<Booking>()
            {
                Status = Status.OK,
                Value = result,
            }));
        }

        [OpenApiOperation(operationId: "BookingSessionCreate", tags: new[] { "BookingSessionCreate" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Create a booking session.",
            Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Booking), Summary = "OK", Description = "OK")]
        [OpenApiParameter("basketId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("fareId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]

        [Function("BookingSessionCreate")]
        public async Task<HttpResponseData> BookingSessionCreate([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{basketId}/create/{fareId}")] HttpRequestData req, string basketId, string fareId)
        {
            var match = Helpers.Utility.GetSliceIDOfferID(fareId);
            var sliceId = match.Groups["sliceId"].Value;
            var offerId = match.Groups["offerId"].Value;

            var result = await _mediator.Send(new BookingSessionRequest() { BasketId = basketId, SliceId = sliceId, OfferId = offerId });

            return await ResultExtention<Booking>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<Booking>()
            {
                Status = Status.OK,
                Value = result,
            }));
        }

        [OpenApiOperation(operationId: "Dynamic Cabin Information V2", tags: new[] { "Dynamic Cabin Information V2" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Provides the Cabin Amenities Information for the select offer ID and passengerCounts.",
            Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CabinAmenity), Summary = "OK", Description = "OK")]
        [OpenApiParameter("passengerCounts", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("offerId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]

        [Function("DynamicCabinV2")]
        public async Task<HttpResponseData> DynamicCabinV2([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{passengerCounts}/{offerId}/dynamicCabin")] HttpRequestData req, string passengerCounts, string offerId)
        {
            string passengerCountsRQ = RegexCompiled.IsMatch(passengerCounts, "^([atci][0-9]+){4}$", RegexOptions.IgnoreCase)
                ? passengerCounts
                : throw new ArgumentException($"\"{passengerCounts}\" is not a valid passengerCounts string");

            var result = await _mediator.Send
                (
                    new DynamicCabinRequest()
                    {
                        OfferId = offerId,
                        PassengerCounts = passengerCountsRQ
                    },
                        cancellationToken: CancellationToken.None
                );

            return await ResultExtention<CabinAmenity>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<CabinAmenity>()
            {
                Status = Status.OK,
                Value = result,
            }));
        }

        [OpenApiOperation(operationId: "Carbin Index Information", tags: new[] { "Carbin Index Information" }, Visibility = OpenApiVisibilityType.Important,
            Summary = "Provides the Carbon Index Information for all the segments and cabins against the offer ID",
            Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<CarbonIndexResponse>), Summary = "OK", Description = "OK")]
        [OpenApiParameter("passengerCounts", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("offerId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [Function("CarbonIndex")]
        public async Task<HttpResponseData> CarbonIndex([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "carbonindex/{offerId}/{passengerCounts}")] HttpRequestData req, string passengerCounts, string offerId)
        {
            var result = await _mediator.Send(new CarbonIndexRequest() { OfferId = offerId, PassengerCounts = passengerCounts }, cancellationToken: CancellationToken.None);
            return await ResultExtention<IEnumerable<CarbonIndexResponse>>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<IEnumerable<CarbonIndexResponse>>()
            {
                Status = Status.OK,
                Value = result,
            }));
        }

        [OpenApiOperation(operationId: "MemberDetails", tags: new[] { "MemberDetails" }, Visibility = OpenApiVisibilityType.Important,
           Summary = "Returns member details of logged in Flying Club customer",
           Description = "Returns member details of logged in Flying Club customer.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Models.Loyalty.MemberDetailsResponse), Summary = "OK", Description = "OK")]

        [Function("MemberDetails")]
        public async Task<HttpResponseData> FetchMemberDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getMemberDetails")] HttpRequestData req)
        {
            var token = Utility.GetAuthorizationToken(reqHeaders: req.Headers, tokenValueWithBearer: false);

            return await ResultExtention<Models.Loyalty.MemberDetailsResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<Models.Loyalty.MemberDetailsResponse>()
            {
                Status = Status.OK,
                Value = await _mediator.Send(new MemberDetailsRequest() { AuthToken = token }, cancellationToken: CancellationToken.None),
            }));
        }


        [OpenApiOperation(operationId: "Sort and Filter Data", tags: new[] { "Sort and Filter Data" }, Visibility = OpenApiVisibilityType.Important,
           Summary = "Sort and Filter Data",
           Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CabinAmenity), Summary = "OK", Description = "OK")]
        [OpenApiParameter("basketId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("fareId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("selectedSort", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("airlinesFilter", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("departureTimesFilter", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("connectionsFilter", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]
        [OpenApiParameter("cabinsFilter", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true)]

        [Function("sort")]
        public async Task<HttpResponseData> Sort([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "{basketId}/sort/{fareId}/{selectedSort}/{airlinesFilter}/{departureTimesFilter}/{connectionsFilter}/{cabinsFilter}")] HttpRequestData req, string basketId, string fareId, string selectedSort, string airlinesFilter, string departureTimesFilter, string connectionsFilter, string cabinsFilter)
        {
            var token = Utility.TryGetAuthorizationToken(req.Headers);
            var match = Helpers.Utility.GetSliceIDOfferID(fareId);

            var sliceId = match.Length > 0 ? match.Groups["sliceId"].Value : fareId;
            var offerId = match.Length > 0 ? match.Groups["offerId"].Value : string.Empty;

            var sortReq = new SortRequest()
            {
                BasketId = basketId,
                SelectedSort = selectedSort,
                Filters = new FilterRequest
                {
                    Airlines = airlinesFilter,
                    DepartureTimes = departureTimesFilter,
                    Connections = connectionsFilter,
                    Cabins = cabinsFilter
                },
                SliceId = sliceId,
                OfferId = offerId,
                AuthToken = token
            };

            var result = await _mediator.Send(sortReq, cancellationToken: CancellationToken.None);

            if (offerId.IsNullOrEmpty())
            {
                return await ResultExtention<SearchResultsResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<SearchResultsResponse>()
                {
                    Status = Status.OK,
                    Value = result,
                }));
            }
            else
            {
                return await ResultExtention<Result>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<Result>()
                {
                    Status = Status.OK,
                    Value = result.Result,
                }));
            }
        }

        private static IEnumerable<Models.Search.Response.TravelPlusSummary.Link> GetSummaryLinks(int totalOnDs, int currentSlice, string basketID, string offerID)
        {
            if (basketID != null)
            {
                var urlPrefix = ConfigReader.Get(Constants.Environment.PostbackUrl);
                return new Models.Search.Response.TravelPlusSummary.Link[]
                {
                    new Models.Search.Response.TravelPlusSummary.Link()
                    {
                        Href = $"{urlPrefix}/{basketID}/summary/slice{totalOnDs}.{currentSlice}/{offerID}",
                        Rel = "self"
                    },
                    new Models.Search.Response.TravelPlusSummary.Link()
                    {
                        Href = $"{urlPrefix}/summary/{basketID}/slice{totalOnDs}.{currentSlice}|{offerID}",
                        Rel = "searchsummary"
                    },
                    new Models.Search.Response.TravelPlusSummary.Link()
                    {
                        Href = $"{urlPrefix}/{basketID}/booking/slice{totalOnDs}.{currentSlice}/{offerID}",
                        Rel = "booking"
                    },
                    new Models.Search.Response.TravelPlusSummary.Link()
                    {
                        Href =  $"{urlPrefix}/{basketID}/booking/slice{totalOnDs}.{currentSlice}/{offerID}",
                        Rel = "create"
                    }
                };
            }
            return Enumerable.Empty<Models.Search.Response.TravelPlusSummary.Link>();
        }

        private static Guid GetBasketId()
        {
            if (AsyncContext.Current.Context.Items.TryGetValue(Constants.Application.BasketIdHeaderContainer, out var headers))
            {
                if (((HttpResponseHeaders)headers).TryGetValues(Constants.Application.BasketIdHeaderName, out var BasketId))
                {
                    Guid.TryParse(BasketId.First(), out Guid result);
                    return result;
                }
            }
            return new Guid();
        }
    }
}
