using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.SearchAndBook.UI.Pages;
using VAA.EAPI.Automation.SearchAndBook.UI.Support;
using Xunit;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Tests.Journeys;

public sealed class VoucherRedemptionJourneyTests
{
    private readonly ITestOutputHelper _output;

    public VoucherRedemptionJourneyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [SkippableFact]
    [Trait("Category", "SearchAndBook-UI-Vouchers")]
    [Trait("WorkItem", "862129")]
    [Trait("tag", "search-and-book-ui-voucher")]
    public async Task EligibleVoucher_IsOfferedForVirginAtlanticRewardFlight()
    {
        EnsureConfiguredOrSkip();
        DateOnly departure = FutureDeparture();
        VoucherLoyaltyAccount account = LoyaltyAccountProvider.GetVoucherAccount(
            validOnOrAfter: departure);
        SearchSliceDeepLink search = CreateRewardSearch("JFK", departure);

        await using PlaywrightBrowserSession session = await SignInAsync(search, account);
        await SelectFareAsync(session, "Economy Classic");

        VoucherRedemptionPage voucherPage = new(session.Page);
        await voucherPage.AssertSummaryGuidanceAndAcceptAsync(30_000);
        VoucherUiSnapshot voucher = await voucherPage.WaitForOptionsAsync(60_000);

        _output.WriteLine($"Voucher expiries: {string.Join(", ", account.VoucherExpiryDates)}");
        _output.WriteLine($"Voucher options: {string.Join(" | ", voucher.OptionTexts)}");
        await session.MarkPassedAsync("Eligible voucher was offered for a VS reward itinerary.");
    }

    [SkippableFact]
    [Trait("Category", "SearchAndBook-UI-Vouchers")]
    [Trait("WorkItem", "862129")]
    [Trait("tag", "search-and-book-ui-voucher")]
    public async Task VoucherAndPointsBalanceTooLow_ShowsInsufficientPointsAndBuyPointsOnSummary()
    {
        EnsureConfiguredOrSkip();
        DateOnly departure = BackendRewardDeparture();
        VoucherLoyaltyAccount account = LoyaltyAccountProvider.GetVoucherAccount(
            validOnOrAfter: departure,
            maximumPoints: 30_000);
        SearchSliceDeepLink search = CreateRewardSearch("JFK", departure) with
        {
            ReturnDate = departure.AddDays(10),
            Cta = null
        };

        await using PlaywrightBrowserSession session = await SignInAsync(search, account);
        await SelectFareAsync(session, "Premium", expectedSlices: 2);

        VoucherRedemptionPage voucherPage = new(session.Page);
        await voucherPage.AssertSummaryGuidanceAndAcceptAsync(30_000);
        VoucherUiSnapshot voucher = await voucherPage.WaitForOptionsAsync(60_000);
        string warning = await new FlightSummaryPage(session.Page)
            .AssertInsufficientPointsBlocksContinueAsync(60_000, account.Points);

        _output.WriteLine($"Voucher options: {string.Join(" | ", voucher.OptionTexts)}");
        _output.WriteLine($"Insufficient-points warning: {warning}");
        await session.MarkPassedAsync("Voucher was applied and the insufficient-points state was returned on Summary.");
    }

    [SkippableFact]
    [Trait("Category", "SearchAndBook-UI-Vouchers")]
    [Trait("WorkItem", "862129")]
    [Trait("tag", "search-and-book-ui-voucher")]
    public async Task VoucherExpiringBeforeOutbound_IsNotOfferedAndBookingCanContinue()
    {
        EnsureConfiguredOrSkip();
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        VoucherLoyaltyAccount account = LoyaltyAccountProvider.GetVoucherAccount(
            validOnOrAfter: today,
            expiresBefore: today.AddMonths(6),
            exactVoucherCount: 1,
            requireAllVouchersInExpiryRange: true);
        DateOnly expiry = account.VoucherExpiryDates
            .First(date => date >= today && date < today.AddMonths(6));
        SearchSliceDeepLink search = CreateRewardSearch("JFK", expiry.AddDays(1));

        await using PlaywrightBrowserSession session = await SignInAsync(search, account);
        await SelectFareAsync(session, "Economy Classic");

        VoucherRedemptionPage voucherPage = new(session.Page);
        await voucherPage.AssertNoOptionsAsync();
        await new FlightSummaryPage(session.Page).ContinueToPassengerDetailsAsync(90_000);

        _output.WriteLine($"Voucher expiry: {expiry}; outbound: {search.DepartureDate}.");
        await session.MarkPassedAsync("Expired voucher was excluded and the non-voucher flow remained available.");
    }

