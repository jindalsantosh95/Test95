namespace VAA.EAPI.Automation.MobileApp.UI.Models;

public sealed record BackendFareExpectation(
    IReadOnlyList<FlightLegDetails> Legs, IReadOnlyList<string> FlightNumbers, string FareTitle, string Currency,
    IReadOnlyList<BackendBaggageAllowance> Baggage)
{
    public FlightLegDetails Flight => Legs[0];
}
