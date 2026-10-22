#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusLeg
{
    public string? Id { get; init; }
    public string? FlightNumber { get; init; }
    public FlightStatusAirport? DepartureAirport { get; init; }
    public FlightStatusAirport? ArrivalAirport { get; init; }
    public FlightStatusAirline? MarketingAirline { get; init; }
    public FlightStatusAirline? OperatingAirline { get; init; }
    public FlightStatusAircraft? Aircraft { get; init; }
    public string? FlightTypeCode { get; init; }
    public bool? ReturnedIndicator { get; init; }
    public string? ShipNumber { get; init; }
    public string? TakeOffNumber { get; init; }
    public bool? Usable { get; init; }
    public FlightStatusLegStatus? Status { get; init; }
    public FlightStatusPosition? FlightPosition { get; init; }
}
