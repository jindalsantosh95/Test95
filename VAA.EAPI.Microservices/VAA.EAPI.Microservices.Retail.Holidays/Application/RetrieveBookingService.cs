using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips;
using VAA.EAPI.Microservices.Retail.Holidays.Helpers;

namespace VAA.EAPI.Microservices.Retail.Holidays.Application
{
    public sealed partial class RetrieveBookingService(IHttpConnector httpConnector, IOAuthTokenProvider oauthTokenProvider, ILogger<RetrieveBookingService> logger) : IRetrieveBookingService
    {
        private const string UrlConfig = "Holidays:RetrieveBookingService:Url";
        private const string EntraScopeConfig = "Holidays:EntraScope";
        private const string AuthorizationProviderConfig = "Holidays:AuthorizationProvider";
        private const string TbxUserIdClaimType = "tbx-UserId";

        private const string AuthorizationProviderHeader = "Authorization-Provider";
        private const string AuthorizationHeader = "Authorization";
        private const string TBXWebUserIdHeader = "tbx-web-user-id";

        private readonly string AuthorizationProviderHeaderValue = ConfigReader.Try(AuthorizationProviderConfig, "AZURE");

        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly IOAuthTokenProvider _oauthTokenProvider = oauthTokenProvider ?? throw new ArgumentNullException(nameof(oauthTokenProvider));
        private readonly ILogger<RetrieveBookingService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


        public async Task<Domain.Trips.TripsDetailsResponse> Handle(TripsDetailsRequest request, CancellationToken cancellationToken)
        {
            TripsDetailsResponse tripDetailsResponse = new();

            var scope = $"{ConfigReader.Get(EntraScopeConfig).TrimEnd('/')}/{ConfigReader.Get(Common.Constants.Environment.EapiMsClientID)}";
            var token = await _oauthTokenProvider.AcquireTokenAsync(scope);
            var tbxUserId = token.ExtractJwtClaim(TbxUserIdClaimType);
            var url = $"{ConfigReader.Get(UrlConfig).TrimEnd('/')}/{request.BookingReference}?expand=all&userId={tbxUserId}&cliId=-1";
            var headers = new Dictionary<string, string>
                {
                    { AuthorizationProviderHeader, AuthorizationProviderHeaderValue },
                    { AuthorizationHeader, token },
                    { TBXWebUserIdHeader, tbxUserId.ToString() }
                };

            var response = await _httpConnector.SendAsync(uri: url, headers: headers, bearerToken: null, HttpMethod.Get, content: string.Empty);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var jObjectTboxResponse = Json.Deserialize<JObject>(content);
                var matchResult = MatchLeadTraveller(jObjectTboxResponse, request.Surname, request.DateOfBirth);

                if (matchResult == false)
                {
                    tripDetailsResponse.Booking = null;
                    tripDetailsResponse.Status = (int)HttpStatusCode.BadRequest;
                    tripDetailsResponse.Errors = ErrorMapper.MapBusinessExceptionToEAPIException(Constants.Application.InvalidRequestParamsErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);

                    return tripDetailsResponse;
                }

                tripDetailsResponse = (TripsDetailsResponse)jObjectTboxResponse;


                if (tripDetailsResponse?.Booking == null)
                {
                    _logger.LogError("Error creating booking response for BookingReference: {BookingReference}. Errors: {Errors}", request.BookingReference, Json.Serialize(tripDetailsResponse?.Errors));
                }

                else if (IsExpiredHolidayBooking(tripDetailsResponse!.Booking))
                {
                    tripDetailsResponse.Booking = null;
                    tripDetailsResponse.Status = (int)HttpStatusCode.BadRequest;
                    tripDetailsResponse.Errors = ErrorMapper.MapBusinessExceptionToEAPIException(Constants.Application.ExpiredBookingErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
                }

                return tripDetailsResponse;
            }

            _logger.LogError("Failed to retrieve holiday booking details for BookingReference: {BookingReference}. Status: {StatusCode}, CorrelationId: {CorrelationId}, Response: {Content}", request.BookingReference, response.StatusCode, AsyncContext.Current.CorrelationId, content);

            return default;
        }

        public async Task<TripListResponseStore> HandleList(IEnumerable<TripsDetailsRequest> requestList, CancellationToken cancellationToken)
        {
            ConcurrentBag<Tuple<TripsDetailsRequest, TripsDetailsResponse>> holidayListResponse = [];

            await Parallel.ForEachAsync(requestList, cancellationToken, async (holidayRequest, cancellationToken) =>
            {
                try
                {
                    var result = await Handle(holidayRequest, cancellationToken);

                    var holidayResponse = new TripsDetailsResponse
                    {
                        Booking = result?.Booking,
                        Status = result != null && result.Booking != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                        Errors = result == null || result?.Booking == null ? result?.Errors ?? ErrorMapper.MapBusinessExceptionToEAPIException(Constants.Application.TboxErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath) : null,
                    };

                    if (holidayResponse?.Errors == null || holidayResponse?.Errors?.Any(x => x.Code == Constants.Application.HolidaysApiRequestValidationErrorCode) == false)
                    {
                        holidayListResponse.Add(Tuple.Create(holidayRequest, holidayResponse));
                    }
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to retrieve holiday booking details for BookingReference: {holidayRequest?.BookingReference}. CorrelationId: {AsyncContext.Current.CorrelationId}");

                    var response = new TripsDetailsResponse()
                    {
                        Booking = null,
                        Status = (int)HttpStatusCode.InternalServerError,
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(Constants.Application.HolidayMSGenericErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    };

                    holidayListResponse.Add(Tuple.Create(holidayRequest, response));

                    if (cancellationToken.IsCancellationRequested)
                        return;
                }
            });

            return (TripListResponseStore)holidayListResponse;
        }

        private static bool IsExpiredHolidayBooking(Booking? booking)
        {
            var returnDate = booking?.Summary?.TravelDates?.EndDate ?? booking?.Summary?.ReturnDate;

            if (returnDate is not { } holidayReturnDate)
                return false;

            return (DateTime.UtcNow.Date - holidayReturnDate.Date).Days >= 7;
        }

        public static bool MatchLeadTraveller(JObject jsonResponse, string requestLastName, DateOnly requestDob)
        {
            var dataArray = jsonResponse["data"] as JArray;
            var travellers = dataArray?[0]?["travellers"] as JArray;

            return travellers?
                .FirstOrDefault(t => t["lead"]?.Value<bool>() == true)
                ?.Value<JToken>("profile") is JToken profile
                && string.Equals(profile["lastName"]?.Value<string>().NormalizeName(), requestLastName.NormalizeName(), StringComparison.OrdinalIgnoreCase)
                && DateOnly.TryParse(profile["dob"]?.Value<string>(), out DateOnly parsedDob)
                && parsedDob == requestDob;
        }
    }
}
