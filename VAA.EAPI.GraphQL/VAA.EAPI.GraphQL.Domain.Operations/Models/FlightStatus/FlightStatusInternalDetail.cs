#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusInternalDetail
{
    public FlightStatusInternalAircraft? Aircraft { get; init; }
    public FlightStatusInternalState? Status { get; init; }
    public FlightStatusInternalPerformance? Performance { get; init; }
    public FlightStatusInternalPosition? Position { get; init; }
    public IEnumerable<FlightStatusInternalAmenity?>? Amenities { get; init; }
    public IEnumerable<FlightStatusInternalMeal?>? Meals { get; init; }
}
