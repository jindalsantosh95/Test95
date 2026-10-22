using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Flows;

public static class RecentAirportSearchFlow
{
    public static void VerifyLatestFivePerDirection(IFlightSearchPanelPage page)
    {
        (string Origin, string Destination)[] searches =
        [
            ("LHR", "JFK"), ("MAN", "MCO"), ("EDI", "LAX"),
            ("GLA", "ATL"), ("NCL", "BOS"), ("BRS", "SFO"),
        ];
        foreach ((string origin, string destination) in searches)
        {
            page.SelectAirport(true, origin);
            page.SelectAirport(false, destination);
        }
        string[] expectedOrigins = searches.TakeLast(5).Reverse().Select(search => search.Origin).ToArray();
        string[] expectedDestinations = searches.TakeLast(5).Reverse().Select(search => search.Destination).ToArray();
        Assert.Equal(expectedOrigins, page.ReadRecentAirports(true));
        page.SelectRecentAirport(true, expectedOrigins[0]);
        Assert.Equal(expectedDestinations, page.ReadRecentAirports(false));
        page.SelectRecentAirport(false, expectedDestinations[0]);

        // Selecting a different recent origin must promote it without contaminating destination history.
        page.SelectAirport(true, expectedOrigins[2]);
        string[] reordered = new[] { expectedOrigins[2] }.Concat(expectedOrigins.Where(code => code != expectedOrigins[2])).ToArray();
        Assert.Equal(reordered, page.ReadRecentAirports(true));
        page.SelectRecentAirport(true, reordered[0]);
        Assert.Equal(expectedDestinations, page.ReadRecentAirports(false));
        page.SelectRecentAirport(false, expectedDestinations[0]);
    }
}
