namespace VAA.EAPI.Automation.MobileApp.UI.Models;

public sealed record ConnectingFlightDetails(
    IReadOnlyList<FlightLegDetails> Legs, IReadOnlyList<string> ConnectionAirports);
