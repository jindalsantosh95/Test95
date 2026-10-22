using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.WalletMS;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;


namespace VAA.EAPI.GraphQL.Domain.PaxOps.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class GetBoardigPassQuery
    {
        private ICheckInServiceProvider _checkInQueryProvider;
        private ITripsDetailsQueryProvider _tripsDetailsQueryProvider;
        private readonly ITelemetryService _telemetryService;
        private readonly IReferenceServiceProvider _referenceServiceProvider;
        private readonly IWalletServiceProvider _walletServiceProvider;
        private readonly ILogger<GetBoardigPassQuery> _logger;


        public GetBoardigPassQuery(ICheckInServiceProvider checkInQueryProvider, ITripsDetailsQueryProvider tripsDetailsQueryProvider, ITelemetryService telemetryService, IReferenceServiceProvider referenceServiceProvider, IWalletServiceProvider walletServiceProvider, ILogger<GetBoardigPassQuery> logger)
        {
            _checkInQueryProvider = checkInQueryProvider ?? throw new ArgumentNullException(nameof(checkInQueryProvider));
            _tripsDetailsQueryProvider = tripsDetailsQueryProvider ?? throw new ArgumentNullException(nameof(tripsDetailsQueryProvider));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _walletServiceProvider = walletServiceProvider ?? throw new ArgumentNullException(nameof(walletServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("GetBoardingPass")]
        [GraphQLDescription("Gets the boarding passes for a given booking")]
        public async Task<BoardingPassResponse> GetBoardingPass(string pnrLocator, string originCode, string givenNames, string surname, IResolverContext context)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(givenNames);
            ArgumentException.ThrowIfNullOrWhiteSpace(surname);
            BoardingPassResponse response = null;

            try
            {
                var tripsBookingResponse = await _tripsDetailsQueryProvider.GetTripDetails(
                    new TripsDetailsRequest
                    {
                        Using = RequestOptionCodeType.PNR,
                        ConfirmationNum = pnrLocator,
                        DepartureAirportCode = originCode,
                        GivenNames = givenNames,
                        Surname = surname
                    });

                if (tripsBookingResponse?.Passengers == null)
                {
                    _logger.LogError("GetBoardingPass: No passengers found for PNR {pnrLocator}, origin {originCode}", pnrLocator, originCode);

                    context.ReportError(
                        ErrorBuilder.New()
                        .SetMessage(Common.Constants.Application.BoardingPassFailDefaultMessage)
                        .SetExtension("code", "500")
                        .Build());

                    _ = _telemetryService.TrackTelemetry((TelemetryEvent)(
                        LoggingCheckinEvent.BOARDING_PASS_FAILED,
                        Data: await TelemetryHelper.DayOfTravel(pnrLocator, originCode, givenNames, surname, null, "No passengers")));

                    return null;
                }

                var checkedInPassengers = TripsHelper.GetCheckedInPassengers(tripsBookingResponse, originCode);

                var ssciBoardingPasses = checkedInPassengers.Any()
                        ? await _checkInQueryProvider.RetrieveBoardingPassAsync(new BoardingPassRequest
                        {
                            PNRLocator = pnrLocator,
                            OriginCode = originCode
                        })
                        : new Infrastructure.Models.CheckInMS.BoardingPassResponse();

                response = TripsHelper.UpdateBoardingPassData(tripsBookingResponse, (BoardingPassResponse)ssciBoardingPasses, checkedInPassengers, originCode);

                if ((ssciBoardingPasses?.CheckInStatus == PassengersCheckInStatus.COMPLETED || ssciBoardingPasses?.CheckInStatus == PassengersCheckInStatus.PARTIALLY_COMPLETE) && ssciBoardingPasses?.BoardingPasses?.Any(bp => bp.CheckedIn && bp.QrCodeData != null) == true)
                {
                    try
                    {
                        var checkedInPasses = response.BoardingPasses.Where(bp => bp.CheckedIn && bp.QrCodeData != null);

                        var airportsTask = _referenceServiceProvider.RetrieveAirportDataAsync();

                        var checkedInOriginAirportCodes = checkedInPasses.Select(bp => bp.OriginAirportCode).ToHashSet();

                        var checkedInSegments = tripsBookingResponse.Product?
                            .FirstOrDefault(x => x.BookingType.Is("AIRLINE") && x.Reference.PnrLocator.Is(pnrLocator))?
                            .Slices
                            .SelectMany(s => s.Segments)
                            .Where(s => checkedInOriginAirportCodes.Contains(s.Origin.Code)) ?? [];

                        var checkedInAirportCodes = checkedInOriginAirportCodes.Union(checkedInPasses.Select(bp => bp.DestinationAirportCode)).ToHashSet();

                        IEnumerable<Models.ReferenceData.AirportDetails> airports;
                        try
                        {
                            airports = await airportsTask ?? [];
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "GetBoardingPass: Failed to retrieve airport data for PNR {pnrLocator}, origin {originCode}", pnrLocator, originCode);

                            airports = [];
                        }

                        var walletLinksResponse = await _walletServiceProvider.GetAirshipWalletLinksAsync((AirshipPassRequest)(checkedInPasses, checkedInPassengers, checkedInSegments, airports.Where(x => checkedInAirportCodes.Contains(x.AirportCode))));
                        var airshipPassResponses = walletLinksResponse?.WalletPasses ?? [];

                        var airshipPassIndex = airshipPassResponses
                            .Where(x => !x.PassengerNIN.IsNullOrWhiteSpace() && !x.OriginAirportCode.IsNullOrWhiteSpace() && x.DepartureDateLocalDateTime.HasValue)
                            .GroupBy(x => (x.PassengerNIN, x.OriginAirportCode, x.DepartureDateLocalDateTime.Value))
                            .ToDictionary(g => g.Key, g => g.First());

                        foreach (var boardingPass in response.BoardingPasses
                            .Where(bp => bp.CheckedIn && bp.QrCodeData != null))
                        {
                            var indexKey = (boardingPass.PassengerNIN, boardingPass.OriginAirportCode, boardingPass.DepartureLocal);

                            if (!airshipPassIndex.TryGetValue(indexKey, out var walletLinks))
                                continue;

                            boardingPass.IosBoardingPassWalletLink = walletLinks.IosBoardingPassWalletLink;
                            boardingPass.AndroidBoardingPassWalletLink = walletLinks.AndroidBoardingPassWalletLink;
                            boardingPass.BoardingPassAdaptiveLink = walletLinks.BoardingPassAdaptiveLink;
                        }

                        response.PnrMultiBoardingPassUrl = walletLinksResponse?.PnrMultiBoardingPassUrl;
                        response.IosPnrMultiBoardingPassUrl = walletLinksResponse?.IosPnrMultiBoardingPassUrl;
                        response.AndroidPnrMultiBoardingPassUrl = walletLinksResponse?.AndroidPnrMultiBoardingPassUrl;
                    }

                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "GetBoardingPass: Failed to retrieve wallet links for checked-in members for PNR {pnrLocator}, origin {originCode}", pnrLocator, originCode);
                    }
                }

                if (response.BoardingPasses.Any(x => x.OperationalStatus != null))
                    response.TripsBookingId = await TripsBookingIdHelper.CreateBookingId(null, tripsBookingResponse.Id, givenNames, surname, pnrLocator);

                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(
                    LoggingCheckinEvent.DAY_OF_TRAVEL,
                    Data: await TelemetryHelper.DayOfTravel(pnrLocator, originCode, givenNames, surname, response, null)));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetBoardingPass: Failed to retrieve boarding passes for PNR {pnrLocator}, origin {originCode}", pnrLocator, originCode);

                context.ReportError(
                    ErrorBuilder.New()
                    .SetMessage(Common.Constants.Application.BoardingPassFailDefaultMessage)
                    .SetExtension("code", "500")
                    .Build());

                var errorMessage = ex.Message;
                try
                {
                    if (ex.InnerException is HttpResponseException httpResponseException)
                    {
                        errorMessage = await httpResponseException.Response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception) { } // Discard. Just log ex.Message.

                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(
                    LoggingCheckinEvent.BOARDING_PASS_FAILED,
                    Data: await TelemetryHelper.DayOfTravel(pnrLocator, originCode, givenNames, surname, response, errorMessage)));

                return null;
            }
        }
    }
}
