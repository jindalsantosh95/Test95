using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.ContactTracing;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.EmergencyContact;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SavePassport;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SaveReturnDate;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SelectPassenger;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Services
{
    public class CheckInServiceProvider : ICheckInServiceProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CheckInMsOcpKey) } };

        protected readonly IHttpConnector _connector;
        private readonly ILogger<CheckInServiceProvider> _logger;
        private readonly ILogAnalytics? _logAnalytics;
        private readonly ITelemetryService? _telemetryService;

        public CheckInServiceProvider(
            IHttpConnector connector,
            ILogger<CheckInServiceProvider>? logger = null,
            ILogAnalytics? logAnalytics = null,
            ITelemetryService? telemetryService = null)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _logger = logger ?? NullLogger<CheckInServiceProvider>.Instance;
            _logAnalytics = logAnalytics;
            _telemetryService = telemetryService;
        }

        public async Task<CheckInResponse> InitiateCheckInAsync(PaxOps.Models.CheckInRequest checkInRequest, bool mapHttpFailureToResponse = true)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}{Common.Constants.Application.GetCheckInDetails}";
            return await GetCheckInAsync(
                uri,
                HttpMethod.Post,
                Json.Serialize(checkInRequest),
                nameof(InitiateCheckInAsync),
                LoggingFailureContext.INITIATE_CHECKIN,
                checkInRequest.PnrLocator,
                checkInRequest.OriginCode,
                mapHttpFailureToResponse: mapHttpFailureToResponse);
        }

        public async Task<CheckInResponse> SelectPassengersAsync(SelectPassengerServiceRequest selectPassengerRequest, string? checkInId = null, bool mapHttpFailureToResponse = true)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}{Common.Constants.Application.SelectPassengers}";
            return await GetCheckInAsync(
                uri,
                HttpMethod.Post,
                Json.Serialize(selectPassengerRequest),
                nameof(SelectPassengersAsync),
                LoggingFailureContext.SELECT_PASSENGER,
                selectPassengerRequest.PnrLocator,
                selectPassengerRequest.OriginCode,
                checkInId,
                mapHttpFailureToResponse);
        }

        public async Task<CheckInResponse> SavePassportAsync(SavePassportServiceRequest savePassportRequest, string? checkInId = null)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}{Common.Constants.Application.SavePassport}";
            return await GetCheckInAsync(uri, HttpMethod.Post, Json.Serialize(savePassportRequest), nameof(SavePassportAsync), LoggingFailureContext.SAVE_PASSPORT, checkInId: checkInId);
        }

        public async Task<CheckInResponse> SaveContactTracingAsync(SaveContactTracingServiceRequest contactTracingRequest, string? checkInId = null)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}{Common.Constants.Application.SaveContactTracing}";
            return await GetCheckInAsync(uri, HttpMethod.Post, Json.Serialize(contactTracingRequest), nameof(SaveContactTracingAsync), LoggingFailureContext.SAVE_CONTACT_TRACING, checkInId: checkInId);
        }

        public async Task<CheckInResponse> SaveEmergencyContactDetails(EmergencyContactServiceRequest emergencyContactRequest, string? checkInId = null)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}{Common.Constants.Application.SaveEmergencyContact}";
            return await GetCheckInAsync(uri, HttpMethod.Post, Json.Serialize(emergencyContactRequest), nameof(SaveEmergencyContactDetails), LoggingFailureContext.SAVE_EMERGENCY_CONTACT, checkInId: checkInId);
        }

        public async Task<CheckInResponse> SaveReturnDateAsync(SaveReturnDateServiceRequest saveReturnDateRequest, string? checkInId = null)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}{Common.Constants.Application.SaveReturnDate}";
            return await GetCheckInAsync(uri, HttpMethod.Post, Json.Serialize(saveReturnDateRequest), nameof(SaveReturnDateAsync), LoggingFailureContext.SAVE_RETURN_DATE, checkInId: checkInId);
        }

        public async Task<Infrastructure.Models.CheckInMS.BoardingPassResponse> GenerateBoardingPassAsync(BoardingPassServiceRequest request)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}{Common.Constants.Application.BoardingPassDetails}";
            return await GetBoardingPassAsync(uri, HttpMethod.Post, Json.Serialize(request), nameof(GenerateBoardingPassAsync));
        }

        public async Task<Infrastructure.Models.CheckInMS.BoardingPassResponse> GetBoardingPassByCheckInIdAsync(string travelPartyId)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}{Common.Constants.Application.GetBoardingPassDetails}";
            return await GetBoardingPassAsync(uri, HttpMethod.Post, Json.Serialize(new BoardingPassRequest { TravelPartyId = travelPartyId }), nameof(GetBoardingPassByCheckInIdAsync));
        }

        public async Task<Infrastructure.Models.CheckInMS.BoardingPassResponse> RetrieveBoardingPassAsync(BoardingPassRequest boardingPassRequest)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}{Common.Constants.Application.RetrieveBoardingPass}";
            return await GetBoardingPassAsync(uri, HttpMethod.Post, Json.Serialize(boardingPassRequest), nameof(RetrieveBoardingPassAsync));
        }

        public async Task<CheckInResponse> GetCheckInResponseAsync(string travelPartyId)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}{Common.Constants.Application.GetCheckInDetails}/{travelPartyId}";
            return (await _connector.SendAsync<CheckInResponse>(uri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task LogTransactionSummaryAsync(Models.TransactionSummaryRequest request)
        {
            if (request is null)
                return;

            try
            {
                var uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}{Common.Constants.Application.LogTransactionSummary}";
                using var response = await _connector.SendAsync(uri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(request), true);
            }
            catch (HttpRequestException)
            {
            }
        }

        private async Task<CheckInResponse> GetCheckInAsync(
            string uri,
            HttpMethod method,
            string requestBody,
            string operation,
            LoggingFailureContext? failureContext = null,
            string? pnrLocator = null,
            string? originCode = null,
            string? checkInId = null,
            bool mapHttpFailureToResponse = true)
        {
            CheckInResponse response;

            try
            {
                response = (await _connector.SendAsync<CheckInResponse>(uri, _headers, string.Empty, method, requestBody)).Response;
            }
            catch (HttpRequestException exception)
            {
                var errorCode = exception.StatusCode is { } statusCode
                    ? $"HTTP_{(int)statusCode}"
                    : "HTTP_CONNECTION_ERROR";

                _logger.LogError(
                    "CheckIn microservice request failed for {Operation} with HTTP status {HttpStatusCode}",
                    operation,
                    exception.StatusCode);

                if (!mapHttpFailureToResponse)
                {
                    throw;
                }

                response = new CheckInResponse
                {
                    OnlineCheckInStatus = new OnlineCheckInStatus
                    {
                        Available = false,
                        Code = errorCode,
                        Title = Application.OnlieCheckInFailDefaultTiTle,
                        Message = Application.OnlieCheckInFailDefaultMessage
                    }
                };
            }

            if (failureContext is { } context && response?.OnlineCheckInStatus?.Available == false)
            {
                await TrackRefusalAsync(response, context, pnrLocator, originCode, checkInId);
            }

            return response;
        }

        private async Task<Infrastructure.Models.CheckInMS.BoardingPassResponse> GetBoardingPassAsync(string uri, HttpMethod method, string requestBody, string operation)
        {
            try
            {
                return (await _connector.SendAsync<Infrastructure.Models.CheckInMS.BoardingPassResponse>(uri, _headers, string.Empty, method, requestBody)).Response;
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(
                    "CheckIn microservice boarding pass request failed for {Operation} with HTTP status {HttpStatusCode}",
                    operation,
                    exception.StatusCode);

                throw;
            }
        }

        private async Task TrackRefusalAsync(CheckInResponse response, LoggingFailureContext failureContext, string? pnrLocator, string? originCode, string? checkInId)
        {
            if (response?.OnlineCheckInStatus?.Available != false)
            {
                return;
            }

            var onlineCheckInStatus = (Models.OnlineCheckInStatus)response.OnlineCheckInStatus;
            var resolvedTravelPartyId = response.TravelPartyId;
            var resolvedPnrLocator = pnrLocator ?? response.Reference?.PnrLocator;
            var resolvedOriginCode = originCode ?? response.CurrentSlice?.Origin?.Code;
            var shouldCreateTransactionSummaryFromCheckInId = resolvedTravelPartyId.IsNullOrEmpty() == true && !checkInId.IsNullOrEmpty();

            if (!checkInId.IsNullOrEmpty())
            {
                if (resolvedPnrLocator.IsNullOrEmpty())
                    resolvedPnrLocator = await CheckInIdHelper.GetPnrLocator(checkInId);

                if (resolvedOriginCode.IsNullOrEmpty())
                    resolvedOriginCode = await CheckInIdHelper.GetOriginCode(checkInId);
            }

            _ = _logAnalytics?.CheckInAnalyticsLogger(
                onlineCheckInStatus,
                failureContext,
                new Models.CheckInLogDetails { PnrLocator = resolvedPnrLocator, Origin = resolvedOriginCode },
                LoggingCheckinEvent.CHECKIN_END_REFUSED);

            if (_telemetryService is not null)
            {
                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(
                    LoggingCheckinEvent.CHECKIN_END_REFUSED,
                    Data: await TelemetryHelper.CheckinEndRefused(
                        onlineCheckInStatus,
                        response,
                        null,
                        null,
                        resolvedPnrLocator,
                        checkInId,
                        failureContext)));
            }

            if (resolvedTravelPartyId.IsNullOrEmpty() == false)
            {
                _ = LogTransactionSummaryAsync((Models.TransactionSummaryRequest)(
                    resolvedTravelPartyId,
                    resolvedPnrLocator,
                    resolvedOriginCode,
                    onlineCheckInStatus.Code));
            }
            else if (shouldCreateTransactionSummaryFromCheckInId)
            {
                _ = LogTransactionSummaryAsync(
                    await Models.TransactionSummaryRequest.CreateAsync(checkInId, onlineCheckInStatus.Code));
            }
        }
    }
}
