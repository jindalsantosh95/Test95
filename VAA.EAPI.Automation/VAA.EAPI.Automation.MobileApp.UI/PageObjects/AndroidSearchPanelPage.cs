using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Text;
using Ids = VAA.EAPI.Automation.MobileApp.UI.PageObjects.AndroidSearchResourceIds;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

/// <summary>Resource-ID-only interactions against the proposed Android Search panel contract.</summary>
public sealed class AndroidSearchPanelPage : NativeAppPage, IFlightSearchPanelPage
{
    public AndroidSearchPanelPage(AppiumDriver driver) : base(driver, TimeSpan.FromSeconds(15))
        => Wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

    private static By ResourceId(string id)
        => new ByAndroidUIAutomator($"new UiSelector().resourceId({JsonConvert.SerializeObject(id)})");

    private IWebElement Control(string id)
    {
        try
        {
            return Wait.Until(_ =>
            {
                IWebElement[] matches = Driver.FindElements(ResourceId(id)).Where(element => element.Displayed).ToArray();
                if (matches.Length > 1)
                {
                    throw new InvalidOperationException($"Android contract: duplicate visible resource ID '{id}'.");
                }
                return matches.SingleOrDefault();
            });
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new WebDriverTimeoutException(
                $"Android contract: resource ID '{id}' was not visible. Verify the agreed ID and modal testTagsAsResourceId exposure.", exception);
        }
    }

    private void Tap(string id)
    {
        IWebElement control = Wait.Until(_ =>
        {
            IWebElement candidate = Control(id);
            return candidate.Enabled ? candidate : null;
        });
        if (control.GetAttribute("clickable") != "true")
        {
            throw new InvalidOperationException($"Android contract: '{id}' must identify the actionable control, not its wrapper.");
        }
        control.Click();
    }

    private void WaitForPanelAfter(string sheet)
    {
        Wait.Until(_ => !Driver.FindElements(ResourceId(sheet)).Any(element => element.Displayed));
        Control(Ids.Panel);
    }

    private string[] Texts(string id)
        => Control(id).FindElements(By.ClassName("android.widget.TextView")).Select(element => element.Text).ToArray();

    public void Open()
    {
        Tap(Ids.BookTab);
        Control(Ids.Panel);
    }

    public void SetRoundTrip(bool roundTrip)
    {
        string id = roundTrip ? Ids.RoundTrip : Ids.OneWay;
        // Radio selection is exposed as checked; the active option need not be clickable.
        if (!IsControlChecked(id))
        {
            Tap(id);
        }
        Wait.Until(_ => IsControlChecked(id));
    }

    public bool IsRoundTrip => IsControlChecked(Ids.RoundTrip);

    private bool IsControlChecked(string id)
        => ParseCheckedState(Control(id).GetAttribute("checked"));

    internal static bool ParseCheckedState(string? checkedState)
        => bool.TryParse(checkedState, out bool selected)
            ? selected
            : throw new InvalidOperationException("Selectable control must expose a boolean checked state.");
    public bool SearchEnabled
    {
        get
        {
            IWebElement button = Control(Ids.Search);
            if (button.GetAttribute("clickable") != "true")
            {
                throw new InvalidOperationException("Android contract: Search ID must identify the button, not its enabled container.");
            }
            return button.Enabled;
        }
    }

    public void OpenAirports(bool origin)
    {
        Tap(origin ? Ids.Origin : Ids.Destination);
        Control(Ids.AirportInput);
    }

    public void CloseAirports(bool back = false)
    {
        Tap(back ? Ids.AirportBack : Ids.AirportClose);
        WaitForPanelAfter(Ids.AirportSheet);
    }

    public void SelectAirport(bool origin, string code)
    {
        OpenAirports(origin);
        IWebElement input = Control(Ids.AirportInput);
        input.Clear();
        input.SendKeys(code);
        Tap(Ids.AirportResultPattern.Replace("{IATA}", code, StringComparison.Ordinal));
        WaitForPanelAfter(Ids.AirportSheet);
        Wait.Until(_ => HasAirport(origin, code));
    }

    public bool HasAirport(bool origin, string code)
        => Texts(origin ? Ids.Origin : Ids.Destination).Any(text => text.Contains($"({code})", StringComparison.Ordinal));