    [SkippableFact]
    [Trait("Category", "SearchAndBook-UI-Vouchers")]
    [Trait("WorkItem", "862129")]
    [Trait("tag", "search-and-book-ui-voucher")]
    public async Task PartnerOperatedFlight_DoesNotOfferVoucherAndBookingCanContinue()
    {
        Skip.If(true, "Awaiting frontend delivery: partner itineraries cannot yet continue through Summary without voucher redemption.");
        EnsureConfiguredOrSkip();
        DateOnly departure = BackendRewardDeparture();
        VoucherLoyaltyAccount account = LoyaltyAccountProvider.GetVoucherAccount(
            validOnOrAfter: departure);
        SearchSliceDeepLink search = CreateRewardSearch("JFK", departure) with { Cta = null };

        await using PlaywrightBrowserSession session = await SignInAsync(search, account);
        FlightResultsPage results = new(session.Page);
        await results.WaitUntilLoadedAsync(90_000);
        await new VoucherRedemptionPage(session.Page)
            .AssertFlightSelectionGuidanceAndAcceptAsync(30_000);
        if (Environment.GetEnvironmentVariable("SAB_PAUSE_AFTER_RESULTS").Is("true"))
        {
            await new ApplicationPage(session.Page).PauseForInteractiveInspectionAsync();
        }
        string partnerFare = await results.SelectAnyAvailablePartnerFareAndWaitForSummaryAsync(120_000);

        await new VoucherRedemptionPage(session.Page).AssertNoOptionsAsync();
        await new FlightSummaryPage(session.Page).ContinueToPassengerDetailsAsync(90_000);

        _output.WriteLine($"Partner fare: {partnerFare}");
        await session.MarkPassedAsync("Partner itinerary excluded vouchers and retained the standard booking flow.");
    }

