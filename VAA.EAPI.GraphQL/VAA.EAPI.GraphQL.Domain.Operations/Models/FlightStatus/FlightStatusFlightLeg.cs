#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusFlightLeg
{
    public string? Id { get; init; }
    public string? MarketingFlightNumber { get; init; }
    public FlightStatusFlightPoint? Departure { get; init; }
    public FlightStatusFlightPoint? Arrival { get; init; }
    public FlightStatusAircraft? Aircraft { get; init; }
    public FlightStatusFlightState? Status { get; init; }
    public FlightStatusFlightDetail? Detail { get; init; }

    public static explicit operator FlightStatusFlightLeg(FlightStatusInternalLeg source)
    {
        var departure = source.Departure is null ? null : (FlightStatusFlightPoint)source.Departure;
        var arrival = source.Arrival is null ? null : (FlightStatusFlightPoint)source.Arrival;

        return new FlightStatusFlightLeg
        {
            Id = source.Id,
            MarketingFlightNumber = source.MarketingFlightNumber,
            Departure = departure,
            Arrival = arrival,
            Aircraft = source.Aircraft is null ? null : (FlightStatusAircraft)source.Aircraft,
            Status = FlightStatusFlightState.FromInternal(source.Status, departure, arrival),
            Detail = source.Detail is null
                ? null
                : FlightStatusFlightDetail.FromInternal(source.Detail, departure, arrival)
        };
    }
}
