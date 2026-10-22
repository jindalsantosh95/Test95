using System.Text.RegularExpressions;
using OpenQA.Selenium.Appium;
using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Flows;

public sealed class NativeBookingCheckoutFlow
{
    private readonly NativeBookingPage _bookingPage;
    private readonly NativeHostedPaymentPage _paymentPage;
    private readonly TripsPage _tripsPage;
    private readonly NativePayPalPage _payPalPage;

    public NativeBookingCheckoutFlow(AppiumDriver driver)
    {
        _bookingPage = new NativeBookingPage(driver);
        _paymentPage = new NativeHostedPaymentPage(driver);
        _tripsPage = new TripsPage(driver);
        _payPalPage = new NativePayPalPage(driver);
    }

    public CompletedNativeBooking Complete(PassengerCounts passengers)
        => Complete(passengers, isAuthenticatedMember: false);

    public CompletedNativeBooking CompleteWithSeatForOneAdult()
        => Complete(PassengerCounts.OneAdult, isAuthenticatedMember: false, requireSeatSelection: true);

    public CompletedNativeBooking CompleteWithCardPresentationForOneAdult()
        => Complete(PassengerCounts.OneAdult, isAuthenticatedMember: false, requireCardPresentation: true);

    public void SubmitTestPaymentForControlledPostHppFailure(Action verifyActiveGuard)
    {
        IReadOnlyList<TravellerDetails> travellers = CreateTravellers(PassengerCounts.OneAdult);
        CompletePassengerDetails(PassengerCounts.OneAdult, travellers, isAuthenticatedMember: false);
        verifyActiveGuard();
        SubmitTestPayment(travellers[0], verifyActiveGuard);
        // No confirmation wait or retry here: the caller must prove the injected post-HPP
        // fault and assert the error state, not treat an expected error as a happy-path failure.
    }

    private void SubmitTestPayment(TravellerDetails traveller, Action? beforePaymentSubmission = null)
    {
        _bookingPage.ContinueToSecurePayment();
        _paymentPage.Pay(new PaymentCardDetails(
            $"{traveller.FirstName} {traveller.LastName}",
            "4000000000002719", "12/30", "123", "1 Test Street", "Crawley",
            "United Kingdom", "RH10 1AA"), beforePaymentSubmission);
    }

    public CompletedNativeBooking CompleteForMember(PassengerCounts passengers, LoyaltyAccount member)
        => Complete(passengers, isAuthenticatedMember: true,
            memberName: $"{member.FirstName} {member.LastName}");

    public void PrepareGuestPassengerDetailsWithoutAcceptingTerms(PassengerCounts passengers)
    {
        _bookingPage.ContinueFromSummary();
        FillGuestPassengerDetails(CreateTravellers(passengers));
        // Deliberately stop before Terms, passenger submission, HPP or payment.
    }

    public void VerifyGuestReviewBackNavigation()
    {
        string summaryTotal = _bookingPage.ReadSummaryTotalBeforeCheckout();
        CompletePassengerDetails(PassengerCounts.OneAdult,
            CreateTravellers(PassengerCounts.OneAdult), isAuthenticatedMember: false);
        IReadOnlyList<string> originalFigures = _bookingPage.VerifyReviewTotal(summaryTotal, isMember: false, allowSeats: false);
        _bookingPage.ReturnFromReviewToPassengerDetails();
        // Do not refill or resave passengers: re-entry must work with retained form state.
        _bookingPage.ContinueFromPassengerDetails(requireSeatBypass: true);
        IReadOnlyList<string> retainedFigures = _bookingPage.VerifyReviewTotal(summaryTotal, isMember: false, allowSeats: false);
        ReviewTotalAssertions.VerifyRetainedFigures(originalFigures, retainedFigures);
        // Stop before HPP. This proves visible readiness, not a new backend session identifier.
    }

    public CompletedNativeBooking CompleteWithPayPalForOneAdult(
        VAA.EAPI.Automation.Core.TestDataConfig.APMPayment.APMDetails account, Action<string>? reportProgress = null)
        => Complete(PassengerCounts.OneAdult, isAuthenticatedMember: false, payPalAccount: account, reportProgress: reportProgress);

    public void OpenSeatSelectionForOneGuestAdult()
        => OpenSeatSelectionForGuests(PassengerCounts.OneAdult);

    public void VerifyGuestSeatBypassToPayment(PassengerCounts passengers)
    {
        PrepareGuestPassengerDetailsWithoutAcceptingTerms(passengers);
        _bookingPage.AcceptBookingTerms();
        _bookingPage.ContinueFromPassengerDetails(requireSeatBypass: true);
    }

    public void OpenSeatSelectionForGuests(PassengerCounts passengers)
    {
        PrepareGuestPassengerDetailsWithoutAcceptingTerms(passengers);
        _bookingPage.AcceptBookingTerms();
        _bookingPage.ContinueFromPassengerDetails(stopAtSeatSelection: true);
    }

    private void FillGuestPassengerDetails(IReadOnlyList<TravellerDetails> travellers)
    {
        CompleteGuestTravellers(travellers.Count, index =>
        {
            TravellerDetails traveller = travellers[index];
            _bookingPage.EnterTravellerName(index, traveller.FirstName, traveller.LastName);
            _bookingPage.SelectDefaultTravellerDateOfBirth(index);
            _bookingPage.SelectTravellerGender(index, traveller.Gender);
        }, () => _bookingPage.ContactDetailsRequiredBeforeTravellerSave,
            () => _bookingPage.EnterContactDetails(
                $"{travellers[0].FirstName}.{travellers[0].LastName}@example.com".ToLowerInvariant(),
                "+44", "7700900123"),
            _bookingPage.SaveTravellerAndContinue);
    }

