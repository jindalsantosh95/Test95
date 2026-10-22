namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

/// <summary>
/// App-owned native booking identifiers. Redesigned passenger IDs are aligned with
/// vaa-mobile-ios commit 9d8d3b2d (PassengerDetailsHub/Editor, without OptionOne).
/// Repeated controls use their zero-based displayed order and never generated UUIDs.
/// </summary>
internal static class NativeBookingAccessibilityIds
{
    public const string SummaryTitle = "NativeFlightSummaryTitle";
    public const string SummaryItinerary = "NativeFlightSummaryItinerary0";
    public const string SummaryContinueButton = "NativeFlightSummaryContinueButton";
    public const string PassengerDetailsTitle = "NativeFlightPassengerDetailsTitle";
    public const string PassengerDetailsHub = "PassengerDetailsHub";
    public const string RedesignedPassengerEditorHeader = "PassengerDetailsModalHeader";
    public const string RedesignedPassengerEditorScrollView = "PassengerDetailsEditorScrollView";
    public const string RedesignedPassengerSaveAndContinue = "PassengerDetailsSaveAndContinue";
    public const string RedesignedPassengerSaveValidation = "PassengerDetailsSaveValidationButton";
    public const string RedesignedPassengerDetailsContinueButton = "PassengerDetailsContinue";
    public const string RedesignedBookingTermsToggle = "PassengerDetailsTermsAcknowledgement";
    public const string RedesignedFirstPassengerNameError = "PassengerDetailsPassenger1FirstNameError";
    public const string RedesignedDateOfBirthPicker = "PassengerDetailsDateOfBirthPicker";
    public const string PassengerDetailsScrollView = "NativeFlightPassengerDetailsScrollView";
    public const string PassengerDetailsSubmissionError = "NativeFlightPassengerDetailsSubmissionError";
    public const string RedesignedPassengerDetailsSubmissionError = "PassengerDetailsSubmissionError";
    public const string PassengerDetailsContinueButton = "NativeFlightPassengerDetailsContinueButton";
    public const string ContactEmailText = "NativeFlightContactEmailText";
    public const string ContactCountryCodeText = "NativeFlightContactCountryCodeText";
    public const string ContactTelephoneText = "NativeFlightContactTelephoneText";
    public const string RedesignedContactEmailText = "PassengerDetailsContactEmailText";
    public const string RedesignedContactCountryCode = "PassengerDetailsContactCountryCode";
    public const string RedesignedContactTelephoneText = "PassengerDetailsContactTelephoneText";
    public const string UnitedKingdomCountryOption = "United Kingdom";
    public const string MemberSelector = "PassengerDetailsMemberSelector";
    public const string MemberSelfOption = "PassengerDetailsMemberSelfOption";
    public const string MemberNotTravellingOption = "PassengerDetailsMemberNotTravellingOption";
    public const string BookingTermsToggle = "NativeFlightBookingTermsToggle";
    public const string SeatSelectionTitle = "seatSelection.navigationHeader";
    public const string SeatPrefix = "seatSelection.seat.";
    public const string SeatSelectionConfirm = "seatSelection.selectSeat";
    public const string SeatSelectionDetailSheet = "seatSelection.detailSheet";
    public const string SeatSelectionDetailClose = "seatSelection.detail.close";
    public const string SeatSelectionLegend = "seatSelection.legend";
    public const string SeatSelectionLegendClose = "seatSelection.legend.close";
    public const string SeatSelectionError = "seatSelection.error";
    public const string SeatSelectionRetry = "seatSelection.retry";
    public const string SeatSelectionLoading = "seatSelection.loading";
    public const string SeatSelectionUnavailable = "seatSelection.unavailable";
    public const string SeatSelectionSubmissionError = "seatSelection.submissionError";
    public const string SeatSelectionPassengerPicker = "NativeFlightSeatSelectionPassengerPicker";
    public const string SeatSelectionContinueButton = "seatSelection.continue";
    public const string SeatSelectionChildGuidanceSheet = "seatSelection.childGuidanceSheet";
    public const string PaymentHandoffTitle = "NativeFlightPaymentHeading";
    public const string PaymentContinueButton = "NativeFlightContinueToPaymentButton";
    public const string PaymentRetryButton = "NativeFlightPaymentRetryButton";
    public const string PaymentHandoffError = "NativeFlightPaymentHandoffError";
    public const string PaymentErrorPage = "NativeFlightPaymentErrorPage";
    public const string PaymentErrorTitle = "NativeFlightPaymentErrorTitle";
    public const string PaymentErrorMessage = "NativeFlightPaymentErrorMessage";
    public const string PaymentErrorReturnToSearch = "NativeFlightPaymentErrorReturnToSearch";
    public const string PaymentSummaryCard = "NativeFlightPaymentSummaryCard";
    public const string PaymentFareConditionsLink = "NativeFlightPaymentFareConditionsLink";
    public const string PaymentGuidance = "NativeFlightPaymentGuidance";
    public const string PaymentBackButton = "NativeFlightPaymentBackButton";
    public const string CardPresentationPage = "NativeFlightCardPresentationCheckPage";
    public const string CardPresentationHeading = "NativeFlightCardPresentationCheckHeading";
    public const string CardPresentationIntroduction = "NativeFlightCardPresentationCheckIntroduction";
    public const string CardPresentationDetails = "NativeFlightCardPresentationCheckDetails";
    public const string CardPresentationCancel = "NativeFlightCardPresentationCheckCancel";
    public const string CardPresentationAgree = "NativeFlightCardPresentationCheckAgree";
    public const string PayWithPointsCard = "NativeFlightPayWithPointsCard";
    public const string PayWithPointsOptionSelector = "NativeFlightPayWithPointsOptionSelector";
    public const string PayWithPointsOptionPrefix = "NativeFlightPayWithPointsOption";
    public const string PayWithPointsApplyButton = "NativeFlightPayWithPointsApplyButton";
    public const string PayWithPointsApplied = "NativeFlightPayWithPointsApplied";
    public const string PayWithPointsAmountToPay = "NativeFlightPayWithPointsAmountToPay";
    public const string PayWithPointsRemoveButton = "NativeFlightPayWithPointsRemoveButton";
    public const string PayWithPointsLoading = "NativeFlightPayWithPointsLoading";
    public const string PayWithPointsInsufficientBalance = "NativeFlightPayWithPointsInsufficientBalance";
    public const string PayWithPointsOptionsUnavailable = "NativeFlightPayWithPointsOptionsUnavailable";
    public const string ConfirmationTitle = "NativeFlightBookingConfirmationTitle";
    public const string ConfirmationBookingReference = "NativeFlightBookingConfirmationBookingReference";
    public const string ConfirmationPassengerCount = "NativeFlightBookingConfirmationPassengerCount";
    public const string ConfirmationTotalPrice = "NativeFlightBookingConfirmationTotalPrice";
    public const string ConfirmationReturnHomeButton = "NativeFlightBookingConfirmationReturnHomeButton";

