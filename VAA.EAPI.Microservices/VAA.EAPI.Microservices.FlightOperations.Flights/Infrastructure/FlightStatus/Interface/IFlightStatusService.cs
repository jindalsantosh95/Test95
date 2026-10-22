using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Interface
{
    public interface IFlightStatusService
    {
        Task<Result<Response<FlightStatusRouteResponse>>> RouteSearchAsync(
            string departureAirportCode,
            string arrivalAirportCode,
            DateOnly scheduledDepartureDate,
            string? timeRange,
            string? stops,
            bool first,
            CancellationToken cancellationToken = default);

        Task<Result<Response<FlightStatusNumberResponse>>> NumberSearchAsync(
            string? selectedLegId,
            string flightNumber,
            DateOnly departureDate,
            string? departureAirportCode,
            CancellationToken cancellationToken = default);
    }
}
