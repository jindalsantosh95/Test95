using System.Globalization;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using Ids = VAA.EAPI.Automation.MobileApp.UI.PageObjects.NativeSearchAccessibilityIds;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

/// <summary>Adapts the established iOS page to shared Search panel scenarios; no Android selectors leak here.</summary>
public sealed class IosSearchPanelPage : NativeAppPage, IFlightSearchPanelPage
{
    private readonly NativeFlightSearchPage _page;

    public IosSearchPanelPage(AppiumDriver driver) : base(driver, TimeSpan.FromSeconds(30))
    {
        _page = new NativeFlightSearchPage(driver);
        Wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
    }

    public void Open()
    {
        new AppEntryHandler(Driver).EnterAsGuest();
        _page.Open();
    }

    public void SetRoundTrip(bool roundTrip)
        => _page.SetJourneyType(roundTrip);

    public bool IsRoundTrip => _page.IsRoundTripSelected;

    public bool SearchEnabled => _page.IsSearchFlightsEnabled();

    public void SelectAirport(bool origin, string code)
    {
        _page.SelectAirport(origin ? AirportSearchField.Origin : AirportSearchField.Destination, code);
        Wait.Until(_ => HasAirport(origin, code));
    }

    private string[] FieldText(string id)
    {
        IWebElement field = WaitForElementByKey(id);
        return new[] { field.Text, field.GetAttribute("label") ?? string.Empty, field.GetAttribute("value") ?? string.Empty }
            // Selenium's By.ClassName becomes CSS on IWebElement. Keep the child
            // lookup native and scoped to the field already located by its owned ID.
            .Concat(field.FindElements(new ByIosClassChain("**/XCUIElementTypeStaticText"))
                .Select(element => element.Text)).ToArray();
    }

    public bool HasAirport(bool origin, string code)
        => FieldText(origin ? Ids.FlyingFromButton : Ids.FlyingToButton).Any(text => text.Contains(code, StringComparison.Ordinal));

    public void SelectDates(DateOnly departure, DateOnly? returning = null)
    {
        _page.SelectDates(departure, returning);
        Wait.Until(_ => HasDates(departure, returning));
    }

    public bool HasDates(DateOnly departure, DateOnly? returning = null)
    {
        string expected = departure.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        if (returning.HasValue)
            expected += " - " + returning.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        return FieldText(Ids.WhenButton).Any(text => text.Contains(expected, StringComparison.Ordinal) &&
            (returning.HasValue || !text.Contains(" - ", StringComparison.Ordinal)));
    }

    public void OpenDates() => _page.OpenDateSelector();
    public void ChooseDate(DateOnly date) => _page.SelectDate(date);
    public void CloseDates() => _page.CancelDateSelection();
    public void OpenPassengers() => _page.OpenPassengerSelector();

    private static PassengerCategory Category(string type) => type switch
    {
        "ADULTS" => PassengerCategory.Adult,
        "TEENS" => PassengerCategory.Teen,
        "CHILDREN" => PassengerCategory.Child,
        "INFANTS" => PassengerCategory.Infant,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown traveller type."),
    };

    public int PassengerCount(string type) => _page.ReadPassengerCount(Category(type));
    public bool CanDecreasePassenger(string type) => _page.IsPassengerDecrementEnabled(Category(type));

    public void SetPassengerCount(string type, int desired)
    {
        if (desired < 0 || desired > 9)
            throw new ArgumentOutOfRangeException(nameof(desired));
        for (int attempt = 0; attempt < 10; attempt++)
        {
            int current = PassengerCount(type);
            if (current == desired)
                return;
            if (current < desired)
                _page.IncrementPassenger(Category(type));
            else
                _page.AttemptPassengerDecrement(Category(type));
            Wait.Until(_ => PassengerCount(type) != current);
        }
        throw new InvalidOperationException($"Could not set {type} to {desired} within ten changes.");
    }

    private void WaitForPassengerDismissal()
        => Wait.Until(_ => !Driver.FindElements(MobileBy.AccessibilityId(Ids.AdultCount)).Any(element => element.Displayed));

    public void ConfirmPassengers()
    {
        _page.ConfirmPassengers();
        WaitForPassengerDismissal();
    }

    public void ClosePassengers()
    {
        TapByKey(Ids.PassengerSelectorCancelButton);
        WaitForPassengerDismissal();
    }

    public void SetOneAdult()
    {
        OpenPassengers();
        SetPassengerCount("INFANTS", 0);
        SetPassengerCount("CHILDREN", 0);
        SetPassengerCount("TEENS", 0);
        SetPassengerCount("ADULTS", 1);
        ConfirmPassengers();
    }

    public bool HasPassengerTotal(int count)
    {
        string[] observed = [];
        try
        {
            return Wait.Until(_ =>
            {
                observed = FieldText(Ids.WhoButton);
                return observed.Any(text => MatchesPassengerTotal(text, count));
            });
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new WebDriverTimeoutException(
                $"Expected passenger summary for {count} people; observed '{string.Join(" | ", observed)}'.", exception);
        }
    }

    internal static bool MatchesPassengerTotal(string text, int count)
        => VAA.EAPI.Automation.Core.Text.RegexCompiled.IsMatch(text,
            $@"(?<!\d){count}\s+{(count == 1 ? "person" : "people")}\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public IReadOnlyList<string> ReadRecentAirports(bool origin)
        => _page.ReadRecentAirportCodes(origin ? AirportSearchField.Origin : AirportSearchField.Destination);

    public void SelectRecentAirport(bool origin, string code)
        => _page.SelectRecentAirport(origin ? AirportSearchField.Origin : AirportSearchField.Destination, code);
}