    [SkippableTheory]
    [Trait("Category", "SearchAndBook-UI-Vouchers")]
    [InlineData("JFK")]
    [InlineData("ATL")]
    [Trait("WorkItem", "862129")]
    [Trait("WorkItem", "862130")]
    [Trait("tag", "search-and-book-ui-voucher")]
    public async Task ReturnVoucher_UpgradePreferredWhenAtLeastAsValuableAtPayment(string destination)
    {
        EnsureConfiguredOrSkip();
        const int availabilityWindowDays = 3;
        DateOnly initialDeparture = BackendRewardDeparture();
        VoucherLoyaltyAccount account = LoyaltyAccountProvider.GetVoucherAccount(
            validOnOrAfter: initialDeparture.AddDays(availabilityWindowDays - 1),
            minimumPoints: 300_000);
        PlaywrightBrowserSession? selectedSession = null;
        SearchSliceDeepLink? selectedSearch = null;
        string selectedFares = string.Empty;
        List<string> attemptedSearches = [];

        for (int dayOffset = 0; dayOffset < availabilityWindowDays; dayOffset++)
        {
            DateOnly candidateDeparture = initialDeparture.AddDays(dayOffset);
            SearchSliceDeepLink candidateSearch = CreateRewardSearch(destination, candidateDeparture) with
            {
                ReturnDate = candidateDeparture.AddDays(10),
                Cta = null
            };
            PlaywrightBrowserSession candidateSession = await SignInAsync(candidateSearch, account);
            try
            {
                FlightResultsPage candidateResults = new(candidateSession.Page);
                await candidateResults.WaitUntilLoadedAsync(90_000);
                await new VoucherRedemptionPage(candidateSession.Page)
                    .AcceptFlightSelectionGuidanceIfShownAsync();
                selectedFares = await candidateResults
                    .SelectVirginAtlanticPremiumWithLargestUpgradeDeltaAndWaitForSummaryAsync(120_000, expectedSlices: 2);
                selectedSession = candidateSession;
                selectedSearch = candidateSearch;
                break;
            }
            catch (InvalidOperationException exception) when (
                exception.Message.Contains("no VS card exposed enabled Economy Classic and Premium", StringComparison.OrdinalIgnoreCase))
            {
                attemptedSearches.Add(
                    $"LHR-{destination}-LHR {candidateDeparture:yyyy-MM-dd}/{candidateSearch.ReturnDate:yyyy-MM-dd}: {exception.Message}");
                await candidateSession.DisposeAsync();
            }
            catch
            {
                await candidateSession.DisposeAsync();
                throw;
            }
        }

        if (selectedSession == null || selectedSearch == null)
        {
            throw new InvalidOperationException(
                $"No return reward search produced paired VS Economy Classic and Premium fares across the {availabilityWindowDays}-day discovery window. " +
                $"Attempts: {string.Join(" | ", attemptedSearches)}");
        }

        await using PlaywrightBrowserSession session = selectedSession;
        SearchSliceDeepLink search = selectedSearch;
        DateOnly departure = search.DepartureDate;

        FlightSummaryPage summary = new(session.Page);
        await summary.AssertJourneySlicesAsync("LHR", destination, isReturn: true);
        await new VoucherRedemptionPage(session.Page)
            .AssertSummaryGuidanceAndAcceptAsync(30_000);
        VoucherTieSnapshot comparison = await new VoucherRedemptionPage(session.Page)
            .AssertUpgradePreferredWhenAtLeastAsValuableAsync(30_000);
        await summary.ContinueToPassengerDetailsAsync(90_000);
        PassengerDetailsPage passengers = new(session.Page);
        await passengers.FillGuestPassengersAsync(
            adults: 2,
            teens: 0,
            children: 0,
            infants: 0,
            departureDate: departure,
            acceptInsurance: false,
            preserveLeadPassenger: true,
            timeoutMs: 120_000);
        await passengers.SubmitAndWaitForProgressAsync(90_000);
        await new SeatSelectionPage(session.Page).SkipSeatSelectionIfShownAsync(120_000);

        await new VoucherRedemptionPage(session.Page)
            .AssertUpgradePreferredAtPaymentAsync(90_000);

        _output.WriteLine($"Route: LHR-{destination}-LHR; selected maximum-delta fares: {selectedFares}; upgrade points to pay: {string.Join(", ", comparison.Amounts)}");
        await session.MarkPassedAsync("Upgrade was preferred when it was at least as valuable as Companion at Payment.");
    }

    [SkippableFact]
    [Trait("Category", "SearchAndBook-UI-Vouchers")]
    [Trait("WorkItem", "862129")]
    [Trait("tag", "search-and-book-ui-voucher")]
    public async Task ReturnVoucher_ExpiringAfterOutboundBeforeInbound_RemainsEligible()
    {
        EnsureConfiguredOrSkip();
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        VoucherLoyaltyAccount account = LoyaltyAccountProvider.GetVoucherAccount(
            validOnOrAfter: today.AddMonths(2),
            expiresBefore: today.AddMonths(10),
            exactVoucherCount: 1,
            requireAllVouchersInExpiryRange: true);
        DateOnly expiry = account.VoucherExpiryDates.Single();
        DateOnly departure = expiry.AddDays(-7);
        SearchSliceDeepLink search = CreateRewardSearch("JFK", departure) with
        {
            ReturnDate = expiry.AddDays(7),
            Cta = null
        };

        await using PlaywrightBrowserSession session = await SignInAsync(search, account);
        await SelectFareAsync(session, "Economy Classic", expectedSlices: 2);

        VoucherRedemptionPage voucherPage = new(session.Page);
        await voucherPage.AssertSummaryGuidanceAndAcceptAsync(30_000);
        VoucherUiSnapshot voucher = await voucherPage.WaitForOptionsAsync(60_000);

        _output.WriteLine(
            $"Voucher expiry: {expiry}; outbound: {departure}; inbound: {search.ReturnDate}; " +
            $"options: {string.Join(" | ", voucher.OptionTexts)}");
        await session.MarkPassedAsync(
            "Voucher remained eligible when only the inbound journey fell after its expiry date.");
    }

