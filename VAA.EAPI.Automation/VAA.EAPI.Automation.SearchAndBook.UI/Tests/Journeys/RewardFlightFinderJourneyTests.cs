using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.SearchAndBook.UI.Flows;
using VAA.EAPI.Automation.SearchAndBook.UI.Support;
using Xunit;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Tests.Journeys;

[Trait("Category", "SearchAndBook-UI-RewardFlightFinder")]
public sealed class RewardFlightFinderJourneyTests
{
    private readonly ITestOutputHelper _output;

    public RewardFlightFinderJourneyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [SkippableFact]
    [Trait("tag", "search-and-book-ui-reward-flight-finder")]
    public async Task HappyPath_PopulatesPointsForEachCabin()
    {
        RewardFlightFinderEnvironment environment = RewardFlightFinderEnvironment.ReadHappyPathOrSkip();
        RewardFlightFinderFlow flow = CreateFlow(environment);

        IReadOnlyDictionary<string, string> cabinPoints = await flow.FindCabinPointsAsync(environment);

        foreach (string cabin in new[] { "Economy", "Premium", "Upper Class" })
        {
            Assert.True(
                cabinPoints.TryGetValue(cabin, out string? points) && !points.IsNullOrWhiteSpace(),
                $"Expected a populated reward-points price for {cabin}.");
            _output.WriteLine($"{cabin}: {points}");
        }
    }

    [SkippableFact]
    [Trait("tag", "search-and-book-ui-reward-flight-finder")]
    public async Task EmptySearch_IndicatesRequiredRouteFieldsAndRetainsMonthSelection()
    {
        RewardFlightFinderEnvironment environment = RewardFlightFinderEnvironment.ReadHappyPathOrSkip();
        RewardFlightFinderFlow flow = CreateFlow(environment);

        RewardFlightFinderRequiredFieldsResult result = await flow.ValidateRequiredFieldsAsync();

        Assert.Equal(new[] { "destination", "origin" }, result.InvalidFields.OrderBy(field => field, StringComparer.Ordinal));
        Assert.False(result.SelectedMonth.IsNullOrWhiteSpace());
    }

    [SkippableFact]
    [Trait("tag", "search-and-book-ui-reward-flight-finder")]
    public async Task SameAirport_IsExcludedFromDestinationChoices()
    {
        RewardFlightFinderEnvironment environment = RewardFlightFinderEnvironment.ReadHappyPathOrSkip();
        RewardFlightFinderFlow flow = CreateFlow(environment);

        IReadOnlyList<string> destinationOptions = await flow.GetDestinationOptionsAsync(environment.Origin);

        Assert.DoesNotContain(environment.Origin, destinationOptions);
    }

    [SkippableFact]
    [Trait("tag", "search-and-book-ui-reward-flight-finder")]
    public async Task MonthNavigation_StartsAtCurrentBoundaryAndMovesBetweenMonths()
    {
        RewardFlightFinderEnvironment environment = RewardFlightFinderEnvironment.ReadHappyPathOrSkip();
        RewardFlightFinderFlow flow = CreateFlow(environment);
        string expectedCurrentMonth = GetCurrentLondonMonthValue();
        string expectedFinalMonth = GetExpectedMonthValue(expectedCurrentMonth, 11);

        RewardFlightFinderMonthNavigationResult result = await flow.ValidateMonthNavigationAsync(environment, expectedFinalMonth);

        Assert.Equal(12, result.MonthOptions.Count);
        Assert.Equal(expectedCurrentMonth, result.MonthOptions[0]);
        Assert.Equal(expectedCurrentMonth, result.SelectedMonth);
        Assert.Equal(expectedFinalMonth, result.MonthOptions[^1]);
        Assert.True(result.PreviousInitiallyDisabled);
        Assert.False(result.NextInitiallyDisabled);
        Assert.False(result.PreviousAfterNextDisabled);
        Assert.Equal(result.InitialResultMonth, result.ReturnedResultMonth);
        Assert.False(result.PreviousAtFinalDisabled);
        Assert.True(result.NextAtFinalDisabled);
    }

    [SkippableFact]
    [Trait("tag", "search-and-book-ui-reward-flight-finder")]
    public async Task NoFlightDays_DisplayNoAvailabilityMessage()
    {
        RewardFlightFinderEnvironment environment = RewardFlightFinderEnvironment.ReadNoAvailabilityOrSkip();
        RewardFlightFinderFlow flow = CreateFlow(environment);

        IReadOnlyList<string> messages = await flow.FindNoAvailabilityMessagesAsync(
            environment.NoAvailabilityOrigin!,
            environment.NoAvailabilityDestination!,
            environment.NoAvailabilityMonth);

        Assert.NotEmpty(messages);
        _output.WriteLine($"No-availability days: {messages.Count}");
    }

    [SkippableFact]
    [Trait("tag", "search-and-book-ui-reward-flight-finder")]
    public async Task ReverseRoute_DisplaysCabinPointsForTheReturnDirection()
    {
        RewardFlightFinderEnvironment environment = RewardFlightFinderEnvironment.ReadHappyPathOrSkip();
        RewardFlightFinderFlow flow = CreateFlow(environment);

        IReadOnlyDictionary<string, string> cabinPoints = await flow.FindReverseRouteCabinPointsAsync(environment);

        Assert.Equal(3, cabinPoints.Count);
        Assert.All(cabinPoints.Values, points => Assert.False(points.IsNullOrWhiteSpace()));
    }

    [SkippableFact]
    [Trait("tag", "search-and-book-ui-reward-flight-finder")]
    public async Task EditSearch_ChangesRouteAndDisplaysNoAvailabilityMessage()
    {
        RewardFlightFinderEnvironment environment = RewardFlightFinderEnvironment.ReadNoAvailabilityOrSkip();
        RewardFlightFinderFlow flow = CreateFlow(environment);

        IReadOnlyList<string> messages = await flow.EditSearchForNoAvailabilityAsync(environment);

        Assert.NotEmpty(messages);
    }

    private static RewardFlightFinderFlow CreateFlow(RewardFlightFinderEnvironment environment)
    {
        return new RewardFlightFinderFlow(environment.BaseUrl);
    }

    private static string GetExpectedMonthValue(string firstMonthValue, int monthOffset)
    {
        DateTime firstMonth = DateTime.ParseExact(
            firstMonthValue,
            "MM_yyyy",
            System.Globalization.CultureInfo.InvariantCulture);
        return firstMonth.AddMonths(monthOffset).ToString("MM_yyyy", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string GetCurrentLondonMonthValue()
    {
        TimeZoneInfo londonTimeZone = GetLondonTimeZone();
        DateTimeOffset londonNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, londonTimeZone);
        return londonNow.ToString("MM_yyyy", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static TimeZoneInfo GetLondonTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        }
    }
}
