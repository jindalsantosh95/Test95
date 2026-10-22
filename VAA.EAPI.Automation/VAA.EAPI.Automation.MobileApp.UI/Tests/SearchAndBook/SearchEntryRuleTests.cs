using System.Globalization;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

[Collection("Mobile")]
public sealed class SearchEntryRuleTests : SearchBookTestBase
{
    public SearchEntryRuleTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("Suite", "Sanity")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-Calendar-Passengers")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC05_SearchEntry_EnforcesCalendarAndPassengerRules()
    {
        TrySetBrowserStackSessionName();
        try
        {
            VerifySearchEntryRules();
            TrySetBrowserStackStatus(
                "passed",
                "Search entry enforced calendar and passenger rules and enabled the completed search.");
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception));
            throw;
        }
    }

    private void VerifySearchEntryRules()
    {
        NativeFlightSearchPage page = OpenSearchEntry();
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        Assert.False(page.IsSearchFlightsEnabled());
        page.SelectAirports("LHR", "JFK");
        page.SelectRoundTrip();

        page.OpenDateSelector();
        Assert.False(page.IsDateSelectionConfirmationEnabled());
        Assert.False(page.IsCalendarDateEnabled(today.AddDays(-1)));
        Assert.True(page.IsCalendarDateEnabled(today));

        DateOnly lastBookableDate = today.AddDays(330);
        Assert.True(page.IsCalendarDateEnabled(lastBookableDate),
            "The calendar must enable the inclusive 330-day booking boundary.");
        Assert.False(page.IsCalendarDateEnabled(lastBookableDate.AddDays(1)),
            "The calendar must disable dates beyond the 330-day booking boundary.");

        // Boundary inspection makes no selection. Reopen an uncommitted calendar before
        // validating the return range; calendar scrolling separately guards against selection changes.
        page.CancelDateSelection();
        page.OpenDateSelector();

        DateOnly departureDate = today.AddDays(7);
        DateOnly returnDate = today.AddDays(14);

        page.SelectDate(departureDate);
        AssertCalendarDateSummary(page.ReadCalendarSelection().DepartureText, departureDate, "departure");
        Assert.Contains("-", page.ReadCalendarSelection().ReturnText, StringComparison.Ordinal);
        Assert.False(page.IsDateSelectionConfirmationEnabled());
        page.SelectDate(returnDate);
        CalendarDateSelection selectedRange = page.ReadCalendarSelection();
        AssertCalendarDateSummary(selectedRange.DepartureText, departureDate, "departure");
        AssertCalendarDateSummary(selectedRange.ReturnText, returnDate, "return");
        Assert.True(page.IsDateSelectionConfirmationEnabled());
        page.ConfirmDateSelection();

        page.OpenPassengerSelector();
        page.IncrementPassenger(PassengerCategory.Infant);
        Assert.Equal(1, page.ReadPassengerCount(PassengerCategory.Adult));
        Assert.Equal(1, page.ReadPassengerCount(PassengerCategory.Infant));
        AssertPassengerIncrementRejected(page, PassengerCategory.Infant);

        page.AttemptPassengerDecrement(PassengerCategory.Adult);
        Assert.Equal(1, page.ReadPassengerCount(PassengerCategory.Adult));

        page.IncrementPassenger(PassengerCategory.Adult);
        page.IncrementPassenger(PassengerCategory.Infant);
        Assert.Equal(2, page.ReadPassengerCount(PassengerCategory.Infant));

        page.AttemptPassengerDecrement(PassengerCategory.Adult);
        Assert.Equal(2, page.ReadPassengerCount(PassengerCategory.Adult));

        for (int totalTravellers = 4; totalTravellers < 9; totalTravellers++)
        {
            page.IncrementPassenger(PassengerCategory.Adult);
        }

        Assert.Equal(7, page.ReadPassengerCount(PassengerCategory.Adult));
        AssertPassengerIncrementRejected(page, PassengerCategory.Adult);
        AssertPassengerIncrementRejected(page, PassengerCategory.Teen);
        AssertPassengerIncrementRejected(page, PassengerCategory.Child);
        AssertPassengerIncrementRejected(page, PassengerCategory.Infant);

        page.ConfirmPassengers();
        Assert.True(page.IsSearchFlightsEnabled());
    }

    private static void AssertPassengerIncrementRejected(
        NativeFlightSearchPage page,
        PassengerCategory category)
    {
        int countBeforeAttempt = page.ReadPassengerCount(category);
        page.AttemptPassengerIncrement(category);
        Assert.Equal(countBeforeAttempt, page.ReadPassengerCount(category));
    }

    private static void AssertCalendarDateSummary(string actual, DateOnly expected, string description)
    {
        string day = expected.Day.ToString(CultureInfo.InvariantCulture);
        string month = expected.ToDateTime(TimeOnly.MinValue).ToString("MMMM", CultureInfo.InvariantCulture);
        bool containsStandaloneDay = actual
            .Split(new[] { ' ', ',', '.', '/', '-' }, StringSplitOptions.RemoveEmptyEntries)
            .Contains(day, StringComparer.Ordinal);
        Assert.True(
            containsStandaloneDay &&
            actual.Contains(month, StringComparison.OrdinalIgnoreCase),
            $"The {description} calendar summary did not contain '{day} {month}'. Actual: '{actual}'.");
    }

    private NativeFlightSearchPage OpenSearchEntry()
    {
        new AppEntryHandler(Fixture.Driver).EnterAsGuest();
        NativeFlightSearchPage page = new(Fixture.Driver);
        page.Open();
        return page;
    }
}
