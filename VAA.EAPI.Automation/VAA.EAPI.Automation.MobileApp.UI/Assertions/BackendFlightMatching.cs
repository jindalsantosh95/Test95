using System.Text.RegularExpressions;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Text;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Assertions;

internal static class BackendFlightMatching
{
    internal static void AssertCurrency(string backendCurrency, string uiPrice)
    {
        Assert.Equal("GBP", backendCurrency);
        Assert.Equal("£", SummaryChangeAssertions.Money(uiPrice).Currency);
    }

    internal static bool Matches(BackendFareExpectation expected, ConnectingFlightDetails observed)
    {
        if (expected.Legs.Count == 0 || expected.FlightNumbers.Count != expected.Legs.Count ||
            observed.Legs.Count != expected.Legs.Count || observed.ConnectionAirports.Count != expected.Legs.Count - 1)
            return false;
        for (int index = 0; index < expected.Legs.Count; index++)
        {
            FlightLegDetails actual = observed.Legs[index], target = expected.Legs[index];
            if (actual.Origin != target.Origin || actual.Destination != target.Destination ||
                actual.DepartureDate != target.DepartureDate || actual.ArrivalDate != target.ArrivalDate ||
                actual.DepartureTime != target.DepartureTime || actual.ArrivalTime != target.ArrivalTime ||
                !actual.OperatingCarrier.Replace("Operated by ", "", StringComparison.OrdinalIgnoreCase).Trim().Is(target.OperatingCarrier) ||
                !RegexCompiled.IsMatch(actual.Descriptor ?? string.Empty, $@"\b{Regex.Escape(expected.FlightNumbers[index])}\b", RegexOptions.IgnoreCase))
                return false;
            if (index < observed.ConnectionAirports.Count &&
                (observed.ConnectionAirports[index] != target.Destination || target.Destination != expected.Legs[index + 1].Origin))
                return false;
        }
        return true;
    }
}
