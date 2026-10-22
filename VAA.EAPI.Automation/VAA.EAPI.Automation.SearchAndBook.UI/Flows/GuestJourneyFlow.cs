using VAA.EAPI.Automation.SearchAndBook.UI.Assertions;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.SearchAndBook.UI.Pages;
using VAA.EAPI.Automation.SearchAndBook.UI.Support;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Flows;

public sealed class GuestJourneyFlow
{
    private readonly PlaywrightBrowserOptions _options;

    public GuestJourneyFlow(PlaywrightBrowserOptions options)
    {
        _options = options;
    }

    public async Task<GuestJourneyResult> RunAsync(GuestJourneyScenario scenario)
    {
        if (scenario.StopAtHostedPaymentPage && scenario.Payment == PaymentJourneyMode.CardholderOffer)
        {
            throw new InvalidOperationException(
                "StopAtHostedPaymentPage cannot be combined with CardholderOffer because that journey opens " +
                "an external credit-card application.");
        }

        SearchSliceDeepLink search = BuildSearch(scenario);
        PlaywrightBrowserOptions options = WithSessionName(_options, scenario.Name);

        LoyaltyAccount? loyaltyAccount = null;
        if (scenario.Authenticated)
        {
            if (scenario.LoyaltyPassword.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("An authenticated journey requires LoyaltyAccount.");
            }

            loyaltyAccount = LoyaltyAccountProvider.GetAdultAccount(
                "Red",
                scenario.MinimumLoyaltyPoints,
                scenario.MaximumLoyaltyPoints);
        }

        await using PlaywrightBrowserSession session = await PlaywrightBrowserSession.StartAsync(options);
        try
        {
            ApplicationPage applicationPage = new(session.Page);
            session.SetCurrentAction(scenario.Authenticated
                ? "signing in with the selected Flying Club account"
                : "opening the flight search results",
                scenario.Name);
            if (scenario.Authenticated)
            {
                FlyingClubB2CLoginPage loginPage = new(session.Page);
                if (scenario.AwardSearch)
                {
                    await loginPage.SignInAsync(
                        search.ToRelativeUrl(),
                        loyaltyAccount.FlyingClubNumber,
                        scenario.LoyaltyPassword,
                        90_000);
                    await applicationPage.DismissPrivacyBannerIfPresentAsync();
                }
                else
                {
                    await loginPage.SignInFromHomeAsync(
                        options.BaseUrl.ToString(),
                        loyaltyAccount.FlyingClubNumber,
                        scenario.LoyaltyPassword,
                        90_000);
                    await applicationPage.OpenAsync(
                        options.BaseUrl,
                        search.ToRelativeUrl(),
                        options.NavigationTimeoutMs);
                }
            }
            else
            {
                await applicationPage.OpenAsync(
                    options.BaseUrl,
                    search.ToRelativeUrl(),
                    options.NavigationTimeoutMs);
            }

            session.SetCurrentAction("waiting for flight results and selecting the requested carrier, cabin and fare");
            FlightResultsPage resultsPage = new(session.Page);
            await resultsPage.WaitUntilLoadedAsync(90_000);
            int expectedSlices = scenario.IsReturn ? 2 : 1;
            string selectedFare;
            if (scenario.SelectAnyAvailableFare && !scenario.CarrierCode.IsNullOrWhiteSpace())
            {
                selectedFare = await resultsPage.SelectAnyAvailableFareOnCarrierAndWaitForSummaryAsync(
                    scenario.CarrierCode,
                    120_000,
                    expectedSlices);
            }
            else if (scenario.SelectAnyAvailableFare)
            {
                selectedFare = await resultsPage.SelectAnyAvailableFareAndWaitForSummaryAsync(
                    120_000,
                    expectedSlices);
            }
            else
            {
                selectedFare = await resultsPage.SelectVirginAtlanticFareAndWaitForSummaryAsync(
                    scenario.FareName,
                    120_000,
                    expectedSlices);
            }

            session.SetCurrentAction("validating the selected itinerary on the flight summary page");
            FlightSummaryPage summaryPage = new(session.Page);
            await summaryPage.WaitUntilReadyAsync(90_000);
            await summaryPage.AssertJourneySlicesAsync(search.Origin, search.Destination, scenario.IsReturn);
            BookingPriceSummary priceSummary = new(session.Page);
            PriceSummarySnapshot initialPrice = await priceSummary.CaptureAsync(30_000);
            BookingPriceAssertions.AssertValid(initialPrice, "Flight summary");
            await summaryPage.ContinueToPassengerDetailsAsync(90_000);

            session.SetCurrentAction("loading and completing the required passenger details");
            PassengerDetailsPage passengerPage = new(session.Page);
            await passengerPage.WaitUntilReadyAsync(90_000);
            PriceSummarySnapshot passengerPrice = await priceSummary.CaptureAsync(30_000);
            BookingPriceAssertions.AssertConsistent(initialPrice, passengerPrice, "Passenger details");

            if (loyaltyAccount is not null)
            {
                await passengerPage.AssertLeadPassengerMatchesAsync(
                    loyaltyAccount.FirstName,
                    loyaltyAccount.LastName,
                    loyaltyAccount.FlyingClubNumber);
            }
            await passengerPage.FillGuestPassengersAsync(
                scenario.Adults,
                scenario.Teens,
                scenario.Children,
                scenario.Infants,
                search.DepartureDate,
                scenario.AcceptInsurance,
                loyaltyAccount is not null,
                120_000);

            session.SetCurrentAction("submitting passenger details and handling seat selection");
            SeatSelectionPage seatPage = new(session.Page);
            IReadOnlyList<string> selectedSeats;
            IReadOnlyList<string> skippedSeatSelections;
            if (scenario.SeatSelection == SeatSelectionMode.Skip)
            {
                await passengerPage.SubmitAndWaitForProgressAsync(90_000);
                await seatPage.SkipSeatSelectionIfShownAsync(120_000);
                selectedSeats = [];
                skippedSeatSelections = ["Skipped by scenario"];
            }
            else
            {
                await passengerPage.SubmitForSeatSelectionAsync(90_000);
                SeatSelectionResult seatSelection = await seatPage.SelectSeatsAndContinueToPaymentAsync(
                    scenario.SeatSelection == SeatSelectionMode.SelectPaid,
                    120_000);
                selectedSeats = seatSelection.SelectedSeats;
                skippedSeatSelections = seatSelection.SkippedReasons;
                foreach (string skippedReason in skippedSeatSelections)
                {
                    Console.WriteLine($"[seat-selection-skipped] {skippedReason}");
                }
            }

            if (scenario.RequireSeatMap && skippedSeatSelections.Any(
                    reason => reason.Contains("seat map unavailable", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    "The required seat-selection experience was unavailable. " +
                    string.Join("; ", skippedSeatSelections));
            }

            if (scenario.RequireSeatMap &&
                selectedSeats.Count == 0 &&
                !skippedSeatSelections.Any(reason =>
                    reason.Contains("no selectable seat available", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    "The required seat map exposed an actionable seat-selection step but no seat outcome was observed.");
            }

            session.SetCurrentAction("loading and validating the payment page");
            PaymentPage paymentPage = new(session.Page);
            await paymentPage.WaitUntilLoadedAsync(120_000);
            PriceSummarySnapshot paymentPrice = await priceSummary.CaptureAsync(30_000);
            GuestJourneyAssertions.AssertPaymentPrice(
                initialPrice,
                paymentPrice,
                scenario.SeatSelection,
                scenario.AcceptInsurance,
                selectedSeats);
            GuestJourneyAssertions.AssertExpectedCurrency(paymentPrice, scenario.ExpectedCurrency);

            string paymentOutcome;
            string? paymentReferenceId = null;
            ConfirmationDetails? confirmation = null;
            if (scenario.Payment == PaymentJourneyMode.CardholderOffer)
            {
                session.SetCurrentAction("opening the US cardholder offer application");
                paymentOutcome = await paymentPage.OpenCardholderOfferApplicationAsync(30_000);
            }
            else
            {
                string pointsOutcome = scenario.Payment == PaymentJourneyMode.PartialPayWithPoints
                    ? await ApplyPayWithPointsAsync(paymentPage, session)
                    : string.Empty;
                session.SetCurrentAction("opening the hosted card-payment form");
                if (scenario.StopAtHostedPaymentPage && selectedSeats.Count == 0)
                {
                    session.SetCurrentAction("validating PayPal is available for the booking without an added seat");
                    await paymentPage.AssertPayPalVisibleAsync(30_000);
                }
                await paymentPage.OpenCardPaymentAsync(90_000);
                string paymentSummary = await paymentPage.GetPageSummaryAsync();
                if (!paymentSummary.Contains("paymentFrames: 1", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"HPP payment frame was not ready. {paymentSummary}");
                }

                paymentOutcome = pointsOutcome.IsNullOrWhiteSpace()
                    ? "HPP card form is interactable."
                    : $"{pointsOutcome} Residual HPP card form is interactable.";

                if (ShouldCompleteCardPayment(scenario, options))
                {
                    session.SetCurrentAction("entering card details, submitting payment and waiting for booking confirmation");
                    paymentReferenceId = await paymentPage.SubmitNonThreeDsCardPaymentAsync(90_000);
                    confirmation = await new ConfirmationPage(session.Page).WaitForConfirmationAsync(
                        180_000,
                        scenario.IsReturn ? 2 : 1);
                    Console.WriteLine(
                        $"[booking-created] PNR={confirmation.BookingReference}; " +
                        $"scenario={scenario.Name}");
                    session.SetCurrentAction("validating the created booking confirmation details");
                    GuestJourneyAssertions.AssertConfirmation(
                        scenario,
                        loyaltyAccount,
                        selectedFare,
                        selectedSeats,
                        confirmation);
                    paymentOutcome = $"Booking confirmed with PNR {confirmation.BookingReference}.";
                    Console.WriteLine(
                        $"[confirmed-booking] PNR={confirmation.BookingReference}; " +
                        $"surname={confirmation.Surname}; " +
                        $"paymentReferenceId={paymentReferenceId ?? "Not captured"}; scenario={scenario.Name}");
                }
            }

            if (!scenario.SelectAnyAvailableFare &&
                !selectedFare.Contains(scenario.FareName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Expected to select '{scenario.FareName}', but selection diagnostics were '{selectedFare}'.");
            }

            string screenshotPath = await session.SaveScreenshotAsync(scenario.Name);
            string passedReason = confirmation is not null
                ? $"Booking confirmed: {scenario.Name}. PNR: {confirmation.BookingReference}"
                : IsAuthenticatedStagingJourney(scenario, options)
                    ? $"Stopped at HPP without submitting payment because authenticated booking " +
                      $"confirmation is restricted in STG (UAT-backed): {scenario.Name}"
                    : $"Reached expected endpoint: {scenario.Name}";
            await session.MarkPassedAsync(passedReason);
            return new GuestJourneyResult(
                search,
                selectedFare,
                initialPrice,
                paymentPrice,
                selectedSeats,
                skippedSeatSelections,
                paymentOutcome,
                paymentReferenceId,
                screenshotPath,
                confirmation);
        }
        catch (Exception exception)
        {
            string failureReason = await session.MarkFailedAsync(exception);
            throw new InvalidOperationException(failureReason, exception);
        }
    }

    private static async Task<string> ApplyPayWithPointsAsync(
        PaymentPage paymentPage,
        PlaywrightBrowserSession session)
    {
        session.SetCurrentAction("applying Pay with Points before residual card payment");
        return await paymentPage.ApplyPayWithPointsAsync(30_000);
    }

    private static bool ShouldCompleteCardPayment(
        GuestJourneyScenario scenario,
        PlaywrightBrowserOptions options) =>
        !scenario.StopAtHostedPaymentPage &&
        scenario.Payment is PaymentJourneyMode.Card or PaymentJourneyMode.PartialPayWithPoints &&
        !IsAuthenticatedStagingJourney(scenario, options);

    private static bool IsAuthenticatedStagingJourney(
        GuestJourneyScenario scenario,
        PlaywrightBrowserOptions options) =>
        scenario.Authenticated && options.BaseUrl.Host.Is("stg.virginatlantic.com");

    private static SearchSliceDeepLink BuildSearch(GuestJourneyScenario scenario)
    {
        int departureOffset = Random.Shared.Next(3, 8);
        DateOnly departure = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(departureOffset));
        DateOnly? returning = scenario.IsReturn
            ? departure.AddDays(Random.Shared.Next(7, 29))
            : null;

        return new SearchSliceDeepLink
        {
            Origin = scenario.Origin,
            Destination = scenario.Destination,
            DepartureDate = departure,
            ReturnDate = returning,
            Adults = scenario.Adults,
            Teens = scenario.Teens,
            Children = scenario.Children,
            Infants = scenario.Infants,
            AwardSearch = scenario.AwardSearch,
            Cta = scenario.AwardSearch ? "AbTest_SP_Flights" : null
        };
    }

    private static PlaywrightBrowserOptions WithSessionName(
        PlaywrightBrowserOptions options,
        string scenarioName)
    {
        if (options.BrowserStack is null)
        {
            return options;
        }

        return options with
        {
            BrowserStack = options.BrowserStack with
            {
                SessionName = $"{options.BrowserStack.SessionName} | {scenarioName}"
            }
        };
    }

}
