#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusFlightDetail
{
    public FlightStatusAircraft? Aircraft { get; init; }
    public FlightStatusFlightState? Status { get; init; }
    public FlightStatusPerformance? Performance { get; init; }
    public FlightStatusPosition? Position { get; init; }
    public IEnumerable<FlightStatusAmenity?>? Amenities { get; init; }
    public IEnumerable<FlightStatusMeal?>? Meals { get; init; }

    public static explicit operator FlightStatusFlightDetail(FlightStatusInternalDetail source) =>
        FromInternal(source, null, null);

    internal static FlightStatusFlightDetail FromInternal(
        FlightStatusInternalDetail source,
        FlightStatusFlightPoint? departure,
        FlightStatusFlightPoint? arrival) => new()
        {
            Aircraft = source.Aircraft is null ? null : (FlightStatusAircraft)source.Aircraft,
            Status = FlightStatusFlightState.FromInternal(source.Status, departure, arrival),
            Performance = source.Performance is null ? null : (FlightStatusPerformance)source.Performance,
            Position = source.Position is null ? null : (FlightStatusPosition)source.Position,
            Amenities = source.Amenities?
                .Where(amenity => amenity is not null)
                .Select(amenity => (FlightStatusAmenity)amenity!)
                .ToArray(),
            Meals = source.Meals?
                .Where(meal => meal is not null)
                .Select(meal => (FlightStatusMeal)meal!)
                .ToArray()
        };
}
