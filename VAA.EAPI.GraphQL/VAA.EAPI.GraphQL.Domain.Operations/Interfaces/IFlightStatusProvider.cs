#nullable enable
using VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

namespace VAA.EAPI.GraphQL.Domain.Operations.Interfaces
{
    public interface IFlightStatusProvider
    {
        Task<FlightStatusRoute?> SearchByRouteAsync(FlightStatusRouteRequest request, CancellationToken cancellationToken);

        Task<FlightStatusFlight?> SearchByNumberAsync(FlightStatusNumberRequest request, CancellationToken cancellationToken);
    }
}
