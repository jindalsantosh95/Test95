using System.Runtime.CompilerServices;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.SearchAndBook.UI.Pages;
using VAA.EAPI.Automation.SearchAndBook.UI.Support;
using Xunit;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Tests.Validation;

[Trait("Category", "SearchAndBook-UI-Regression")]
public sealed class PassengerValidationTests
{
    [SkippableFact]
    public Task AdultDob_EnforcesAgeBoundary() =>
        RunOnPassengerPageAsync(1, 0, 0, 0, async (page, departure) =>
        {
            await AssertAgeYearsAsync(page, 0, departure, [16, 30], [15, 1]);
            Assert.True(await page.IsDobSelectableAsync(0, departure.AddYears(-16)));
            Assert.False(await page.IsDobSelectableAsync(0, departure.AddYears(-16).AddDays(1)));
        });

    [SkippableFact]
    public Task TeenDob_EnforcesAgeRange() =>
        RunOnPassengerPageAsync(1, 1, 0, 0, async (page, departure) =>
        {
            await AssertAgeYearsAsync(page, 1, departure, [12, 15], [11, 17]);
            Assert.True(await page.IsDobSelectableAsync(1, departure.AddYears(-12)));
            Assert.False(await page.IsDobSelectableAsync(1, departure.AddYears(-12).AddDays(1)));
            Assert.False(await page.IsDobSelectableAsync(1, departure.AddYears(-16)));
        });

    [SkippableFact]
    public Task ChildDob_EnforcesAgeRange() =>
        RunOnPassengerPageAsync(1, 0, 1, 0, async (page, departure) =>
        {
            await AssertAgeYearsAsync(page, 1, departure, [2, 11], [1, 13]);
            Assert.True(await page.IsDobSelectableAsync(1, departure.AddYears(-2)));
            Assert.False(await page.IsDobSelectableAsync(1, departure.AddYears(-2).AddDays(1)));
            Assert.False(await page.IsDobSelectableAsync(1, departure.AddYears(-12)));
        });

    [SkippableFact]
    public Task InfantDob_EnforcesAgeRange() =>
        RunOnPassengerPageAsync(1, 0, 0, 1, async (page, departure) =>
        {
            await AssertAgeYearsAsync(page, 1, departure, [0, 1], [3]);
            Assert.True(await page.IsDobSelectableAsync(1, departure.AddYears(-2).AddDays(1)));
            Assert.False(await page.IsDobSelectableAsync(1, departure.AddYears(-2)));
        });

    [SkippableFact]
    [Trait("Category", "SearchAndBook-UI-Validation-NoSmoke")]
    public Task EmptyPassengerForm_CannotProgress() =>
        RunOnPassengerPageAsync(1, 0, 0, 0, async (page, _) =>
        {
            Assert.True(await page.IsEmptyFormSubmissionPreventedAsync());
        });

    [SkippableFact]
    public Task ValidPassengerForm_SubmitIsEnabled() =>
        RunOnPassengerPageAsync(1, 0, 0, 0, async (page, _) =>
        {
            await page.FillDefaultAdultGuestAsync(120_000);
            Assert.True(await page.IsSubmitEnabledAsync());
        });

    [SkippableFact]
    public Task Passenger_CanAddVirginLoyaltyAccount() =>
        RunOnPassengerPageAsync(1, 0, 0, 0, async (page, _) =>
        {
            LoyaltyAccount account = LoyaltyAccountProvider.GetAdultAccount("Red", 1);
            await page.AddLoyaltyAccountAsync("Virgin", account.FlyingClubNumber);
        }, assertionAction: "retrieving a Virgin Flying Club account and adding it to the passenger");

    [SkippableFact]
    public Task AdultYear2000_AllowsJanuaryAndDecember() =>
        RunOnPassengerPageAsync(1, 0, 0, 0, async (page, _) =>
        {
            await page.SetDobFieldAsync(0, "year", "2000");
            IReadOnlyList<string> months = await page.GetDobOptionsAsync(0, "month");
            Assert.Contains("01", months);
            Assert.Contains("12", months);
        });

