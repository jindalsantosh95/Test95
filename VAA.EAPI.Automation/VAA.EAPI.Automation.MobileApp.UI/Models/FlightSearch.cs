namespace VAA.EAPI.Automation.MobileApp.UI.Models;

public sealed record FlightSearch(
    string Origin,
    string Destination,
    DateOnly DepartureDate,
    PassengerCounts Passengers,
    DateOnly? ReturnDate = null)
{
    public bool IsReturn => ReturnDate.HasValue;

    public static FlightSearch OneWay(
        string origin,
        string destination,
        int monthsFromToday,
        PassengerCounts? passengers = null)
    {
        DateOnly departureDate = DateOnly.FromDateTime(DateTime.Today).AddMonths(monthsFromToday);
        return new FlightSearch(origin, destination, departureDate, passengers ?? PassengerCounts.OneAdult);
    }

    public static FlightSearch Return(
        string origin,
        string destination,
        int monthsFromToday,
        int tripLengthDays = 7,
        PassengerCounts? passengers = null)
    {
        DateOnly departureDate = DateOnly.FromDateTime(DateTime.Today).AddMonths(monthsFromToday);
        return new FlightSearch(
            origin,
            destination,
            departureDate,
            passengers ?? PassengerCounts.OneAdult,
            departureDate.AddDays(tripLengthDays));
    }
}

public sealed record PassengerCounts(int Adults, int Teens = 0, int Children = 0, int Infants = 0)
{
    public static PassengerCounts OneAdult { get; } = new(1);

    public int Total => Adults + Teens + Children + Infants;
}

public sealed record FlightSummary(
    string OriginText,
    string DestinationText,
    string FareText,
    string ItineraryText,
    string PriceText,
    bool ContinueEnabled);

public sealed record InsufficientPointsSummary(string WarningText, bool ContinueEnabled);

public sealed record FareOfferObservation(
    string ItineraryFingerprint,
    string FareName,
    decimal Price,
    string PromotionText);

public sealed record FareBaggageObservation(
    string FareLabel,
    string SummaryFareLabel,
    IReadOnlyList<string> BenefitLabels,
    IReadOnlyList<string> BaggageAllowances);