    [SkippableFact]
    [Trait("Category", "SearchAndBook-UI-Vouchers")]
    [Trait("WorkItem", "862129")]
    [Trait("tag", "search-and-book-ui-voucher")]
    public async Task SinglePassengerEconomyFare_ShowsVoucherUnavailableAndPointsOption()
    {
        EnsureConfiguredOrSkip();
        DateOnly departure = BackendRewardDeparture();
        VoucherLoyaltyAccount account = LoyaltyAccountProvider.GetVoucherAccount(
            validOnOrAfter: departure,
            minimumPoints: 300_000);
        SearchSliceDeepLink search = CreateRewardSearch("JFK", departure) with { Adults = 1 };

        await using PlaywrightBrowserSession session = await SignInAsync(search, account);
        await SelectFareAsync(session, "Economy Classic");

        string unavailableMessage = await new VoucherRedemptionPage(session.Page)
            .AssertUnavailableVoucherShowsPointsOptionAsync(60_000);

        _output.WriteLine($"Voucher unavailable message: {unavailableMessage}");
        await session.MarkPassedAsync(
            "Single passenger was shown the voucher-unavailable state and a points-payment option.");
    }

    [SkippableFact]
    [Trait("Category", "SearchAndBook-UI-Vouchers")]
    [Trait("WorkItem", "862130")]
    [Trait("tag", "search-and-book-ui-voucher")]
    public async Task CompanionVoucher_RemoveAndReapply_RecalculatesPaymentWithoutTechnicalError()
    {
        EnsureConfiguredOrSkip();
        DateOnly departure = BackendRewardDeparture();
        VoucherLoyaltyAccount account = LoyaltyAccountProvider.GetVoucherAccount(
            validOnOrAfter: departure,
            minimumPoints: 300_000);
        SearchSliceDeepLink search = CreateRewardSearch("JFK", departure);

        await using PlaywrightBrowserSession session = await SignInAsync(search, account);
        await SelectFareAsync(session, "Economy Classic");
        await ReachPaymentAsync(session, departure, adults: 2);

        VoucherRedemptionPage voucherPage = new(session.Page);
        VoucherPaymentSnapshot initiallyApplied = await voucherPage.SelectPaymentOptionAsync("Companion", 90_000);
        BookingPriceSummary priceSummary = new(session.Page);
        PriceSummarySnapshot appliedPrice = await priceSummary.CaptureAsync(30_000);

        VoucherPaymentSnapshot removed = await voucherPage.RemoveSelectedVoucherAsync(60_000);
        PriceSummarySnapshot removedPrice = await priceSummary.WaitForChangeAsync(appliedPrice, 60_000);
        VAA.EAPI.Automation.SearchAndBook.UI.Assertions.VoucherJourneyAssertions.AssertPriceRecalculated(
            appliedPrice,
            removedPrice,
            "removal");

        VoucherPaymentSnapshot reapplied = await voucherPage.SelectPaymentOptionAsync("Companion", 60_000);
        PriceSummarySnapshot reappliedPrice = await priceSummary.WaitForChangeAsync(removedPrice, 60_000);
        VAA.EAPI.Automation.SearchAndBook.UI.Assertions.VoucherJourneyAssertions.AssertPriceRecalculated(
            removedPrice,
            reappliedPrice,
            "selection");

        _output.WriteLine($"Initial voucher state: {initiallyApplied.SectionText}");
        _output.WriteLine($"Removed voucher state: {removed.SectionText}");
        _output.WriteLine($"Reapplied voucher state: {reapplied.SectionText}");
        await session.MarkPassedAsync(
            "Companion voucher removal and re-selection refreshed Payment without a technical error.");
    }

