using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class SeatCarrierEligibilityTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(3, "down")]
    [InlineData(4, "down")]
    [InlineData(10, "up")]
    public void FlightDetailsNavigation_RecoversOvershotCandidate(int visibleIndex, string expected)
    {
        string source = $"<App><Element name='NativeFlightResult{visibleIndex}CarrierAF' visible='true'/><Element name='NativeFlightResult0' visible='false'/></App>";
        Assert.Equal(expected, PageObjects.NativeFlightSearchPage.FlightDetailsScrollDirection(source, 4));
        Assert.Throws<InvalidOperationException>(() =>
            PageObjects.NativeFlightSearchPage.FlightDetailsScrollDirection("<App/>", 4));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Air France", "Operated by VS", true)]
    [InlineData("Air France", "Delta Air Lines", true)]
    [InlineData("Virgin Atlantic", "KLM", true)]
    [InlineData("DL", "Virgin Atlantic", true)]
    [InlineData("Air France", "KLM", false)]
    [InlineData("Air France", "Unknown", false)]
    public void ProximityInbound_RequiresAtLeastOneExplicitSupportedLeg(string firstCarrier, string secondCarrier, bool expected)
    {
        FlightLegDetails first = new("YYZ", "CDG", new DateOnly(2027, 1, 20),
            new DateOnly(2027, 1, 21), new TimeOnly(18, 0), new TimeOnly(7, 0), firstCarrier, null);
        FlightLegDetails second = first with { Origin = "CDG", Destination = "LHR", OperatingCarrier = secondCarrier };
        Assert.Equal(expected, SeatCarrierEligibilityAssertions.HasSeatSupportedLeg(new([first, second], ["CDG"])));
        Assert.False(SeatCarrierEligibilityAssertions.HasSeatSupportedLeg(new([], [])));
        Assert.True(SeatCarrierEligibilityAssertions.HasSeatSupportedLeg(new([first with { OperatingCarrier = "VS" }], [])));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Air France", true)]
    [InlineData("Delta Air Lines", false)]
    [InlineData("Virgin Atlantic", false)]
    [InlineData("", false)]
    public void UnsupportedItinerary_ChecksEveryLeg(string secondCarrier, bool expected)
    {
        FlightLegDetails first = new("LHR", "AMS", new DateOnly(2027, 1, 12),
            new DateOnly(2027, 1, 12), new TimeOnly(8, 0), new TimeOnly(10, 0), "KLM", null);
        ConnectingFlightDetails details = new([first, first with { OperatingCarrier = secondCarrier }], ["AMS"]);
        Assert.Equal(expected, SeatCarrierEligibilityAssertions.AllLegsAreKnownUnsupported(details));
        Assert.False(SeatCarrierEligibilityAssertions.AllLegsAreKnownUnsupported(new([], [])));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Operated by KLM", true)]
    [InlineData("Air France", true)]
    [InlineData("KL", true)]
    [InlineData("AF", true)]
    [InlineData("Delta Air Lines", false)]
    [InlineData("VS", false)]
    [InlineData("DL", false)]
    [InlineData("Virgin Atlantic", false)]
    [InlineData("KLM and Delta Air Lines", false)]
    [InlineData("Unknown", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void UnsupportedCarrier_RequiresExplicitKnownIdentity(string? carrier, bool expected)
        => Assert.Equal(expected, SeatCarrierEligibilityAssertions.IsKnownUnsupported(carrier));
}
