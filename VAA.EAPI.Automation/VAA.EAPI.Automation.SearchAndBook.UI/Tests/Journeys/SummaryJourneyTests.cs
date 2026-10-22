using System.Globalization;
using VAA.EAPI.Automation.SearchAndBook.UI.Assertions;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.SearchAndBook.UI.Pages;
using VAA.EAPI.Automation.SearchAndBook.UI.Support;
using Xunit;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Tests.Journeys;

[Trait("Category", "SearchAndBook-UI-Regression")]
public sealed class SummaryJourneyTests
{
    private readonly ITestOutputHelper _output;

    public SummaryJourneyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [SkippableFact]
    [Trait("Category", "SearchAndBook-UI-LoggedIn")]
    [Trait("tag", "search-and-book-ui-logged-in")]
    public async Task RewardSearch_WithInsufficientPoints_BlocksPassengerContinue()
    {
        string baseUrl = ReadBaseUrlOrSkip();
        string password = ReadLoyaltyPasswordOrSkip();
        PlaywrightBrowserOptions options = PlaywrightBrowserOptions.FromEnvironment(baseUrl);
        SearchSliceDeepLink search = CreateSearch(awardSearch: true);
        LoyaltyAccount account = LoyaltyAccountProvider.GetAdultAccount(
            "Red",
            minimumPoints: 0,
            maximumPoints: 5_001);

        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(options);
        session.SetCurrentAction(
            "signing in for an insufficient-points reward search",
            nameof(RewardSearch_WithInsufficientPoints_BlocksPassengerContinue));
        try
        {
            FlyingClubB2CLoginPage loginPage = new(session.Page);
            await loginPage.SignInAsync(
                search.ToRelativeUrl(),
                account.FlyingClubNumber,
                password,
                90_000);
            await new ApplicationPage(session.Page).DismissPrivacyBannerIfPresentAsync();

            session.SetCurrentAction("selecting an Economy Classic reward fare");
            FlightResultsPage resultsPage = new(session.Page);
            await resultsPage.WaitUntilLoadedAsync(90_000);
            await resultsPage.SelectVirginAtlanticFareAndWaitForSummaryAsync(
                "Economy Classic",
                120_000);

            session.SetCurrentAction("validating the insufficient-points state on flight summary");
            FlightSummaryPage summaryPage = new(session.Page);
            string warning = await summaryPage.AssertInsufficientPointsBlocksContinueAsync(90_000);

            _output.WriteLine($"Account points: {account.Points}");
            _output.WriteLine($"Warning: {warning}");
            await session.MarkPassedAsync("Insufficient-points summary validation passed.");
        }
        catch (Exception exception)
        {
            string failureReason = await session.MarkFailedAsync(exception);
            throw new InvalidOperationException(failureReason, exception);
        }
    }