    public static string TravellerCard(int travellerIndex) => $"NativeFlightTraveller{travellerIndex}Card";
    public static string RedesignedTravellerCard(int travellerIndex) =>
        $"PassengerDetailsPassengerCard{travellerIndex + 1}";
    public static string RedesignedTravellerFieldPrefix(int travellerIndex) =>
        $"PassengerDetailsPassenger{travellerIndex + 1}";
    public static string RedesignedTravellerFirstNameText(int travellerIndex) =>
        $"{RedesignedTravellerFieldPrefix(travellerIndex)}FirstNameText";
    public static string RedesignedTravellerLastNameText(int travellerIndex) =>
        $"{RedesignedTravellerFieldPrefix(travellerIndex)}SurnameText";
    public static string RedesignedTravellerDateOfBirth(int travellerIndex) =>
        $"{RedesignedTravellerFieldPrefix(travellerIndex)}DateOfBirth";
    public static string RedesignedTravellerDisplayedDateOfBirthOption(int travellerIndex) =>
        $"{RedesignedTravellerFieldPrefix(travellerIndex)}DateOfBirthDisplayedOption";
    public static string RedesignedTravellerGender(int travellerIndex) =>
        $"{RedesignedTravellerFieldPrefix(travellerIndex)}Gender";
    public static string RedesignedTravellerMembershipNumberText(int travellerIndex) =>
        $"{RedesignedTravellerFieldPrefix(travellerIndex)}MembershipNumberText";
    public static string TravellerFirstNameText(int travellerIndex) => $"NativeFlightTraveller{travellerIndex}FirstNameText";
    public static string TravellerLastNameText(int travellerIndex) => $"NativeFlightTraveller{travellerIndex}LastNameText";
    public static string TravellerMembershipNumberText(int travellerIndex) =>
        $"NativeFlightTraveller{travellerIndex}MembershipNumberText";
    public static string TravellerDateOfBirthButton(int travellerIndex) => $"NativeFlightTraveller{travellerIndex}DateOfBirthButton";
    public static string TravellerDateOfBirthPicker(int travellerIndex) => $"NativeFlightTraveller{travellerIndex}DateOfBirthPicker";
    public static string TravellerGenderSelector(int travellerIndex) =>
        $"NativeFlightTraveller{travellerIndex}GenderSelector";
    public static string TravellerGenderOption(int travellerIndex, string gender) =>
        $"NativeFlightTraveller{travellerIndex}Gender{gender}Option";
    public static string LegacyTravellerGenderButton(int travellerIndex, string gender) =>
        $"NativeFlightTraveller{travellerIndex}Gender{gender}Button";
    public const string SeatSelectionPrice = "seatSelection.seatPrice";
    public const string SeatSelectionViewport = "seatSelection.viewport";
    public const string SeatSelectionPassengers = "seatSelection.passengers";
    public const string SeatSelectionTripTotal = "seatSelection.tripTotal";
    public const string SeatSelectionDetailPrice = "seatSelection.detail.price";
    public static string SeatPassengerOption(int travellerIndex) => $"seatSelection.passenger.{travellerIndex}";
    public static string Seat(int sectorIndex, int travellerIndex, string seatNumber) =>
        $"NativeFlightSeat_{sectorIndex}_{travellerIndex}_{seatNumber}";
    public static string PayWithPointsOption(int optionIndex) => $"{PayWithPointsOptionPrefix}{optionIndex}";
}
