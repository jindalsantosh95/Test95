using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class CalendarNavigationTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("13 Jan", "20 Jan")]
    [InlineData("13 Jan", "")]
    public void UnchangedSelection_AllowsIntentionalDateTap(string departure, string inbound)
        => NativeFlightSearchPage.AssertCalendarSelectionUnchanged(
            new CalendarDateSelection(departure, inbound), new CalendarDateSelection(departure, inbound));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("14 Jan", "20 Jan")]
    [InlineData("13 Jan", "21 Jan")]
    [InlineData("13 Jan", "")]
    public void DelayedScrollSelectionChange_BlocksSubsequentDateTap(string departure, string inbound)
        => Assert.Throws<InvalidOperationException>(() => NativeFlightSearchPage.AssertCalendarSelectionUnchanged(
            new CalendarDateSelection("13 Jan", "20 Jan"), new CalendarDateSelection(departure, inbound)));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    public void ReversedScroll_ReducesTravelInsideTheMeasuredGutter(int divisor)
    {
        using System.Text.Json.JsonDocument payload = System.Text.Json.JsonDocument.Parse(
            System.Text.Json.JsonSerializer.Serialize(
                NativeFlightSearchPage.CalendarScrollGesture(16, 252, 591, true, divisor).ToDictionary()));
        System.Text.Json.JsonElement actions = payload.RootElement.GetProperty("actions");
        int start = actions[0].GetProperty("y").GetInt32();
        int end = actions[2].GetProperty("y").GetInt32();
        Assert.InRange(start, 252, 590);
        Assert.InRange(end, start + 1, 591);
        Assert.InRange(end - start, 339 / divisor, 339 / divisor + 1);
        Assert.Equal(350, actions[2].GetProperty("duration").GetInt32());
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(16)]
    public void UnsupportedScrollCorrection_FailsBeforeInteraction(int divisor)
        => Assert.Throws<ArgumentOutOfRangeException>(() =>
            NativeFlightSearchPage.CalendarScrollGesture(16, 252, 591, true, divisor));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(false, 591, 252)]
    [InlineData(true, 252, 591)]
    public void CalendarSwipe_MovesImmediatelyWithoutHoldingADay(bool earlier, int startY, int endY)
    {
        using System.Text.Json.JsonDocument payload = System.Text.Json.JsonDocument.Parse(
            System.Text.Json.JsonSerializer.Serialize(
                NativeFlightSearchPage.CalendarScrollGesture(16, 252, 591, earlier).ToDictionary()));
        System.Text.Json.JsonElement root = payload.RootElement;
        Assert.Equal("touch", root.GetProperty("parameters").GetProperty("pointerType").GetString());
        System.Text.Json.JsonElement actions = root.GetProperty("actions");
        Assert.Equal(4, actions.GetArrayLength());
        Assert.Equal("pointerMove", actions[0].GetProperty("type").GetString());
        Assert.Equal(0, actions[0].GetProperty("duration").GetInt32());
        Assert.Equal(startY, actions[0].GetProperty("y").GetInt32());
        Assert.Equal("pointerDown", actions[1].GetProperty("type").GetString());
        Assert.Equal("pointerMove", actions[2].GetProperty("type").GetString());
        Assert.Equal(350, actions[2].GetProperty("duration").GetInt32());
        Assert.Equal(endY, actions[2].GetProperty("y").GetInt32());
        Assert.Equal(16, actions[0].GetProperty("x").GetInt32());
        Assert.Equal(16, actions[2].GetProperty("x").GetInt32());
        Assert.Equal("viewport", actions[2].GetProperty("origin").GetString());
        Assert.Equal("pointerUp", actions[3].GetProperty("type").GetString());
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(149, 44, false)]
    [InlineData(150, 44, true)]
    [InlineData(636, 44, true)]
    [InlineData(650, 44, false)]
    [InlineData(200, 0, false)]
    public void DateTap_RequiresEntireCellAboveFooter(int y, int height, bool expected)
        => Assert.Equal(expected, NativeFlightSearchPage.CalendarDayFits(
            new System.Drawing.Rectangle(32, y, 44, height), 150, 680));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(32, 320, 16)]
    [InlineData(0, 368, 384)]
    public void ScrollGutter_UsesObservedCellBoundsNotScreenPercentages(int x, int width, int expectedX)
    {
        string source = $"<App><Day name='NativeFlightCalendarDay_2027-01-04' visible='true' x='{x}' y='200' width='{width}' height='44'/></App>";
        (int gutter, int upper, int lower) = NativeFlightSearchPage.CalendarGutter(
            source, new System.Drawing.Rectangle(0, 100, 400, 700), 150, 680);
        Assert.Equal(expectedX, gutter);
        Assert.InRange(upper, 151, 679);
        Assert.InRange(lower, upper + 1, 679);
        Assert.True(gutter < x || gutter > x + width);
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("<App/>")]
    [InlineData("<App><Day name='NativeFlightCalendarDay_2027-01-04' visible='true' y='200' height='44'/></App>")]
    [InlineData("<App><Day name='NativeFlightCalendarDay_2027-01-04' visible='true' x='0' y='200' width='400' height='44'/></App>")]
    [InlineData("<App><Day name='NativeFlightCalendarDay_2027-01-04' visible='true' x='32' y='700' width='44' height='44'/></App>")]
    public void UnsafeGutter_FailsRatherThanDraggingAcrossDates(string source)
        => Assert.Throws<InvalidOperationException>(() => NativeFlightSearchPage.CalendarGutter(
            source, new System.Drawing.Rectangle(0, 100, 400, 700), 150, 680));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void ScrollGutter_RespectsOffsetContainerAndAllVisibleColumns()
    {
        const string source = """
            <App>
              <Day name="NativeFlightCalendarDay_2027-01-04" visible="true" x="82" y="200" width="44" height="44"/>
              <Day name="NativeFlightCalendarDay_2027-01-05" visible="true" x="360" y="250" width="44" height="44"/>
            </App>
            """;
        (int x, int upper, int lower) = NativeFlightSearchPage.CalendarGutter(
            source, new System.Drawing.Rectangle(50, 160, 400, 500), 150, 680);
        Assert.Equal(66, x);
        Assert.InRange(upper, 161, 659);
        Assert.InRange(lower, upper + 1, 659);
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(2026, 12, 28, true)]
    [InlineData(2027, 1, 4, false)]
    [InlineData(2027, 9, 28, false)]
    public void RecycledTarget_UsesObservedDatesToChooseDirection(int year, int month, int day, bool earlier)
    {
        const string source = """
            <App>
              <Day name="NativeFlightCalendarDay_2026-09-28" visible="false" y="200" height="44" />
              <Day name="NativeFlightCalendarDay_2026-12-28" visible="true" y="90" height="44" />
              <Day name="NativeFlightCalendarDay_2027-01-04" visible="true" y="200" height="44" />
              <Day name="NativeFlightCalendarDay_2027-01-05" visible="true" y="244" height="44" />
              <Day name="NativeFlightCalendarDay_2026-09-29" visible="true" y="700" height="44" />
            </App>
            """;
        Assert.Equal(earlier, NativeFlightSearchPage.CalendarTargetPrecedesVisibleDates(
            new DateOnly(year, month, day), source, 150, 680));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("<App />")]
    [InlineData("<App><Day name='NativeFlightCalendarDay_invalid' visible='true' y='200' height='44' /></App>")]
    [InlineData("<App><Day name='NativeFlightCalendarDay_2027-01-04' visible='false' y='200' height='44' /></App>")]
    public void MissingDateEvidence_FailsInsteadOfScrollingBlindly(string source)
        => Assert.Throws<InvalidOperationException>(() => NativeFlightSearchPage.CalendarTargetPrecedesVisibleDates(
            new DateOnly(2026, 12, 28), source, 150, 680));
}