    [SkippableFact]
    [Trait("Category", "SearchAndBook-UI-Vouchers")]
    [Trait("WorkItem", "862130")]
    [Trait("tag", "search-and-book-ui-voucher")]
    public async Task MoreThanTwoVouchers_PaymentDisplaysTwoEarliestExpiryDates()
    {
        EnsureConfiguredOrSkip();
        DateOnly departure = BackendRewardDeparture();
        VoucherLoyaltyAccount account = LoyaltyAccountProvider.GetVoucherAccount(
            validOnOrAfter: departure,
            minimumVoucherCount: 3,
            requireAllVouchersInExpiryRange: true,
            requireDifferentVoucherExpiryDates: true);
        SearchSliceDeepLink search = CreateRewardSearch("JFK", departure);

        await using PlaywrightBrowserSession session = await SignInAsync(search, account);
        await SelectFareAsync(session, "Economy Classic");
        await ReachPaymentAsync(session, departure, adults: 2);

        VoucherPaymentSnapshot payment = await new VoucherRedemptionPage(session.Page)
            .WaitForPaymentOptionsAsync(90_000);
        VAA.EAPI.Automation.SearchAndBook.UI.Assertions.VoucherJourneyAssertions.AssertTwoEarliestExpiriesDisplayed(
            account.VoucherExpiryDates,
            payment.ExpiryDates);

        _output.WriteLine($"Account voucher expiries: {string.Join(", ", account.VoucherExpiryDates)}");
        _output.WriteLine($"Displayed voucher expiries: {string.Join(", ", payment.ExpiryDates)}");
        await session.MarkPassedAsync("Payment displayed the two earliest voucher expiry dates.");
    }

    [SkippableFact]
    [Trait("WorkItem", "862131")]
    [Trait("Category", "SearchAndBook-UI-Voucher-Payment")]
    [Trait("tag", "search-and-book-ui-voucher-payment")]
    public async Task SuccessfulVoucherRedemption_ConfirmationShowsVoucherAndPointsBreakdown()
    {
        EnsureConfiguredOrSkip();
        DateOnly departure = BackendRewardDeparture();
        VoucherLoyaltyAccount account = LoyaltyAccountProvider.GetVoucherAccount(
            validOnOrAfter: departure,
            minimumPoints: 300_000);
        SearchSliceDeepLink search = CreateRewardSearch("JFK", departure);

        await using PlaywrightBrowserSession session = await SignInAsync(search, account);
        await SelectFareAsync(session, "Economy Classic");
        await ReachPaymentAsync(session, departure, adults: 2);

        VoucherRedemptionPage voucherPage = new(session.Page);
        await voucherPage.WaitForPaymentOptionsAsync(90_000);
        PaymentPage paymentPage = new(session.Page);
        await paymentPage.SubmitNonThreeDsCardPaymentAsync(90_000);
        ConfirmationDetails confirmation = await new ConfirmationPage(session.Page)
            .WaitForConfirmationAsync(180_000);
        VAA.EAPI.Automation.SearchAndBook.UI.Assertions.VoucherJourneyAssertions.AssertConfirmationBreakdown(
            confirmation);

        _output.WriteLine(
            $"Voucher benefit: {confirmation.VoucherBenefit}; points used: {confirmation.PointsUsed}; " +
            $"booking reference: {confirmation.BookingReference}.");
        await session.MarkPassedAsync(
            $"Voucher redemption confirmed with booking reference {confirmation.BookingReference}.");
    }

