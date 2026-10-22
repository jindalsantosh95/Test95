#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusSegment
{
    public string? EnrouteStops { get; init; }
    public bool? ChangeOfGauge { get; init; }
    public bool? CodeShare { get; init; }
    public bool? ConnectionCarrier { get; init; }
    public bool? ConnectionCarrierOrCodeShareOperator { get; init; }
    public string? ConnectionCarrierOrCodeShareOperatorName { get; init; }
    public IEnumerable<FlightStatusLeg?>? Legs { get; init; }
    public IEnumerable<FlightStatusLeg?>? LegsWithChangeOfGauge { get; init; }
}
