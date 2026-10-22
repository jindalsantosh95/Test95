using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Assertions;

public static class ConnectingFlightAssertions
{
    public static void MatchesSearch(FlightSearch search, ConnectingFlightDetails details)
    {
        Assert.True(details.Legs.Count >= 2, "A connecting itinerary must expose at least two legs.");
        Assert.Equal(details.Legs.Count - 1, details.ConnectionAirports.Count);
        Assert.Equal(search.Origin, details.Legs[0].Origin);
        Assert.Equal(search.Destination, details.Legs[^1].Destination);
        Assert.Equal(search.DepartureDate, details.Legs[0].DepartureDate);
        Assert.True(details.Legs.Select(leg => leg.OperatingCarrier).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1,
            "The +1 itinerary must expose more than one operating carrier in Flight details.");
        for (int index = 0; index < details.ConnectionAirports.Count; index++)
        {
            Assert.Equal(details.Legs[index].Destination, details.Legs[index + 1].Origin);
            Assert.Equal(details.Legs[index].Destination, details.ConnectionAirports[index]);
        }
        // These are airport-local clocks; do not infer UTC chronology or duration from them.
    }

    public static void Retained(ConnectingFlightDetails result, ConnectingFlightDetails summary)
    {
        Assert.Equal(result.Legs.ToArray(), summary.Legs.ToArray());
        Assert.Equal(result.ConnectionAirports.ToArray(), summary.ConnectionAirports.ToArray());
    }
}