    public string[] RecentAirports()
    {
        // Read the identified list, not translated headings or positional selectors.
        string prefix = Ids.AirportRecentPattern.Replace("{IATA}", string.Empty, StringComparison.Ordinal);
        By rows = new ByAndroidUIAutomator(
            $"new UiSelector().resourceIdMatches({JsonConvert.SerializeObject(Regex.Escape(prefix) + "[A-Z]{3}")})");
        List<string> airports = [];
        bool reachedEnd = false;
        for (int attempt = 0; attempt <= 5; attempt++)
        {
            IWebElement list = Control(Ids.AirportList);
            string[] visible = list.FindElements(rows).Where(element => element.Displayed)
                .OrderBy(element => element.Location.Y)
                .Select(element => (element.GetAttribute("resource-id") ??
                    throw new InvalidOperationException("Recent airport row lost its resource ID."))[prefix.Length..]).ToArray();
            if (visible.Distinct(StringComparer.Ordinal).Count() != visible.Length)
            {
                throw new InvalidOperationException("Duplicate visible recent-airport IDs.");
            }
            foreach (string airport in visible)
            {
                if (!airports.Contains(airport, StringComparer.Ordinal))
                    airports.Add(airport);
            }
            if (reachedEnd)
                return airports.ToArray();
            if (attempt < 5)
            {
                object? result = Driver.ExecuteScript("mobile: scrollGesture", new Dictionary<string, object>
                {
                    ["elementId"] = GetElementReference(list),
                    ["direction"] = "down",
                    ["percent"] = 0.65,
                });
                reachedEnd = result is bool canScroll
                    ? !canScroll
                    : throw new InvalidOperationException("Airport list scroll did not report whether more content remains.");
            }
        }
        throw new InvalidOperationException("Recent airport list did not reach its end within five bounded scrolls.");
    }

    public IReadOnlyList<string> ReadRecentAirports(bool origin)
    {
        OpenAirports(origin);
        return RecentAirports();
    }

    public void SelectRecentAirport(bool origin, string code)
    {
        // Reading a long recent list may have scrolled; reopen to restore its top before choosing.
        CloseAirports();
        OpenAirports(origin);
        Tap(Ids.AirportRecentPattern.Replace("{IATA}", code, StringComparison.Ordinal));
        WaitForPanelAfter(Ids.AirportSheet);
    }

    public void OpenDates()
    {
        Tap(Ids.Dates);
        Control(Ids.Calendar);
    }

    public void CloseDates()
    {
        Tap(Ids.CloseDates);
        WaitForPanelAfter(Ids.DateSheet);
    }

    public void SelectDates(DateOnly departure, DateOnly? returning = null)
    {
        OpenDates();
        // Read the draft: switching to return preserves the outbound date. Clicking it
        // again would complete a same-day range and the next click would restart it.
        for (int selection = 0; selection < 3; selection++)
        {
            string outbound = DraftDate(Ids.DepartureValue);
            string inbound = returning.HasValue ? DraftDate(Ids.ReturnValue) : "-";
            DateOnly? next = NextDateSelection(departure, returning, outbound, inbound);
            if (!next.HasValue)
                break;
            ChooseDate(next.Value);
            Wait.Until(_ => DraftDate(Ids.DepartureValue) != outbound ||
                (returning.HasValue && DraftDate(Ids.ReturnValue) != inbound));
        }
        if (NextDateSelection(departure, returning,
            DraftDate(Ids.DepartureValue), returning.HasValue ? DraftDate(Ids.ReturnValue) : "-").HasValue)
            throw new InvalidOperationException("Calendar draft did not match the requested dates after three selections.");
        Tap(Ids.ConfirmDates);
        WaitForPanelAfter(Ids.DateSheet);
        Wait.Until(_ => HasDates(departure, returning));
    }

    private string DraftDate(string id)
    {
        IWebElement value = Control(id);
        // The semantic full date includes the year and is independent of font-scale formatting.
        string description = value.GetAttribute("content-desc") ?? string.Empty;
        if (!description.IsNullOrWhiteSpace())
            return description;
        if (value.Text == "-")
            return "-";
        throw new InvalidOperationException($"Calendar value '{id}' must expose its full date description.");
    }

    internal static DateOnly? NextDateSelection(DateOnly departure, DateOnly? returning,
        string outbound, string inbound)
    {
        DateOnly? currentDeparture = ParseDraftDate(outbound);
        DateOnly? currentReturn = ParseDraftDate(inbound);
        if (currentDeparture == departure && currentReturn == returning)
            return null;
        // Only a matching, incomplete range can accept the return directly.
        // A complete or different range must start again with departure.
        return returning.HasValue && currentDeparture == departure && !currentReturn.HasValue
            ? returning.Value : departure;
    }

    private static DateOnly? ParseDraftDate(string value)
    {
        if (value == "-")
            return null;
        string[] formats = ["dddd, d MMMM yyyy", "dddd, MMMM d, yyyy"];
        return DateOnly.TryParseExact(value, formats, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateOnly date)
            ? date
            : throw new InvalidOperationException($"Calendar full date is not in the supported English format: '{value}'.");
    }

