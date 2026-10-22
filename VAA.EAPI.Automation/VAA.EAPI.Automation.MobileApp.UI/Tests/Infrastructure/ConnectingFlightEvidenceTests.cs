using System.Xml.Linq;
using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class ConnectingFlightEvidenceTests
{
    private static readonly FlightSearch Search = new("LHR", "JFK", new DateOnly(2027, 1, 12), PassengerCounts.OneAdult);

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void ConnectingDetails_ReadsScopedValuesAndRetainsOrderedLegs()
    {
        ConnectingFlightDetails details = ReadFixture();
        ConnectingFlightAssertions.MatchesSearch(Search, details);
        ConnectingFlightAssertions.Retained(details, ReadFixture());
        Assert.Equal("AMS", details.Legs[0].Destination);
        Assert.Equal(new TimeOnly(9, 0), details.Legs[0].ArrivalTime);
        Assert.Equal("Operated by Delta Air Lines", details.Legs[1].OperatingCarrier);
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("OriginAirportCode")]
    [InlineData("DepartureDate")]
    [InlineData("OperatingCarrier")]
    public void ConnectingDetails_RejectsMissingRequiredField(string field)
    {
        Dictionary<string, string> values = FixtureValues();
        values.Remove(NativeSearchAccessibilityIds.FlightLegField(1, field));
        Assert.Throws<InvalidOperationException>(() => FlightDetailsEvidence.Read(values, 1));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void ConnectingDetails_RejectsConflictingIdentifierValues()
    {
        string id = NativeSearchAccessibilityIds.FlightLegField(1, "OriginAirportCode");
        string source = $"<App><Text name='{id}' value='LHR'/><Text name='{id}' value='MAN'/></App>";
        Assert.Throws<InvalidOperationException>(() => FlightDetailsEvidence.Capture(source, 1, new Dictionary<string, string>()));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void ConnectingDetails_DoesNotUseAirportLabelAsCode()
    {
        Dictionary<string, string> values = new();
        string id = NativeSearchAccessibilityIds.FlightLegField(1, "OriginAirportCode");
        FlightDetailsEvidence.Capture($"<App><Text name='{id}' label='Heathrow Airport'/></App>", 1, values);
        Assert.False(values.ContainsKey(id));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("connection")]
    [InlineData("next-origin")]
    [InlineData("date")]
    [InlineData("single-carrier")]
    public void ConnectingDetails_RejectsBrokenItinerary(string fault)
    {
        ConnectingFlightDetails details = ReadFixture();
        FlightLegDetails[] legs = details.Legs.ToArray();
        if (fault == "next-origin")
            legs[1] = legs[1] with { Origin = "CDG" };
        if (fault == "date")
            legs[0] = legs[0] with { DepartureDate = Search.DepartureDate.AddDays(1) };
        if (fault == "single-carrier")
            legs[1] = legs[1] with { OperatingCarrier = legs[0].OperatingCarrier };
        ConnectingFlightDetails changed = new(legs, fault == "connection" ? ["CDG"] : details.ConnectionAirports);
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ConnectingFlightAssertions.MatchesSearch(Search, changed));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("order")]
    [InlineData("time")]
    [InlineData("carrier")]
    [InlineData("descriptor")]
    public void ConnectingDetails_RejectsSummaryMismatch(string fault)
    {
        ConnectingFlightDetails result = ReadFixture();
        FlightLegDetails[] legs = result.Legs.ToArray();
        if (fault == "order")
            Array.Reverse(legs);
        if (fault == "time")
            legs[1] = legs[1] with { ArrivalTime = new TimeOnly(15, 0) };
        if (fault == "carrier")
            legs[1] = legs[1] with { OperatingCarrier = "Operated by KLM" };
        if (fault == "descriptor")
            legs[1] = legs[1] with { Descriptor = "DL999" };
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ConnectingFlightAssertions.Retained(result, new(legs, result.ConnectionAirports)));
    }

    private static ConnectingFlightDetails ReadFixture() => FlightDetailsEvidence.Read(FixtureValues(), 1);

    private static Dictionary<string, string> FixtureValues()
    {
        XElement source = new("App");
        for (int index = 1; index <= 2; index++)
        {
            Dictionary<string, string> fields = new()
            {
                ["OriginAirportCode"] = index == 1 ? "LHR" : "AMS",
                ["DestinationAirportCode"] = index == 1 ? "AMS" : "JFK",
                ["DepartureDate"] = "2027-01-12",
                ["ArrivalDate"] = "2027-01-12",
                ["DepartureTime"] = index == 1 ? "Departure time is 06:30" : "Departure time is 11:35",
                ["ArrivalTime"] = index == 1 ? "Arrival time is 09:00" : "Arrival time is 14:00",
                ["OperatingCarrier"] = index == 1 ? "Operated by KLM" : "Operated by Delta Air Lines",
                ["Descriptor"] = index == 1 ? "VS7046" : "VS3951",
            };
            foreach ((string field, string value) in fields)
                source.Add(new XElement("Text", new XAttribute("name", NativeSearchAccessibilityIds.FlightLegField(index, field)),
                    new XAttribute("value", value), new XAttribute("label", value)));
        }
        source.Add(new XElement("Text", new XAttribute("name", NativeSearchAccessibilityIds.FlightConnection(1)),
            new XAttribute("value", "AMS"), new XAttribute("label", "Change in Amsterdam")));
        Dictionary<string, string> observed = new();
        FlightDetailsEvidence.Capture(source.ToString(), 1, observed);
        return observed;
    }
}