    [SkippableFact]
    public Task LeapYearFebruary_AllowsDay29Not30() =>
        RunOnPassengerPageAsync(1, 0, 0, 0, async (page, _) =>
        {
            await page.SetDobFieldAsync(0, "year", "2000");
            await page.SetDobFieldAsync(0, "month", "02");
            IReadOnlyList<string> days = await page.GetDobOptionsAsync(0, "day");
            Assert.Contains("29", days);
            Assert.DoesNotContain("30", days);
        });

    [SkippableFact]
    public Task NonLeapYearFebruary_AllowsDay28Not29() =>
        RunOnPassengerPageAsync(1, 0, 0, 0, async (page, _) =>
        {
            await page.SetDobFieldAsync(0, "year", "2001");
            await page.SetDobFieldAsync(0, "month", "02");
            IReadOnlyList<string> days = await page.GetDobOptionsAsync(0, "day");
            Assert.Contains("28", days);
            Assert.DoesNotContain("29", days);
        });

    [SkippableFact]
    public Task April_RemovesPreviouslySelectedDay31() =>
        RunOnPassengerPageAsync(1, 0, 0, 0, async (page, _) =>
        {
            await page.SetDobFieldAsync(0, "year", "2000");
            await page.SetDobFieldAsync(0, "day", "31");
            await page.SetDobFieldAsync(0, "month", "04");
            IReadOnlyList<string> days = await page.GetDobOptionsAsync(0, "day");
            Assert.Contains("30", days);
            Assert.DoesNotContain("31", days);
        });

    private static async Task AssertAgeYearsAsync(
        PassengerDetailsPage page,
        int passengerIndex,
        DateOnly departure,
        IReadOnlyList<int> allowedAges,
        IReadOnlyList<int> disallowedAges)
    {
        IReadOnlyList<string> years = await page.GetDobOptionsAsync(passengerIndex, "year");
        foreach (int age in allowedAges)
        {
            Assert.Contains((departure.Year - age).ToString(), years);
        }
        foreach (int age in disallowedAges)
        {
            Assert.DoesNotContain((departure.Year - age).ToString(), years);
        }
    }

    private static async Task RunOnPassengerPageAsync(
        int adults,
        int teens,
        int children,
        int infants,
        Func<PassengerDetailsPage, DateOnly, Task> assertion,
        string assertionAction = "running the passenger-page validation assertion",
        [CallerMemberName] string testName = "")
    {
        string? baseUrl = Environment.GetEnvironmentVariable("SAB_BASE_URL");
        Skip.If(baseUrl.IsNullOrWhiteSpace(), "Set SAB_BASE_URL to run passenger validation journeys.");
        PlaywrightBrowserOptions options = PlaywrightBrowserOptions.FromEnvironment(baseUrl!);
        DateOnly departure = DateOnly.FromDateTime(
            DateTime.UtcNow.Date.AddMonths(Random.Shared.Next(3, 8)));
        SearchSliceDeepLink search = new()
        {
            Origin = "LHR",
            Destination = "JFK",
            DepartureDate = departure,
            Adults = adults,
            Teens = teens,
            Children = children,
            Infants = infants
        };

        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(options);
        try
        {
            session.SetCurrentAction(
                "opening flight results for passenger-page validation",
                testName);
            await new ApplicationPage(session.Page).OpenAsync(
                options.BaseUrl,
                search.ToRelativeUrl(),
                options.NavigationTimeoutMs);
            session.SetCurrentAction("selecting a flight for passenger-page validation");
            FlightResultsPage resultsPage = new(session.Page);
            await resultsPage.WaitUntilLoadedAsync(90_000);
            await resultsPage.SelectAnyAvailableFareAndWaitForSummaryAsync(120_000);
            session.SetCurrentAction("continuing from flight summary to passenger details");
            FlightSummaryPage summaryPage = new(session.Page);
            await summaryPage.ContinueToPassengerDetailsAsync(90_000);
            session.SetCurrentAction(assertionAction);
            PassengerDetailsPage passengerPage = new(session.Page);
            await passengerPage.WaitUntilReadyAsync(90_000);
            await assertion(passengerPage, departure);
            await session.MarkPassedAsync("Passenger-page validation passed.");
        }
        catch (Exception exception)
        {
            string failureReason = await session.MarkFailedAsync(exception);
            throw new InvalidOperationException(failureReason, exception);
        }
    }
}
