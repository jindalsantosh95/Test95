using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.SearchAndBook.UI.Pages;
using VAA.EAPI.Automation.SearchAndBook.UI.Support;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Flows;

internal sealed class RewardFlightFinderFlow
{
    private const int InteractionTimeoutMs = 30_000;
    private const int ResultsTimeoutMs = 90_000;
    private readonly PlaywrightBrowserOptions _options;

    public RewardFlightFinderFlow(string baseUrl)
    {
        _options = PlaywrightBrowserOptions.FromEnvironment(baseUrl);
    }

    public Task<IReadOnlyDictionary<string, string>> FindCabinPointsAsync(
        RewardFlightFinderEnvironment environment) =>
        RunAsync(
            "searching reward-seat availability",
            "Reward Flight Finder populated points for Economy, Premium and Upper Class.",
            async page =>
            {
                await page.SearchAsync(environment.Origin, environment.Destination, environment.Month, ResultsTimeoutMs);
                return await page.GetCabinPointsAsync(ResultsTimeoutMs);
            });

    public Task<RewardFlightFinderRequiredFieldsResult> ValidateRequiredFieldsAsync() =>
        RunAsync(
            "validating the required search fields",
            "Empty Reward Flight Finder search highlighted required route fields and retained its mandatory month.",
            async page => new RewardFlightFinderRequiredFieldsResult(
                await page.GetInvalidRequiredSearchFieldsAsync(InteractionTimeoutMs),
                await page.GetSelectedMonthValueAsync(InteractionTimeoutMs)));

    public Task<IReadOnlyList<string>> GetDestinationOptionsAsync(string origin) =>
        RunAsync(
            "checking same-airport destination prevention",
            "Selected origin was excluded from Reward Flight Finder destination choices.",
            page => page.SelectOriginAndGetDestinationOptionsAsync(origin, InteractionTimeoutMs));

    public Task<RewardFlightFinderMonthNavigationResult> ValidateMonthNavigationAsync(
        RewardFlightFinderEnvironment environment,
        string finalMonth) =>
        RunAsync(
            "validating Reward Flight Finder month navigation",
            "Reward Flight Finder respected current, first and last month boundaries.",
            async page =>
            {
                IReadOnlyList<string> monthOptions = await page.GetMonthOptionValuesAsync(InteractionTimeoutMs);
                string selectedMonth = await page.GetSelectedMonthValueAsync(InteractionTimeoutMs);
                await page.SearchAsync(environment.Origin, environment.Destination, selectedMonth, ResultsTimeoutMs);

                string initialResultMonth = await page.GetDisplayedResultMonthAsync(InteractionTimeoutMs);
                bool previousInitiallyDisabled = await page.IsMonthNavigationDisabledAsync("Previous month", InteractionTimeoutMs);
                bool nextInitiallyDisabled = await page.IsMonthNavigationDisabledAsync("Next month", InteractionTimeoutMs);

                await page.NavigateMonthAsync("Next month", ResultsTimeoutMs);
                bool previousAfterNextDisabled = await page.IsMonthNavigationDisabledAsync("Previous month", InteractionTimeoutMs);
                await page.NavigateMonthAsync("Previous month", ResultsTimeoutMs);
                string returnedResultMonth = await page.GetDisplayedResultMonthAsync(InteractionTimeoutMs);

                await page.EditSearchAsync(ResultsTimeoutMs);
                await page.SearchAsync(environment.Origin, environment.Destination, finalMonth, ResultsTimeoutMs);
                bool previousAtFinalDisabled = await page.IsMonthNavigationDisabledAsync("Previous month", InteractionTimeoutMs);
                bool nextAtFinalDisabled = await page.IsMonthNavigationDisabledAsync("Next month", InteractionTimeoutMs);

                return new RewardFlightFinderMonthNavigationResult(
                    monthOptions,
                    selectedMonth,
                    initialResultMonth,
                    returnedResultMonth,
                    previousInitiallyDisabled,
                    nextInitiallyDisabled,
                    previousAfterNextDisabled,
                    previousAtFinalDisabled,
                    nextAtFinalDisabled);
            });

    public Task<IReadOnlyList<string>> FindNoAvailabilityMessagesAsync(
        string origin,
        string destination,
        string? month) =>
        RunAsync(
            "searching an itinerary with no-flight days",
            "Reward Flight Finder displayed its no-flight availability state.",
            async page =>
            {
                await page.SearchAsync(origin, destination, month, ResultsTimeoutMs);
                return await page.GetNoAvailabilityMessagesAsync(ResultsTimeoutMs);
            });

    public Task<IReadOnlyDictionary<string, string>> FindReverseRouteCabinPointsAsync(
        RewardFlightFinderEnvironment environment) =>
        RunAsync(
            "searching and reversing reward-seat availability",
            "Reward Flight Finder displayed cabin points after reversing the route.",
            async page =>
            {
                await page.SearchAsync(environment.Origin, environment.Destination, environment.Month, ResultsTimeoutMs);
                await page.ReverseRouteAsync(environment.Origin, environment.Destination, ResultsTimeoutMs);
                return await page.GetCabinPointsAsync(ResultsTimeoutMs);
            });

    public Task<IReadOnlyList<string>> EditSearchForNoAvailabilityAsync(
        RewardFlightFinderEnvironment environment) =>
        RunAsync(
            "editing a reward-seat search to an itinerary with no-flight days",
            "Edited Reward Flight Finder search displayed its no-flight availability state.",
            async page =>
            {
                await page.SearchAsync(environment.Origin, environment.Destination, environment.Month, ResultsTimeoutMs);
                await page.EditSearchAsync(ResultsTimeoutMs);
                await page.SearchAsync(
                    environment.NoAvailabilityOrigin!,
                    environment.NoAvailabilityDestination!,
                    environment.NoAvailabilityMonth,
                    ResultsTimeoutMs);
                return await page.GetNoAvailabilityMessagesAsync(ResultsTimeoutMs);
            });

    private async Task<TResult> RunAsync<TResult>(
        string action,
        string successMessage,
        Func<RewardFlightFinderPage, Task<TResult>> journey)
    {
        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(_options);
        try
        {
            session.SetCurrentAction("opening Reward Flight Finder");
            ApplicationPage applicationPage = new(session.Page);
            await applicationPage.OpenAsync(_options.BaseUrl, "/reward-flight-finder", _options.NavigationTimeoutMs);

            session.SetCurrentAction(action);
            RewardFlightFinderPage rewardFlightFinderPage = new(session.Page);
            TResult result = await journey(rewardFlightFinderPage);
            await session.MarkPassedAsync(successMessage);
            return result;
        }
        catch (Exception exception)
        {
            string failureReason = await session.MarkFailedAsync(exception);
            throw new InvalidOperationException(failureReason, exception);
        }
    }
}