    [SkippableFact]
    public async Task EconomyLight_SummaryUpgrade_ReachesHppWithUpgradedPrice()
    {
        string baseUrl = ReadBaseUrlOrSkip();
        PlaywrightBrowserOptions options = PlaywrightBrowserOptions.FromEnvironment(baseUrl);
        SearchSliceDeepLink search = CreateSearch(awardSearch: false);

        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(options);
        session.SetCurrentAction(
            "opening flight results for the summary-upgrade journey",
            nameof(EconomyLight_SummaryUpgrade_ReachesHppWithUpgradedPrice));
        try
        {
            await new ApplicationPage(session.Page).OpenAsync(
                options.BaseUrl,
                search.ToRelativeUrl(),
                options.NavigationTimeoutMs);

            session.SetCurrentAction("selecting an Economy Light fare for summary upgrade");
            FlightResultsPage resultsPage = new(session.Page);
            await resultsPage.WaitUntilLoadedAsync(90_000);
            await resultsPage.SelectVirginAtlanticFareAndWaitForSummaryAsync(
                "Economy Light",
                120_000);

            session.SetCurrentAction("applying and validating the summary-page cabin upgrade");
            BookingPriceSummary priceSummary = new(session.Page);
            PriceSummarySnapshot beforeUpgrade = await priceSummary.CaptureAsync(30_000);
            FlightSummaryPage summaryPage = new(session.Page);
            string targetCabin = await summaryPage.ApplyAvailableUpgradeAsync(30_000);
            PriceSummarySnapshot afterUpgrade = await priceSummary.CaptureAsync(30_000);
            AssertPriceIncreased(beforeUpgrade, afterUpgrade);

            session.SetCurrentAction("completing passenger details after the summary upgrade");
            await summaryPage.ContinueToPassengerDetailsAsync(90_000);
            PassengerDetailsPage passengerPage = new(session.Page);
            await passengerPage.FillGuestPassengersAsync(
                adults: 1,
                teens: 0,
                children: 0,
                infants: 0,
                search.DepartureDate,
                acceptInsurance: false,
                preserveLeadPassenger: false,
                timeoutMs: 120_000);
            await passengerPage.SubmitAndWaitForProgressAsync(90_000);
            SeatSelectionPage seatPage = new(session.Page);
            await seatPage.SkipSeatSelectionIfShownAsync(120_000);

            session.SetCurrentAction("validating the upgraded price and hosted payment form");
            PriceSummarySnapshot paymentPrice = await priceSummary.CaptureAsync(30_000);
            BookingPriceAssertions.AssertConsistent(afterUpgrade, paymentPrice, "Upgraded payment");
            PaymentPage paymentPage = new(session.Page);
            await paymentPage.OpenCardPaymentAsync(90_000);

            _output.WriteLine($"Upgrade target: {targetCabin}");
            _output.WriteLine($"Before upgrade: {beforeUpgrade}");
            _output.WriteLine($"After upgrade: {afterUpgrade}");
            _output.WriteLine("Upgraded journey reached an interactable HPP card form.");
            await session.MarkPassedAsync("Summary upgrade journey reached the HPP card form.");
        }
        catch (Exception exception)
        {
            string failureReason = await session.MarkFailedAsync(exception);
            throw new InvalidOperationException(failureReason, exception);
        }
    }

    private static SearchSliceDeepLink CreateSearch(bool awardSearch)
    {
        DateOnly departure = DateOnly.FromDateTime(
            DateTime.UtcNow.Date.AddMonths(Random.Shared.Next(3, 8)));
        return new SearchSliceDeepLink
        {
            Origin = "LHR",
            Destination = "JFK",
            DepartureDate = departure,
            Adults = 1,
            AwardSearch = awardSearch,
            Cta = awardSearch ? "AbTest_SP_Flights" : null
        };
    }

    private static void AssertPriceIncreased(
        PriceSummarySnapshot before,
        PriceSummarySnapshot after)
    {
        BookingPriceAssertions.AssertValid(before, "Pre-upgrade summary");
        BookingPriceAssertions.AssertValid(after, "Post-upgrade summary");
        if (!before.Currency.Equals(after.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Upgrade changed currency from '{before.Currency}' to '{after.Currency}'.");
        }

        decimal beforeTotal = decimal.Parse(
            before.Total,
            NumberStyles.Number,
            CultureInfo.InvariantCulture);
        decimal afterTotal = decimal.Parse(
            after.Total,
            NumberStyles.Number,
            CultureInfo.InvariantCulture);
        if (afterTotal <= beforeTotal)
        {
            throw new InvalidOperationException(
                $"Upgrade did not increase the basket total. Before: {before}; after: {after}.");
        }
    }

    private static string ReadBaseUrlOrSkip()
    {
        string? baseUrl = Environment.GetEnvironmentVariable("SAB_BASE_URL");
        Skip.If(baseUrl.IsNullOrWhiteSpace(), "Set SAB_BASE_URL to run Search & Book journeys.");
        return baseUrl!;
    }

    private static string ReadLoyaltyPasswordOrSkip()
    {
        string? password = Environment.GetEnvironmentVariable("LoyaltyAccount");
        Skip.If(password.IsNullOrWhiteSpace(), "Set LoyaltyAccount to run logged-in journeys.");
        return password!;
    }
}