    public void ChooseDate(DateOnly date)
    {
        string id = Ids.DatePattern.Replace("{yyyy-MM-dd}", date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), StringComparison.Ordinal);
        for (int attempt = 0; attempt <= 3; attempt++)
        {
            IWebElement calendar = Control(Ids.Calendar);
            IWebElement footer = Control(Ids.DateFooter);
            int bottom = Math.Min(calendar.Location.Y + calendar.Size.Height, footer.Location.Y);
            IWebElement? day = Driver.FindElements(ResourceId(id)).FirstOrDefault(element =>
                element.Displayed && element.Enabled && element.Location.Y >= calendar.Location.Y &&
                element.Location.Y + element.Size.Height <= bottom);
            if (day is not null)
            {
                day.Click();
                Wait.Until(_ => IsControlChecked(id));
                return;
            }
            if (attempt < 3)
            {
                Driver.ExecuteScript("mobile: scrollGesture", new Dictionary<string, object>
                {
                    ["elementId"] = GetElementReference(calendar),
                    ["direction"] = "down",
                    ["percent"] = 0.65,
                });
            }
        }
        throw new NoSuchElementException($"Date '{id}' was not safely visible above the footer after three calendar-only scrolls.");
    }

    public bool HasDates(DateOnly departure, DateOnly? returning = null)
    {
        string expected = departure.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        if (returning.HasValue)
        {
            expected += " - " + returning.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        }
        return Texts(Ids.Dates).Any(text => text.Replace("Sept", "Sep", StringComparison.Ordinal) == expected);
    }

    public void OpenPassengers()
    {
        Tap(Ids.Passengers);
        Control(Ids.PassengerSheet);
    }

    public int PassengerCount(string type)
    {
        IWebElement count = Control(Ids.PassengerCountPattern.Replace("{TYPE}", type, StringComparison.Ordinal));
        string value = count.Text.IsNullOrWhiteSpace() ? count.GetAttribute("content-desc") ?? string.Empty : count.Text;
        Match match = RegexCompiled.Match(value, @"\d+$", RegexOptions.CultureInvariant);
        return match.Success ? int.Parse(match.Value, CultureInfo.InvariantCulture)
            : throw new InvalidOperationException($"No numeric count exposed for {type}.");
    }

    public bool CanDecreasePassenger(string type)
        => Control(Ids.PassengerDecrementPattern.Replace("{TYPE}", type, StringComparison.Ordinal)).Enabled;

    public void SetPassengerCount(string type, int desired)
    {
        if (desired < 0 || desired > 9)
        {
            throw new ArgumentOutOfRangeException(nameof(desired));
        }
        for (int attempt = 0; attempt < 10; attempt++)
        {
            int current = PassengerCount(type);
            if (current == desired)
                return;
            string pattern = current < desired ? Ids.PassengerIncrementPattern : Ids.PassengerDecrementPattern;
            Tap(pattern.Replace("{TYPE}", type, StringComparison.Ordinal));
            Wait.Until(_ => PassengerCount(type) != current);
        }
        throw new InvalidOperationException($"Could not set {type} count to {desired} within ten changes.");
    }

    public void ConfirmPassengers()
    {
        Tap(Ids.ConfirmPassengers);
        WaitForPanelAfter(Ids.PassengerSheet);
    }

    public void ClosePassengers()
    {
        Tap(Ids.ClosePassengers);
        WaitForPanelAfter(Ids.PassengerSheet);
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
        => Texts(Ids.Passengers).Contains(count == 1 ? "1 traveller" : $"{count} travellers", StringComparer.Ordinal);

    public void OpenPromo()
    {
        Tap(Ids.PromoEntry);
        Control(Ids.PromoInput);
    }

    public void EnterPromo(string code)
    {
        IWebElement input = Control(Ids.PromoInput);
        input.Clear();
        input.SendKeys(code);
        Wait.Until(_ => Control(Ids.PromoInput).Text == code);
    }

    public string PromoValue => Control(Ids.PromoInput).Text;
    public void WaitForPromoValidity(bool enabled) => Wait.Until(_ => CanApplyPromo == enabled);
    public bool CanApplyPromo => Control(Ids.PromoApply).Enabled;
    public bool HasPromo(string code) => Texts(Ids.PromoEntry).Any(text => text.Contains(code, StringComparison.Ordinal));

    public void ApplyPromo()
    {
        Tap(Ids.PromoApply);
        WaitForPanelAfter(Ids.PromoSheet);
    }

    public void ClearPromo()
    {
        Tap(Ids.PromoClear);
        WaitForPanelAfter(Ids.PromoSheet);
    }

    public void ClosePromo()
    {
        Tap(Ids.PromoClose);
        WaitForPanelAfter(Ids.PromoSheet);
    }
}
