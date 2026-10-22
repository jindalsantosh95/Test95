using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.SecureFlightInfo;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SecureFlightInfo;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public sealed class SecureFlightInfoMutation(
        ITripsDetailsQueryProvider tripsDetailsQueryProvider,
        ITelemetryService telemetryService,
        ILogger<SecureFlightInfoMutation> logger)
    {
        private readonly ITripsDetailsQueryProvider _tripsDetailsQueryProvider = tripsDetailsQueryProvider ?? throw new ArgumentNullException(nameof(tripsDetailsQueryProvider));
        private readonly ITelemetryService _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        private readonly ILogger<SecureFlightInfoMutation> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [GraphQLName("UpdateSecureFlightInfo")]
        [GraphQLDescription("Updates secure flight information for one passenger in a booking.")]
        public async Task<SecureFlightInfoResponse?> UpdateSecureFlightInfo(UpdateSecureFlightInfoRequest request, IResolverContext context)
        {
            if (request is null)
            {
                context.ReportError("Missing mandatory field(s): request");
                return null;
            }

            var missingFields = new[]
            {
                (nameof(request.SecureFlightReservationId), request.SecureFlightReservationId),
                (nameof(request.PassengerId), request.PassengerId)
            }.Where(field => field.Item2.IsNullOrWhiteSpace()).Select(field => field.Item1).ToArray();

            if (missingFields.Any())
            {
                context.ReportError($"Missing mandatory field(s): {string.Join(", ", missingFields)}");
                return null;
            }

            var secureFlightDetails = request.SecureFlightDetails?.ToList() ?? [];
            if (!secureFlightDetails.Any(detail => detail is not null && !detail.Number.IsNullOrWhiteSpace()))
            {
                context.ReportError("At least one secure flight detail must be provided.");
                return null;
            }

            var hasCanadianTravelNumber = secureFlightDetails.Any(detail =>
                detail?.Type == SecureFlightDetailType.CanadianTravelNumber &&
                !detail.Number.IsNullOrWhiteSpace());
            var hasRedressNumber = secureFlightDetails.Any(detail =>
                detail?.Type == SecureFlightDetailType.RedressNumber &&
                !detail.Number.IsNullOrWhiteSpace());

            if (hasCanadianTravelNumber && hasRedressNumber)
            {
                context.ReportError("Canadian Travel Number and Redress Number cannot be provided together.");
                return null;
            }

            _ = _telemetryService.TrackTelemetry((TelemetryEvent)(
                LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_UPDATE_INITIATED,
                Data: new Dictionary<string, object>
                {
                    { "SecureFlightReservationId", request.SecureFlightReservationId },
                    { "PassengerFirstNin", request.PassengerFirstNin },
                    { "PassengerLastNin", request.PassengerLastNin }
                }));

            try
            {
                var passengerDetails = await _tripsDetailsQueryProvider.UpdatePassengerDetails(new UpdatePassengerDetailsRequest
                {
                    BookingId = request.SecureFlightReservationId,
                    PassengerResourceId = request.PassengerId,
                    TrustedTravellerPrograms = secureFlightDetails
                        .Where(detail => detail is not null && !detail.Number.IsNullOrWhiteSpace())
                        .Select(detail => new TrustedTravellerProgramResponse
                        {
                            Program = GetProgramCode(detail.Type),
                            Number = detail.Number
                        })
                });

                var passengers = passengerDetails?.PassengerInformation?.Passengers
                    ?? throw new InvalidOperationException("Passenger details were not available.");

                var passenger = passengers.SingleOrDefault(item => item.PassengerResourceId == request.PassengerId)
                    ?? throw new InvalidOperationException("Passenger is not in the booking.");

                var response = (SecureFlightInfoResponse)(request.SecureFlightReservationId, passenger);
                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(
                    LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_UPDATED,
                    Data: TelemetryHelper.SecureFlightInfoDetails(response)));

                return response;
            }
            catch (Exception exception)
            {
                return await SecureFlightInfoFailureHandler.HandleAsync<SecureFlightInfoResponse>(
                    context,
                    exception,
                    new SecureFlightInfoFailureContext
                    {
                        SecureFlightReservationId = request.SecureFlightReservationId,
                        LoggingFailureContext = LoggingFailureContext.UPDATE_SECURE_FLIGHT_INFO,
                        Operation = "UpdateSecureFlightInfo",
                        DefaultCode = "500"
                    },
                    _telemetryService,
                    _logger);
            }
        }

        private static string GetProgramCode(SecureFlightDetailType type) => type switch
        {
            SecureFlightDetailType.KnownTravellerNumber => "KNOWN_TRAVELER_NUMBER",
            SecureFlightDetailType.CanadianTravelNumber => "CANADA_TRAVELER_NUMBER",
            SecureFlightDetailType.RedressNumber => "REDRESS_NUMBER",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported secure flight detail type.")
        };

    }
}
