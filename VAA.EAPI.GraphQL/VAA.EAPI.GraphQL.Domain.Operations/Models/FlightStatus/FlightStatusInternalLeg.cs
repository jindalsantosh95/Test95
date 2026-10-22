#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusInternalLeg
{
    public string? Id { get; init; }
    public string? MarketingFlightNumber { get; init; }
    public FlightStatusInternalPoint? Departure { get; init; }
    public FlightStatusInternalPoint? Arrival { get; init; }
    public FlightStatusInternalAircraft? Aircraft { get; init; }
    public FlightStatusInternalState? Status { get; init; }
    public FlightStatusInternalDetail? Detail { get; init; }
}