    public SelectedPayWithPointsOption SelectPayWithPointsOptionForMember(PassengerCounts passengers)
    {
        IReadOnlyList<TravellerDetails> travellers = CreateTravellers(passengers);
        CompletePassengerDetails(passengers, travellers, isAuthenticatedMember: true);
        return _bookingPage.SelectFirstPayWithPointsOption();
    }

    private CompletedNativeBooking Complete(
        PassengerCounts passengers,
        bool isAuthenticatedMember,
        bool requireSeatSelection = false,
        string? memberName = null,
        bool requireCardPresentation = false,
        VAA.EAPI.Automation.Core.TestDataConfig.APMPayment.APMDetails? payPalAccount = null,
        Action<string>? reportProgress = null)
    {
        string summaryTotal = _bookingPage.ReadSummaryTotalBeforeCheckout();
        IReadOnlyList<TravellerDetails> travellers = CreateTravellers(passengers);
        CompletePassengerDetails(passengers, travellers, isAuthenticatedMember, requireSeatSelection);

        _bookingPage.VerifyReviewTotal(summaryTotal, isAuthenticatedMember, requireSeatSelection);
        reportProgress?.Invoke("Review figures and readiness verified.");

        if (payPalAccount is null)
        {
            SubmitTestPayment(travellers[0]);
        }
        else
        {
            reportProgress?.Invoke("Opening HPP from Review.");
            _bookingPage.ContinueToSecurePayment();
            reportProgress?.Invoke("HPP handoff completed; opening PayPal.");
            _payPalPage.SignInAndApprove(payPalAccount, reportProgress);
        }

        if (requireCardPresentation)
        {
            _bookingPage.VerifyAndAcceptCardPresentation();
        }

        BookingConfirmationSummary confirmation = _bookingPage.ReadConfirmation();
        string[] expectedNames = isAuthenticatedMember
            ? [memberName ?? string.Empty]
            : travellers.Select(traveller => $"{traveller.FirstName} {traveller.LastName}").ToArray();
        BookingConfirmationAssertions.AssertTravellers(confirmation.TravellerNamesEvidence, expectedNames);
        string bookingReference = ExtractBookingReference(confirmation.BookingReferenceText);
        _bookingPage.ReturnToBookingHome();
        _tripsPage.Open();
        if (!_tripsPage.ContainsBookingReference(bookingReference))
        {
            throw new InvalidOperationException(
                $"Completed booking '{bookingReference}' was not displayed in My Trips.");
        }

        return new CompletedNativeBooking(bookingReference, confirmation, travellers.Count);
    }

    private void CompletePassengerDetails(
        PassengerCounts passengers,
        IReadOnlyList<TravellerDetails> travellers,
        bool isAuthenticatedMember,
        bool requireSeatSelection = false)
    {
        _bookingPage.ContinueFromSummary();
        if (!isAuthenticatedMember)
        {
            FillGuestPassengerDetails(travellers);

            _bookingPage.AcceptBookingTerms();
        }
        else
        {
            if (passengers != PassengerCounts.OneAdult)
            {
                throw new InvalidOperationException(
                    "Authenticated prepopulated checkout currently supports exactly one adult.");
            }

            // Login has already proved the leased member identity. For one adult travelling as
            // themselves, the app owns all prepopulated traveller/contact values; traversing those
            // controls adds no booking assertion and makes the native page unnecessarily slow.
            _bookingPage.AcceptBookingTerms();
        }

        _bookingPage.ContinueFromPassengerDetails(requireSeatSelection);
    }

    internal static void CompleteGuestTravellers(
        int count,
        Action<int> enterTraveller,
        Func<bool> contactBeforeSave,
        Action enterContact,
        Action<bool> saveTraveller)
    {
        bool contactEntered = false;
        for (int index = 0; index < count; index++)
        {
            enterTraveller(index);
            if (index == 0 && contactBeforeSave())
            {
                enterContact();
                contactEntered = true;
            }
            saveTraveller(index == count - 1);
        }

        // The legacy form shares a single contact section below all traveller cards.
        // The redesigned editor requires contact data before saving traveller zero.
        if (!contactEntered)
        {
            enterContact();
        }
    }

    private static IReadOnlyList<TravellerDetails> CreateTravellers(PassengerCounts passengers)
    {
        List<TravellerDetails> travellers = [];
        AddTravellers(travellers, passengers.Adults, "Adult");
        AddTravellers(travellers, passengers.Teens, "Teen");
        AddTravellers(travellers, passengers.Children, "Child");
        AddTravellers(travellers, passengers.Infants, "Infant");
        return travellers;
    }

    private static void AddTravellers(List<TravellerDetails> travellers, int count, string type)
    {
        string[] ordinalNames = ["One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine"];
        for (int number = 1; number <= count; number++)
        {
            travellers.Add(new TravellerDetails($"{type}{ordinalNames[number - 1]}", "Automation", "Male"));
        }
    }

    private static string ExtractBookingReference(string confirmationText)
    {
        Match match = Regex.Match(confirmationText, @"\b[A-Z0-9]{6}\b", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            throw new InvalidOperationException(
                $"A six-character booking reference was not present in '{confirmationText}'.");
        }

        return match.Value.ToUpperInvariant();
    }

    private sealed record TravellerDetails(string FirstName, string LastName, string Gender);
}

public sealed record CompletedNativeBooking(
    string BookingReference,
    BookingConfirmationSummary Confirmation,
    int PassengerCount);
