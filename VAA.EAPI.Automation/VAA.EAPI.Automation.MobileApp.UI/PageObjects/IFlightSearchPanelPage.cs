namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

/// <summary>Shared Search panel behaviour; platform adapters own identifiers and native mechanics.</summary>
public interface IFlightSearchPanelPage
{
    void Open();
    void SetRoundTrip(bool roundTrip);
    bool IsRoundTrip { get; }
    bool SearchEnabled { get; }
    void SetOneAdult();
    void SelectAirport(bool origin, string code);
    bool HasAirport(bool origin, string code);
    void SelectDates(DateOnly departure, DateOnly? returning = null);
    bool HasDates(DateOnly departure, DateOnly? returning = null);
    void OpenDates();
    void ChooseDate(DateOnly date);
    void CloseDates();
    void OpenPassengers();
    int PassengerCount(string type);
    bool CanDecreasePassenger(string type);
    void SetPassengerCount(string type, int desired);
    void ConfirmPassengers();
    void ClosePassengers();
    bool HasPassengerTotal(int count);
    IReadOnlyList<string> ReadRecentAirports(bool origin);
    void SelectRecentAirport(bool origin, string code);
}
