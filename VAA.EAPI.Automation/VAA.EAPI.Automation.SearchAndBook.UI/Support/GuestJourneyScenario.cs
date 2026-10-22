using VAA.EAPI.Automation.SearchAndBook.UI.Pages;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Support;

public enum SeatSelectionMode
{
    Skip,
    SelectAvailable,
    SelectPaid
}

public enum PaymentJourneyMode
{
    Card,
    PartialPayWithPoints,
    CardholderOffer
}

public sealed record GuestJourneyScenario
{
    public required string Name { get; init; }

    public string Origin { get; init; } = "LHR";

    public string Destination { get; init; } = "JFK";

    public required string FareName { get; init; }

    public bool SelectAnyAvailableFare { get; init; }

    public string? CarrierCode { get; init; }

    public bool IsReturn { get; init; }

    public SeatSelectionMode SeatSelection { get; init; } = SeatSelectionMode.Skip;

    public bool RequireSeatMap { get; init; }

    public int Adults { get; init; } = 1;

    public int Teens { get; init; }

    public int Children { get; init; }

    public int Infants { get; init; }

    public bool AcceptInsurance { get; init; }

    public string? ExpectedCurrency { get; init; }

    public bool Authenticated { get; init; }

    public bool AwardSearch { get; init; }

    public int MinimumLoyaltyPoints { get; init; } = 1;

    public int? MaximumLoyaltyPoints { get; init; }

    public PaymentJourneyMode Payment { get; init; } = PaymentJourneyMode.Card;

    public bool StopAtHostedPaymentPage { get; init; }

    public string? LoyaltyPassword { get; init; }
}

public sealed record GuestJourneyResult(
    SearchSliceDeepLink Search,
    string SelectedFare,
    PriceSummarySnapshot InitialPrice,
    PriceSummarySnapshot PaymentPrice,
    IReadOnlyList<string> SelectedSeats,
    IReadOnlyList<string> SkippedSeatSelections,
    string PaymentOutcome,
    string? PaymentReferenceId,
    string ScreenshotPath,
    ConfirmationDetails? Confirmation);
