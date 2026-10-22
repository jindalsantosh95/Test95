#nullable enable
using VAA.EAPI.GraphQL.Common.Extensions;
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusFlight
{
    public string? FlightDate { get; init; }
    public string? FlightNumber { get; init; }
    public string? SelectedLegId { get; init; }
    public IEnumerable<FlightStatusFlightLeg?>? Legs { get; init; }
    public FlightStatusFlightLeg? ActiveLeg { get; init; }
    public IEnumerable<FlightStatusWarning?>? Warnings { get; init; }

    public static explicit operator FlightStatusFlight(FlightStatusInternalNumberResponse source) => new()
    {
        FlightDate = source.FlightDate,
        FlightNumber = source.FlightNumber,
        SelectedLegId = source.SelectedLegId,
        Legs = source.Legs?
            .Where(leg => leg is not null)
            .Select(leg => (FlightStatusFlightLeg)leg!)
            .ToArray(),
        ActiveLeg = source.ActiveLeg is null ? null : (FlightStatusFlightLeg)source.ActiveLeg,
        Warnings = source.Warnings?
            .Where(warning => warning is not null)
            .Select(warning => (FlightStatusWarning)warning!)
            .ToArray()
    };

    internal static FlightStatusFlight FromInternal(
        FlightStatusInternalNumberResponse source,
        DateOnly requestedDepartureDate)
    {
        var flight = (FlightStatusFlight)source;
        if (!flight.FlightDate.IsNullOrWhiteSpace())
            return flight;

        return new FlightStatusFlight
        {
            FlightDate = requestedDepartureDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            FlightNumber = flight.FlightNumber,
            SelectedLegId = flight.SelectedLegId,
            Legs = flight.Legs,
            ActiveLeg = flight.ActiveLeg,
            Warnings = flight.Warnings
        };
    }
}
