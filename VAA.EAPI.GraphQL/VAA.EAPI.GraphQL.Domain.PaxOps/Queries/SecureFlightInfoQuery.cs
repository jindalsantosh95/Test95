using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SecureFlightInfo;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;
using TripMS = VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public sealed class SecureFlightInfoQuery(
        ITripsDetailsQueryProvider tripsDetailsQueryProvider,
        ITelemetryService telemetryService,
        ILogger<SecureFlightInfoQuery> logger)
    {
        private readonly ITripsDetailsQueryProvider _tripsDetailsQueryProvider = tripsDetailsQueryProvider ?? throw new ArgumentNullException(nameof(tripsDetailsQueryProvider));
        private readonly ITelemetryService _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        private readonly ILogger<SecureFlightInfoQuery> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [GraphQLName("GetSecureFlightInfo")]
        [GraphQLDescription("Retrieves secure flight information for one passenger in a booking.")]
        public async Task<SecureFlightInfoResponse?> GetSecureFlightInfo(SecureFlightInfoRequest request, IResolverContext context)
        {
            if (request is null)
            {
                context.ReportError("Missing mandatory field(s): request");
                return null;
            }

            var missingFields = new[]
            {
                (nameof(request.PnrLocator), request.PnrLocator),
                (nameof(request.GivenNames), request.GivenNames),
                (nameof(request.Surname), request.Surname),
                (nameof(request.PassengerFirstNin), request.PassengerFirstNin),
                (nameof(request.PassengerLastNin), request.PassengerLastNin)
            }.Where(field => field.Item2.IsNullOrWhiteSpace()).Select(field => field.Item1).ToArray();

            if (missingFields.Any())
            {
                context.ReportError($"Missing mandatory field(s): {string.Join(", ", missingFields)}");
                return null;
            }

            _ = _telemetryService.TrackTelemetry((TelemetryEvent)(
                LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_REQUESTED,
                Data: new Dictionary<string, object>
                {
                    { "PnrLocator", request.PnrLocator },
                    { "PassengerFirstNin", request.PassengerFirstNin },
                    { "PassengerLastNin", request.PassengerLastNin }
                }));

            try
            {
                var booking = await _tripsDetailsQueryProvider.GetTripDetails(new TripMS.TripsDetailsRequest
                {
                    Using = TripMS.RequestOptionCodeType.PNR,
                    ConfirmationNum = request.PnrLocator,
                    GivenNames = request.GivenNames,
                    Surname = request.Surname
                });

                if (booking?.Id.IsNullOrWhiteSpace() == true)
                    throw new InvalidOperationException("Trip booking was not found.");

                var passengerDetails = await _tripsDetailsQueryProvider.GetPassengerDetails(booking.Id);

                var passengers = passengerDetails?.PassengerInformation?.Passengers
                    ?? throw new InvalidOperationException("Passenger details were not available.");

                var matchingPassenger = passengers.SingleOrDefault(passenger =>
                    passenger.LastNameNum == request.PassengerLastNin
                    && passenger.FirstNameNum == request.PassengerFirstNin)
                    ?? throw new InvalidOperationException("Travel Requirements passenger is not in the booking.");

                var response = (SecureFlightInfoResponse)(booking, matchingPassenger);

                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(
                    LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_RETRIEVED,
                    Data: TelemetryHelper.SecureFlightInfoDetails(response, request.PnrLocator)));

                return response;
            }
            catch (Exception exception)
            {
                return await SecureFlightInfoFailureHandler.HandleAsync<SecureFlightInfoResponse>(
                    context,
                    exception,
                    new SecureFlightInfoFailureContext
                    {
                        PnrLocator = request.PnrLocator,
                        LoggingFailureContext = LoggingFailureContext.GET_SECURE_FLIGHT_INFO,
                        Operation = "GetSecureFlightInfo",
                        DefaultCode = "500"
                    },
                    _telemetryService,
                    _logger);
            }
        }
    }
}
