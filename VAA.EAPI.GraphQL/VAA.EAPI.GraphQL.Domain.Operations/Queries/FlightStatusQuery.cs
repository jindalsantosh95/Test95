#nullable enable
using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Operations.Interfaces;
using VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;
using VAA.EAPI.GraphQL.Domain.Operations.Services;

namespace VAA.EAPI.GraphQL.Domain.Operations.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public sealed class FlightStatusQuery
    {
        private readonly IFlightStatusProvider _flightStatusProvider;
        private readonly TimeProvider _timeProvider;

        public FlightStatusQuery(IFlightStatusProvider flightStatusProvider, TimeProvider? timeProvider = null)
        {
            _flightStatusProvider = flightStatusProvider ?? throw new ArgumentNullException(nameof(flightStatusProvider));
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        [GraphQLName("flightStatusByRoute")]
        [GraphQLDescription("Searches flight status by route and scheduled departure date.")]
        public async Task<FlightStatusRoute?> FlightStatusByRoute(
            [GraphQLNonNullType] string departureAirportCode,
            [GraphQLNonNullType] string arrivalAirportCode,
            [GraphQLNonNullType] DateOnly scheduledDepartureDate,
            IEnumerable<int>? timeRange,
            IEnumerable<int>? stops,
            bool? first,
            CancellationToken cancellationToken)
        {
            try
            {
                var request = new FlightStatusRouteRequest
                {
                    DepartureAirportCode = departureAirportCode,
                    ArrivalAirportCode = arrivalAirportCode,
                    ScheduledDepartureDate = scheduledDepartureDate,
                    TimeRange = timeRange,
                    Stops = stops,
                    First = first
                };
                FlightStatusInputValidator.Validate(request, _timeProvider);
                return await _flightStatusProvider.SearchByRouteAsync(request, cancellationToken);
            }
            catch (ArgumentException exception)
            {
                throw CreateError(
                    exception.Message,
                    "FLIGHT_STATUS_INVALID_INPUT");
            }
            catch (FlightStatusProviderException exception)
            {
                throw CreateDependencyError(exception);
            }
        }

        [GraphQLName("flightStatusByNumber")]
        [GraphQLDescription("Looks up flight status by flight number and departure date.")]
        public async Task<FlightStatusFlight?> FlightStatusByNumber(
            [GraphQLNonNullType] string flightNumber,
            [GraphQLNonNullType] DateOnly departureDate,
            [GraphQLNonNullType] string departureAirportCode,
            string? selectedLegId,
            CancellationToken cancellationToken)
        {
            try
            {
                var request = new FlightStatusNumberRequest
                {
                    FlightNumber = flightNumber?.Trim() ?? string.Empty,
                    DepartureDate = departureDate,
                    DepartureAirportCode = departureAirportCode?.Trim().ToUpperInvariant() ?? string.Empty,
                    SelectedLegId = selectedLegId?.Trim()
                };
                FlightStatusInputValidator.Validate(request, _timeProvider);
                return await _flightStatusProvider.SearchByNumberAsync(request, cancellationToken);
            }
            catch (ArgumentException exception)
            {
                throw CreateError(
                    exception.Message,
                    "FLIGHT_STATUS_INVALID_INPUT");
            }
            catch (FlightStatusProviderException exception)
            {
                throw CreateDependencyError(exception);
            }
        }

        private static GraphQLException CreateDependencyError(FlightStatusProviderException exception) =>
            exception.Failure switch
            {
                FlightStatusProviderFailure.NotFound => CreateError("No results.", "FLIGHT_STATUS_NOT_FOUND"),
                FlightStatusProviderFailure.InvalidInput => CreateError("Flight-status search input is invalid.", "FLIGHT_STATUS_INVALID_INPUT"),
                FlightStatusProviderFailure.Timeout => CreateError("Flight-status service did not respond in time.", "FLIGHT_STATUS_TIMEOUT"),
                _ => CreateError("Flight-status service is temporarily unavailable.", "FLIGHT_STATUS_UNAVAILABLE")
            };

        private static GraphQLException CreateError(string message, string code) =>
            new(ErrorBuilder.New()
                .SetMessage(message)
                .SetExtension("code", code)
                .Build());
    }
}
