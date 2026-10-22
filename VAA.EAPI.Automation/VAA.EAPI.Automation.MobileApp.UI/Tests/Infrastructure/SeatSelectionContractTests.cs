using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class SeatSelectionContractTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("10", "110", "40", "40", true)]
    [InlineData("10", "480", "40", "40", false)]
    [InlineData("10", "110", "0", "40", false)]
    [InlineData("invalid", "110", "40", "40", false)]
    public void SnapshotSeatFits_UsesGeometryWithoutSnapshotVisibility(
        string x, string y, string width, string height, bool expected)
    {
        System.Xml.Linq.XElement node = new("XCUIElementTypeButton",
            new System.Xml.Linq.XAttribute("x", x), new System.Xml.Linq.XAttribute("y", y),
            new System.Xml.Linq.XAttribute("width", width), new System.Xml.Linq.XAttribute("height", height));
        Assert.Equal(expected, NativeSeatSelectionPage.SnapshotSeatFits(node, new(0, 100, 400, 400)));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void SafeSeatArea_ClipsOversizedViewportToVisibleAnchors()
    {
        Assert.Equal(new System.Drawing.Rectangle(0, 101, 400, 398),
            NativeSeatSelectionPage.SafeSeatArea(new(0, 0, 400, 900), new(400, 900), 100, 500));
        Assert.Equal(new System.Drawing.Rectangle(20, 150, 350, 300),
            NativeSeatSelectionPage.SafeSeatArea(new(20, 150, 350, 300), new(400, 900), 100, 500));
        Assert.Throws<InvalidOperationException>(() =>
            NativeSeatSelectionPage.SafeSeatArea(new(0, 0, 400, 900), new(400, 900), 500, 100));
        Assert.Throws<InvalidOperationException>(() =>
            NativeSeatSelectionPage.SafeSeatArea(new(0, 600, 400, 200), new(400, 900), 100, 500));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void SeatBounds_RejectOccludedPartialAndInvalidTargets()
    {
        Assert.True(NativeSeatSelectionPage.SeatBoundsAreSafe(new(10, 110, 40, 40), 400, 100, 500));
        Assert.False(NativeSeatSelectionPage.SeatBoundsAreSafe(new(10, 480, 40, 40), 400, 100, 500));
        Assert.False(NativeSeatSelectionPage.SeatBoundsAreSafe(new(10, 500, 40, 40), 400, 100, 500));
        Assert.False(NativeSeatSelectionPage.SeatBoundsAreSafe(new(10, 90, 40, 40), 400, 100, 500));
        Assert.False(NativeSeatSelectionPage.SeatBoundsAreSafe(new(-1, 110, 40, 40), 400, 100, 500));
        Assert.False(NativeSeatSelectionPage.SeatBoundsAreSafe(new(380, 110, 40, 40), 400, 100, 500));
        Assert.False(NativeSeatSelectionPage.SeatBoundsAreSafe(new(10, 110, 0, 40), 400, 100, 500));
        Assert.False(NativeSeatSelectionPage.SeatBoundsAreSafe(new(10, 110, 40, 40), 400, 500, 100));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void SeatContract_MatchesOwnedActionIdentifiers()
    {
        Newtonsoft.Json.Linq.JObject contract = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Contracts", "search-book-automation-contract.json")));
        Newtonsoft.Json.Linq.JToken seats = contract["seatSelectionContract"]
            ?? throw new InvalidOperationException("Seat contract missing.");
        Dictionary<string, string> expected = new()
        {
            ["heading"] = NativeBookingAccessibilityIds.SeatSelectionTitle,
            ["seatPrefix"] = NativeBookingAccessibilityIds.SeatPrefix,
            ["passenger"] = NativeBookingAccessibilityIds.SeatPassengerOption(0),
            ["secondPassenger"] = NativeBookingAccessibilityIds.SeatPassengerOption(1),
            ["seatPrice"] = NativeBookingAccessibilityIds.SeatSelectionPrice,
            ["viewport"] = NativeBookingAccessibilityIds.SeatSelectionViewport,
            ["tripTotal"] = NativeBookingAccessibilityIds.SeatSelectionTripTotal,
            ["detailPrice"] = NativeBookingAccessibilityIds.SeatSelectionDetailPrice,
            ["confirm"] = NativeBookingAccessibilityIds.SeatSelectionConfirm,
            ["detailClose"] = NativeBookingAccessibilityIds.SeatSelectionDetailClose,
            ["legend"] = NativeBookingAccessibilityIds.SeatSelectionLegend,
            ["legendClose"] = NativeBookingAccessibilityIds.SeatSelectionLegendClose,
            ["childGuidanceSheet"] = NativeBookingAccessibilityIds.SeatSelectionChildGuidanceSheet,
            ["loadError"] = NativeBookingAccessibilityIds.SeatSelectionError,
            ["retry"] = NativeBookingAccessibilityIds.SeatSelectionRetry,
            ["loading"] = NativeBookingAccessibilityIds.SeatSelectionLoading,
            ["submissionError"] = NativeBookingAccessibilityIds.SeatSelectionSubmissionError,
        };
        foreach ((string key, string value) in expected)
        {
            Assert.Equal(value, seats.Value<string>(key));
        }
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Adult One, Choose a seat", true)]
    [InlineData("Adult One, 21A, Window", false)]
    [InlineData("Adult One", false)]
    [InlineData(null, false)]
    public void UnassignedPassenger_RequiresExplicitState(string? label, bool expected)
        => Assert.Equal(expected, NativeSeatSelectionPage.IsUnassignedPassenger(label));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Adult One, 21A, Window", "21A", true)]
    [InlineData("Adult One, 121A, Window", "21A", false)]
    [InlineData("Adult One, Choose a seat", "21A", false)]
    [InlineData(null, "21A", false)]
    [InlineData("Adult One, 21A", "", false)]
    public void PassengerSeat_RequiresExactAssignedSeat(string? label, string seat, bool expected)
        => Assert.Equal(expected, NativeBookingPage.PassengerHasSeat(label, seat));
}