    private static async Task<PlaywrightBrowserSession> SignInAsync(
        SearchSliceDeepLink search,
        VoucherLoyaltyAccount account)
    {
        string baseUrl = ReadRequiredOrSkip("SAB_BASE_URL");
        string password = ReadRequiredOrSkip("LoyaltyAccount");
        PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(
            PlaywrightBrowserOptions.FromEnvironment(baseUrl));
        await new FlyingClubB2CLoginPage(session.Page).SignInAsync(
            search.ToRelativeUrl(),
            account.FlyingClubNumber,
            password,
            90_000);
        await VirginAtlanticConsent.DismissPrivacyBannerIfPresentAsync(session.Page);
        return session;
    }

    private static async Task SelectFareAsync(
        PlaywrightBrowserSession session,
        string fare,
        int expectedSlices = 1)
    {
        FlightResultsPage results = new(session.Page);
        await results.WaitUntilLoadedAsync(90_000);
        await new VoucherRedemptionPage(session.Page)
            .AcceptFlightSelectionGuidanceIfShownAsync();
        await results.SelectVirginAtlanticFareAndWaitForSummaryAsync(
            fare,
            120_000,
            expectedSlices);
    }

    private static async Task ReachPaymentAsync(
        PlaywrightBrowserSession session,
        DateOnly departure,
        int adults)
    {
        VoucherRedemptionPage voucherPage = new(session.Page);
        await voucherPage.AssertSummaryGuidanceAndAcceptAsync(30_000);
        await voucherPage.WaitForOptionsAsync(60_000);
        await new FlightSummaryPage(session.Page).ContinueToPassengerDetailsAsync(90_000);
        PassengerDetailsPage passengers = new(session.Page);
        await passengers.FillGuestPassengersAsync(
            adults,
            teens: 0,
            children: 0,
            infants: 0,
            departureDate: departure,
            acceptInsurance: false,
            preserveLeadPassenger: true,
            timeoutMs: 120_000);
        await passengers.SubmitAndWaitForProgressAsync(90_000);
        await new SeatSelectionPage(session.Page).SkipSeatSelectionIfShownAsync(120_000);
        await new PaymentPage(session.Page).WaitUntilLoadedAsync(120_000);
    }

    private static SearchSliceDeepLink CreateRewardSearch(string destination, DateOnly departure)
    {
        return new SearchSliceDeepLink
        {
            Origin = "LHR",
            Destination = destination,
            DepartureDate = departure,
            Adults = 2,
            AwardSearch = true,
            Cta = "AbTest_SP_Flights"
        };
    }

    private static DateOnly FutureDeparture() =>
        DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(4));

    private static DateOnly BackendRewardDeparture()
    {
        const string variableName = "SAB_VOUCHER_DEPARTURE_DATE";
        string? configuredDate = Environment.GetEnvironmentVariable(variableName);
        if (!configuredDate.IsNullOrWhiteSpace())
        {
            if (DateOnly.TryParseExact(configuredDate, "yyyy-MM-dd", out DateOnly parsedDate))
            {
                return parsedDate;
            }

            throw new InvalidOperationException(
                $"{variableName} must use the yyyy-MM-dd format. Received '{configuredDate}'.");
        }

        DateTime targetMonth = DateTime.UtcNow.Date.AddMonths(6);
        int targetDay = Math.Min(14, DateTime.DaysInMonth(targetMonth.Year, targetMonth.Month));
        return new DateOnly(targetMonth.Year, targetMonth.Month, targetDay);
    }

    private static string ReadRequiredOrSkip(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        Skip.If(value.IsNullOrWhiteSpace(), $"Set {name} to run voucher UI journeys.");
        return value!;
    }

    private static void EnsureConfiguredOrSkip()
    {
        string baseUrl = ReadRequiredOrSkip("SAB_BASE_URL");
        bool validUrl = Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? uri);
        bool isTestEnvironment = validUrl &&
            System.Text.RegularExpressions.Regex.IsMatch(
                uri!.Host,
                @"^vaa-test4\.virginatlantic\.com$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        Skip.If(
            !isTestEnvironment,
            "Voucher frontend journeys are enabled only on vaa-test4 until the feature is deployed elsewhere.");
    }
}
