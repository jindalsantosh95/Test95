using System.Globalization;
using System.Xml.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Interactions;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Text;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.VisualValidation;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

public sealed class NativeFlightSearchPage : NativeAppPage
{
    private const string BookTabAccessibilityId = "Book";
    private static readonly TimeSpan ScreenTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ResultsTimeout = TimeSpan.FromSeconds(60);
    private static readonly By BookTabPostRestartFallback = By.XPath(
        "//*[@text='Book' or @content-desc='Book' or @label='Book' or @name='Book']");

    public NativeFlightSearchPage(AppiumDriver driver)
        : base(driver, ScreenTimeout)
    {
    }

    public void Open()
    {
        if (TryWaitForElementByKey(
            NativeSearchAccessibilityIds.FlyingFromButton,
            TimeSpan.FromMilliseconds(500)) is not null)
        {
            // iOS restores the last navigation stack when a local session relaunches the app.
            // Re-enter Search rather than inheriting dates, filters, or an incomplete range
            // from the preceding test. BrowserStack starts from this landing page naturally.
            Driver.Navigate().Back();
            OpenSearchFromBookLanding();
            return;
        }

        TapBookTab();
        OpenSearchFromBookLanding();
    }

    private void OpenSearchFromBookLanding()
    {
        WaitForElement(
            new ByIosNSPredicate("type == 'XCUIElementTypeStaticText' AND (label == 'Search for flights' OR name == 'Search for flights')"),
            ScreenTimeout).Click();
        WaitForElementByKey(NativeSearchAccessibilityIds.FlyingFromButton, ScreenTimeout);
    }

    public void ReturnToSearchPanelWithoutPayment()
    {
        for (int step = 0; step <= 6; step++)
        {
            if (IsVisibleByKey(NativeSearchAccessibilityIds.FlyingFromButton, TimeSpan.FromSeconds(1)))
                return;
            if (step < 6)
                Driver.Navigate().Back();
        }
        throw new InvalidOperationException("Could not return to Search within six native back actions. No payment action attempted.");
    }

    public byte[] CaptureFlyingFromSelectionRow(string accessibilityId)
        => CaptureSearchPanelComponent(accessibilityId);

    public byte[] CaptureFlyingToSelectionRow(string accessibilityId)
        => CaptureSearchPanelComponent(accessibilityId);

    public byte[] CaptureJourneyTypePicker(string accessibilityId)
        => CaptureSearchPanelComponent(accessibilityId);

    public byte[] CaptureWhenSelectionRow(string accessibilityId)
        => CaptureSearchPanelComponent(accessibilityId);

    public byte[] CaptureWhoSelectionRow(string accessibilityId)
        => CaptureSearchPanelComponent(accessibilityId);

    public byte[] CaptureSearchFlightsButton(string accessibilityId)
    {
        IWebElement button = FindKeyWithScrolling(
            accessibilityId,
            maximumSwipes: 3)
                             ?? throw new NoSuchElementException(
                                 "The Search Flights button was not displayed after bounded scrolling.");
        string label = ReadAccessibleLabel(button).Trim();
        if (!label.Equals("Search flights", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"The Search Flights accessibility label was '{label}', not 'Search flights'.");
        }

        return new AppiumComponentScreenshot().Capture(Driver, button);
    }

    public byte[] CaptureRewardSearchControl(string accessibilityId, bool enabled)
    {
        if (enabled)
        {
            EnsureRewardSearchEnabled();
        }
        else
        {
            EnsureRewardSearchDisabled();
        }

        return CaptureSearchPanelScrollableComponent(accessibilityId);
    }

    public byte[] ToggleAndCaptureRewardSearchControl()
    {
        IWebElement toggle = FindKeyWithScrolling(
            NativeSearchAccessibilityIds.RewardSearchToggle,
            maximumSwipes: 3)
                              ?? throw new NoSuchElementException(
                                  "The Reward Search toggle was not displayed after bounded scrolling.");
        IWebElement control = WaitForElementByKey(
            NativeSearchAccessibilityIds.RewardSearchControl,
            ScreenTimeout);
        byte[] before = new AppiumComponentScreenshot().Capture(Driver, control);
        toggle.Click();

        byte[]? changed = null;
        Wait.Until(_ =>
        {
            IWebElement refreshed = WaitForElementByKey(
                NativeSearchAccessibilityIds.RewardSearchControl,
                ScreenTimeout);
            byte[] candidate = new AppiumComponentScreenshot().Capture(Driver, refreshed);
            if (before.AsSpan().SequenceEqual(candidate))
            {
                return false;
            }

            changed = candidate;
            return true;
        });

        return changed ?? throw new InvalidOperationException(
            "The Reward Search control did not produce a changed visual state after selection.");
    }

    public byte[] CaptureDestinationAirportSearchField(string accessibilityId)
    {
        TapByKey(NativeSearchAccessibilityIds.FlyingToButton);
        IWebElement searchField = WaitForElementByKey(
            accessibilityId,
            ScreenTimeout);
        return new AppiumComponentScreenshot().Capture(Driver, searchField);
    }

    public byte[] CaptureInitialCalendarHeader(string accessibilityId)
    {
        OpenDateSelector();
        IWebElement calendar = WaitForElementByKey(accessibilityId, ScreenTimeout);
        const int headerHeightPoints = 64;
        return new AppiumComponentScreenshot().CaptureRegion(
            Driver,
            calendar,
            relativeX: 0,
            relativeY: 0,
            width: calendar.Size.Width,
            height: Math.Min(headerHeightPoints, calendar.Size.Height));
    }

    public byte[] CaptureEmptyDiscountCodeField(string accessibilityId)
    {
        OpenDiscountCodeEditor();
        return CaptureSearchPanelComponent(accessibilityId);
    }

    public byte[] CaptureDefaultPassengerSelector(string accessibilityId)
    {
        OpenPassengerSelector();
        return CaptureSearchPanelComponent(accessibilityId);
    }

    public byte[] CaptureDisabledDiscountCodeApplyButton(string accessibilityId)
    {
        OpenDiscountCodeEditor();
        return CaptureSearchPanelComponent(accessibilityId);
    }

    public void OpenDiscountCodeEditor()
    {
        TapByKey(NativeSearchAccessibilityIds.DiscountCodeButton);
        WaitForElementByKey(NativeSearchAccessibilityIds.DiscountCodeTextField, ScreenTimeout);
    }

    public string ReadDiscountCodeDraft()
        => WaitForElementByKey(NativeSearchAccessibilityIds.DiscountCodeTextField)
            .GetAttribute("value") ?? string.Empty;

    public string ReadDiscountCodePanelLabel()
        => ReadAccessibleLabel(WaitForElementByKey(NativeSearchAccessibilityIds.DiscountCodeButton));

    public bool IsDiscountCodeApplyEnabled()
        => WaitForElementByKey(NativeSearchAccessibilityIds.DiscountCodeApplyButton).Enabled;

    public void EnterDiscountCodeDraft(string code, bool expectedApplyEnabled)
    {
        IWebElement field = WaitForElementByKey(NativeSearchAccessibilityIds.DiscountCodeTextField);
        field.Clear();
        field.SendKeys(code);
        Wait.Until(_ => ReadDiscountCodeDraft() == code &&
            IsDiscountCodeApplyEnabled() == expectedApplyEnabled);
    }

    public void ApplyDiscountCodeDraft()
    {
        Wait.Until(_ => IsDiscountCodeApplyEnabled());
        DismissDiscountCodeEditor(NativeSearchAccessibilityIds.DiscountCodeApplyButton);
    }

    public void ClearDiscountCode()
        => DismissDiscountCodeEditor(NativeSearchAccessibilityIds.DiscountCodeClearButton);

    public void CancelDiscountCodeDraft()
        => DismissDiscountCodeEditor(NativeSearchAccessibilityIds.DiscountCodeCancelButton);

    private void DismissDiscountCodeEditor(string actionId)
    {
        TapByKey(actionId);
        Wait.Until(_ => !Driver.FindElements(
            MobileBy.AccessibilityId(NativeSearchAccessibilityIds.DiscountCodeTextField))
            .Any(element => element.Displayed));
        WaitForElementByKey(NativeSearchAccessibilityIds.DiscountCodeButton);
    }

    private byte[] CaptureSearchPanelComponent(string accessibilityId)
    {
        IWebElement component = WaitForElementByKey(accessibilityId, ScreenTimeout);
        return new AppiumComponentScreenshot().Capture(Driver, component);
    }

    private byte[] CaptureSearchPanelScrollableComponent(string accessibilityId)
    {
        IWebElement component = FindKeyWithScrolling(accessibilityId, maximumSwipes: 3)
                                ?? throw new NoSuchElementException(
                                    $"The visual component '{accessibilityId}' was not displayed after bounded scrolling.");
        return new AppiumComponentScreenshot().Capture(Driver, component);
    }

    private void TapBookTab()
    {
        IWebElement? bookTab = TryWaitForElementByKey(BookTabAccessibilityId, TimeSpan.FromSeconds(2));
        if (bookTab is not null && bookTab.Displayed)
        {
            bookTab.Click();
            return;
        }

        // BrowserStack's iOS accessibility-id lookup can miss the visible root tab after
        // app launch. The proved passing path uses this exact semantic XPath.
        // Keep the exception local to the app-entry tab and remove it once the app's
        // accessibility-id lookup is reliable after restart.
        try
        {
            WaitForElement(BookTabPostRestartFallback, ScreenTimeout).Click();
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new WebDriverTimeoutException(
                "The native Book tab did not become available after the app-entry transition. " +
                $"Tried accessibility ID '{BookTabAccessibilityId}' first, then the documented " +
                "iOS semantic fallback. Inspect the BrowserStack screenshot/source for an onboarding " +
                "or restart overlay before changing the app selector contract.",
                exception);
        }
    }

    public void SearchOneWay(FlightSearch search)
    {
        SelectSearchCriteria(search, "One way");
        SelectDates(search.DepartureDate);
        SelectPassengers(search.Passengers);
        TapSearchFlights();
    }

    public void SearchReturn(FlightSearch search)
    {
        if (!search.ReturnDate.HasValue)
        {
            throw new ArgumentException("A return search requires a return date.", nameof(search));
        }

        SelectSearchCriteria(search, "Round trip");
        SelectDates(search.DepartureDate, search.ReturnDate.Value);
        SelectPassengers(search.Passengers);
        TapSearchFlights();
    }

    public void EnsureRewardSearchEnabled()
    {
        IWebElement toggle = WaitForElementByKey(
            NativeSearchAccessibilityIds.RewardSearchToggle,
            ScreenTimeout);
        if (!IsRewardSearchSelected(toggle))
        {
            toggle.Click();
            Wait.Until(_ => IsRewardSearchSelected(
                WaitForElementByKey(NativeSearchAccessibilityIds.RewardSearchToggle, ScreenTimeout)));
        }
    }

    public void EnsureRewardSearchDisabled()
    {
        IWebElement toggle = WaitForElementByKey(
            NativeSearchAccessibilityIds.RewardSearchToggle,
            ScreenTimeout);
        if (IsRewardSearchSelected(toggle))
        {
            toggle.Click();
            Wait.Until(_ => !IsRewardSearchSelected(
                WaitForElementByKey(NativeSearchAccessibilityIds.RewardSearchToggle, ScreenTimeout)));
        }
    }

    public void EditSearch()
    {
        TapByKey(NativeSearchAccessibilityIds.ResultsEditSearch);
        WaitForElementByKey(NativeSearchAccessibilityIds.SearchFlightsButton, ScreenTimeout);
    }

    public void ReturnToSearchEntry()
    {
        Driver.Navigate().Back();
        WaitForElementByKey(NativeSearchAccessibilityIds.SearchFlightsButton, ScreenTimeout);
    }

    public void ApplyDiscountCode(string discountCode)
    {
        TapByKey(NativeSearchAccessibilityIds.DiscountCodeButton);
        IWebElement field = WaitForElementByKey(NativeSearchAccessibilityIds.DiscountCodeTextField, ScreenTimeout);
        field.Clear();
        field.SendKeys(discountCode);
        TapByKey(NativeSearchAccessibilityIds.DiscountCodeApplyButton);

        Wait.Until(_ =>
        {
            IWebElement? discountCodeButton = TryWaitForElementByKey(
                NativeSearchAccessibilityIds.DiscountCodeButton,
                TimeSpan.FromSeconds(1));

            return discountCodeButton is not null &&
                   ReadAccessibleLabel(discountCodeButton)
                       .Contains(discountCode, StringComparison.OrdinalIgnoreCase);
        });
    }

    public void SubmitEditedSearch()
    {
        TapSearchFlights();
        WaitForResults();
    }

    public FareOfferObservation ReadFareOffer(int flightIndex, string cabin, string fareName, bool requirePromotion = false)
    {
        IReadOnlyDictionary<string, string> results = ReadFlightResultTextFromPageSource();
        string resultId = NativeSearchAccessibilityIds.FlightResult(flightIndex);
        if (!results.TryGetValue(resultId, out string? resultText))
        {
            throw new NoSuchElementException($"Flight result {flightIndex} was unavailable for fare comparison.");
        }

        IWebElement? cabinButton = FindKeyWithScrolling(
            NativeSearchAccessibilityIds.FlightCabin(flightIndex, cabin), 3);
        if (cabinButton is null)
        {
            throw new NoSuchElementException($"Flight result {flightIndex} did not expose cabin '{cabin}'.");
        }

        cabinButton.Click();
        WaitForElementByKey(NativeSearchAccessibilityIds.FareSelectorReady, ScreenTimeout);

        // Discover the exact fare title rather than assuming its carousel position.
        int fareIndex = FindFareIndex(Driver.PageSource, fareName);
        string fareText = ReadAccessibleLabel(WaitForElementByKey(
            NativeSearchAccessibilityIds.FareSelectButton(fareIndex), ScreenTimeout));
        if (!fareText.Contains('£'))
        {
            throw new InvalidOperationException($"Fare '{fareName}' must expose a GBP price for this comparison: {fareText}");
        }
        decimal price = ReadMinimumPrice(fareText)
            ?? throw new InvalidOperationException($"Fare '{fareName}' exposed no readable price: {fareText}");
        string promotionText = string.Empty;
        if (requirePromotion)
        {
            Wait.Until(_ =>
            {
                promotionText = ReadFarePromotion(Driver.PageSource, fareIndex);
                return !promotionText.IsNullOrWhiteSpace();
            });
        }
        else
        {
            promotionText = ReadFarePromotion(Driver.PageSource, fareIndex);
        }

        TapByKey(NativeSearchAccessibilityIds.FareSelectorDismiss);
        WaitForElementByKey(resultId, ScreenTimeout);

        return new FareOfferObservation(
            BuildItineraryFingerprint(resultText),
            fareName,
            price,
            promotionText);
    }

    internal static int FindFareIndex(string source, string fareName)
    {
        int[] indexes = XDocument.Parse(source).Descendants()
            .Where(element => ((string?)element.Attribute("label") ?? string.Empty).Trim().Is(fareName))
            .Select(element => (string?)element.Attribute("name") ?? string.Empty)
            .Where(id => id.StartsWith(NativeSearchAccessibilityIds.FareSelectorFarePrefix, StringComparison.Ordinal)
                && id.EndsWith("Title", StringComparison.Ordinal))
            .Select(id => id[NativeSearchAccessibilityIds.FareSelectorFarePrefix.Length..^"Title".Length])
            .Where(index => index.Length > 0 && index.All(char.IsDigit))
            .Select(index => int.Parse(index, CultureInfo.InvariantCulture))
            .Distinct().ToArray();
        return indexes.Length == 1 ? indexes[0]
            : throw new InvalidOperationException($"Expected one exact '{fareName}' fare title; found {indexes.Length}.");
    }

    internal static string ReadFarePromotion(string source, int fareIndex)
    {
        string id = NativeSearchAccessibilityIds.FareAppliedPromotion(fareIndex);
        string[] labels = XDocument.Parse(source).Descendants()
            .Where(element => (string?)element.Attribute("name") == id)
            .Select(element => ((string?)element.Attribute("label") ?? string.Empty).Trim())
            .Where(label => !label.IsNullOrWhiteSpace())
            .Distinct(StringComparer.Ordinal).ToArray();
        return labels.Length switch
        {
            0 => string.Empty,
            1 => labels[0],
            _ => throw new InvalidOperationException($"Conflicting promotion labels for '{id}'."),
        };
    }

    public string ReadAppliedPromotion(int flightIndex)
    {
        IWebElement promotion = WaitForElementByKey(
            NativeSearchAccessibilityIds.FlightResultAppliedPromotion(flightIndex), ScreenTimeout);
        return ReadAccessibleLabel(promotion);
    }

    public void OpenDateSelector()
    {
        TapByKey(NativeSearchAccessibilityIds.WhenButton);
        WaitForElementByKey(NativeSearchAccessibilityIds.Calendar, ScreenTimeout);
    }

    public void CancelDateSelection()
    {
        TapByKey(NativeSearchAccessibilityIds.CalendarCancelButton);
        Wait.Until(_ => !IsCalendarVisible());
    }

    public bool IsSearchFlightsEnabled()
        => WaitForElementByKey(NativeSearchAccessibilityIds.SearchFlightsButton, ScreenTimeout).Enabled;

    public void SelectRoundTrip()
        => SetJourneyType(true);

    public void SetJourneyType(bool roundTrip)
    {
        JourneyTypeOption(roundTrip).Click();
        Wait.Until(_ => IsRoundTripSelected == roundTrip);
    }

    public bool IsRoundTripSelected
    {
        get
        {
            IWebElement option = JourneyTypeOption(true);
            return option.Selected || option.GetAttribute("value") == "1";
        }
    }

    private IWebElement JourneyTypeOption(bool roundTrip)
    {
        IWebElement picker = WaitForElementByKey(NativeSearchAccessibilityIds.JourneyTypePicker, ScreenTimeout);
        string id = roundTrip ? NativeSearchAccessibilityIds.RoundTripOption : NativeSearchAccessibilityIds.OneWayOption;
        // Compatibility for SwiftUI's segmented Picker: older builds expose the
        // option ID on Text, not the actionable button. Remove when both buttons own IDs.
        return picker.FindElements(MobileBy.AccessibilityId(id))
            .FirstOrDefault(element => element.Displayed && element.TagName == "XCUIElementTypeButton")
            ?? picker.FindElement(new ByIosNSPredicate(
                $"type == 'XCUIElementTypeButton' AND label == '{(roundTrip ? "Round trip" : "One way")}'"));
    }

    public bool IsCalendarDateEnabled(DateOnly date)
    {
        IWebElement day = FindCalendarDayWithScrolling(date, 12)
            ?? throw new NoSuchElementException(
                $"Calendar date {date:yyyy-MM-dd} was not exposed for an availability assertion.");
        return day.Enabled;
    }

    public bool IsDateSelectionConfirmationEnabled()
        => WaitForElementByKey(NativeSearchAccessibilityIds.DatePickerNextButton, ScreenTimeout).Enabled;

    public void SelectDate(DateOnly date) => SelectCalendarDay(date);

    public CalendarDateSelection ReadCalendarSelection()
        => new(
            ReadControlText(NativeSearchAccessibilityIds.CalendarDepartureDateValue),
            ReadControlText(NativeSearchAccessibilityIds.CalendarReturnDateValue));

    public void ConfirmDateSelection()
    {
        IWebElement nextButton = WaitForElementByKey(
            NativeSearchAccessibilityIds.DatePickerNextButton,
            ScreenTimeout);

        if (!nextButton.Enabled)
        {
            throw new InvalidOperationException(
                "The calendar Next button remained disabled after selecting dates.");
        }

        nextButton.Click();
        try
        {
            Wait.Until(_ => !IsCalendarVisible());
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new WebDriverTimeoutException(
                "The calendar did not dismiss after an enabled Next action. " +
                "Do not use the calendar cancel button as a normal-path fallback; inspect the " +
                "BrowserStack calendar state and app-owned date-selection contract.",
                exception);
        }
    }

    public void SelectAirports(string origin, string destination)
    {
        SelectAirport(
            NativeSearchAccessibilityIds.FlyingFromButton,
            NativeSearchAccessibilityIds.OriginAirportSearchField,
            NativeSearchAccessibilityIds.OriginPrefix,
            origin);
        SelectAirport(
            NativeSearchAccessibilityIds.FlyingToButton,
            NativeSearchAccessibilityIds.DestinationAirportSearchField,
            NativeSearchAccessibilityIds.DestinationPrefix,
            destination);
    }

    public void SelectAirport(AirportSearchField field, string code)
        => SelectAirport(
            field == AirportSearchField.Origin ? NativeSearchAccessibilityIds.FlyingFromButton : NativeSearchAccessibilityIds.FlyingToButton,
            field == AirportSearchField.Origin ? NativeSearchAccessibilityIds.OriginAirportSearchField : NativeSearchAccessibilityIds.DestinationAirportSearchField,
            field == AirportSearchField.Origin ? NativeSearchAccessibilityIds.OriginPrefix : NativeSearchAccessibilityIds.DestinationPrefix,
            code);

    public IReadOnlyList<string> ReadRecentAirportCodes(AirportSearchField field)
    {
        string selectionButton = field == AirportSearchField.Origin
            ? NativeSearchAccessibilityIds.FlyingFromButton
            : NativeSearchAccessibilityIds.FlyingToButton;
        string sectionId = field == AirportSearchField.Origin
            ? NativeSearchAccessibilityIds.OriginRecentAirportsSection
            : NativeSearchAccessibilityIds.DestinationRecentAirportsSection;
        string airportPrefix = field == AirportSearchField.Origin
            ? NativeSearchAccessibilityIds.OriginRecentAirportPrefix
            : NativeSearchAccessibilityIds.DestinationRecentAirportPrefix;

        TapByKey(selectionButton);
        WaitForElementByKey(sectionId, ScreenTimeout);

        return Driver.FindElements(new ByIosNSPredicate(
                $"type == 'XCUIElementTypeButton' AND name BEGINSWITH '{airportPrefix}'"))
            .Where(element => element.Displayed)
            .Select(element => element.GetAttribute("name") ?? string.Empty)
            .Where(name => name.StartsWith(airportPrefix, StringComparison.Ordinal))
            .Select(name => name[airportPrefix.Length..])
            .Where(code => !code.IsNullOrWhiteSpace())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public void SelectRecentAirport(AirportSearchField field, string airportCode)
    {
        string airportPrefix = field == AirportSearchField.Origin
            ? NativeSearchAccessibilityIds.OriginRecentAirportPrefix
            : NativeSearchAccessibilityIds.DestinationRecentAirportPrefix;
        TapByKey($"{airportPrefix}{airportCode.ToUpperInvariant()}");
    }

    public void OpenPassengerSelector()
    {
        IWebElement whoButton = WaitForElementByKey(NativeSearchAccessibilityIds.WhoButton, ScreenTimeout);
        whoButton.Click();
        if (TryWaitForElementByKey(
            NativeSearchAccessibilityIds.AdultCount,
            TimeSpan.FromSeconds(2)) is null)
        {
            // XCUITest can resolve the app-owned identifier to a non-actionable SwiftUI child.
            // Target the enabled native button by its stable visible semantics until the app
            // moves the identifier onto the actionable control.
            WaitForElement(new ByIosNSPredicate(
                "type == 'XCUIElementTypeButton' AND enabled == true AND " +
                "(label CONTAINS[c] 'Who' OR name CONTAINS[c] 'Who')"), ScreenTimeout).Click();
        }

        WaitForElementByKey(NativeSearchAccessibilityIds.AdultCount, ScreenTimeout);
    }

    public int ReadPassengerCount(PassengerCategory category)
    {
        (string countKey, _, _) = PassengerKeys(category);
        return ParsePassengerCount(WaitForElementByKey(countKey, ScreenTimeout).Text);
    }

    public bool IsPassengerIncrementEnabled(PassengerCategory category)
    {
        (_, string incrementKey, _) = PassengerKeys(category);
        return WaitForElementByKey(incrementKey, ScreenTimeout).Enabled;
    }

    public bool IsPassengerDecrementEnabled(PassengerCategory category)
    {
        (_, _, string decrementKey) = PassengerKeys(category);
        return WaitForElementByKey(decrementKey, ScreenTimeout).Enabled;
    }

    public void IncrementPassenger(PassengerCategory category)
    {
        (string countKey, string incrementKey, _) = PassengerKeys(category);
        IWebElement count = WaitForElementByKey(countKey, ScreenTimeout);
        int expectedCount = ParsePassengerCount(count.Text) + 1;
        TapByKey(incrementKey);
        Wait.Until(_ => ParsePassengerCount(count.Text) == expectedCount);
    }

    public void AttemptPassengerIncrement(PassengerCategory category)
    {
        (_, string incrementKey, _) = PassengerKeys(category);
        IWebElement increment = WaitForElementByKey(incrementKey, ScreenTimeout);
        if (increment.Enabled)
        {
            increment.Click();
        }
    }

    public void AttemptPassengerDecrement(PassengerCategory category)
    {
        (_, _, string decrementKey) = PassengerKeys(category);
        IWebElement decrement = WaitForElementByKey(decrementKey, ScreenTimeout);
        if (decrement.Enabled)
        {
            decrement.Click();
        }
    }

    public string ReadPassengerValidation()
        => WaitForElementByKey(NativeSearchAccessibilityIds.PassengerValidation, ScreenTimeout).Text;

    public void ConfirmPassengers()
        => TapByKey(NativeSearchAccessibilityIds.PassengerSelectorNextButton);

    public void WaitForResults()
    {
        OpenQA.Selenium.Support.UI.WebDriverWait resultsWait = new(Driver, ResultsTimeout);
        resultsWait.Until(_ =>
            IsVisibleByKey("NativeFlightSearchResultsTitle", TimeSpan.FromMilliseconds(500)) ||
            IsVisibleByKey(NativeSearchAccessibilityIds.ResultsNavigationHeader, TimeSpan.FromMilliseconds(500)) ||
            ThrowIfResultsUnavailable() ||
            ThrowIfSubmissionFailed());
    }

    private void TapSearchFlights()
    {
        IWebElement searchButton = WaitForElementByKey(
            NativeSearchAccessibilityIds.SearchFlightsButton, ScreenTimeout);
        Wait.Until(_ => searchButton.Displayed && searchButton.Enabled);
        searchButton.Click();
    }

    public ResultsRefinement ApplyDiscoveredPriceSortAndFilter()
    {
        IWebElement sortControl = WaitForElementByKey(NativeSearchAccessibilityIds.ResultsSort, ScreenTimeout);
        IWebElement filterControl = WaitForElementByKey(NativeSearchAccessibilityIds.ResultsFilter, ScreenTimeout);
        string initialFilterLabel = ReadAccessibleLabel(filterControl);
        string baselineResultSignature = ReadResultSignature();

        sortControl.Click();
        IWebElement sortOption = WaitForElement(new ByIosNSPredicate(
            $"name == '{NativeSearchAccessibilityIds.PriceSortOption}' AND selected == false AND enabled == true"),
            ScreenTimeout);
        string sortLabel = ReadAccessibleLabel(sortOption);
        sortOption.Click();
        TapByKey(NativeSearchAccessibilityIds.ApplySort);
        WaitForRefinementSheetToClose(NativeSearchAccessibilityIds.ApplySort);
        AssertVisibleResultsArePriceAscending();

        OpenFilterSheet();
        IWebElement filterOption = WaitForElement(new ByIosNSPredicate(
            "name BEGINSWITH 'NativeFlightResultsFilterOption-AIRLINE_NAME-' AND selected == false AND enabled == true"),
            ScreenTimeout);
        string filterIdentifier = filterOption.GetAttribute("name") ?? string.Empty;
        if (filterIdentifier.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(
                "The discovered filter option did not expose an accessibility identifier.");
        }
        string filterLabel = ReadAccessibleLabel(filterOption);
        filterOption.Click();
        TapByKey(NativeSearchAccessibilityIds.ApplyFilters);
        WaitForRefinementSheetToClose(NativeSearchAccessibilityIds.ApplyFilters);
        string carrierCode = filterIdentifier[(filterIdentifier.LastIndexOf('-') + 1)..];
        AssertEveryResultContainsOperatedCarrier(carrierCode, ReadAirlineName(filterLabel));

        return new ResultsRefinement(
            sortLabel,
            filterIdentifier,
            filterLabel,
            initialFilterLabel,
            baselineResultSignature);
    }

    public void AssertRefinementPersistsThenClear(ResultsRefinement refinement)
    {
        AssertSortOptionSelected(NativeSearchAccessibilityIds.PriceSortOption);
        OpenFilterSheet();
        IWebElement selectedFilter = WaitForElement(new ByIosNSPredicate(
            "type == 'XCUIElementTypeButton' AND selected == true"), ScreenTimeout);
        string selectedLabel = ReadAccessibleLabel(selectedFilter);
        if (!selectedLabel.Equals(refinement.FilterLabel, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Persisted filter mismatch. Expected '{refinement.FilterLabel}', found '{selectedLabel}'.");
        }

        TapByKey(NativeSearchAccessibilityIds.ClearFilters);
        Wait.Until(_ => !IsVisibleByKey(
            NativeSearchAccessibilityIds.ActiveFilterSummary, TimeSpan.FromMilliseconds(500)));
        AssertSortOptionSelected(NativeSearchAccessibilityIds.PriceSortOption);
    }

    public void AssertPriceSortOrdersResults()
    {
        ApplySort(NativeSearchAccessibilityIds.PriceSortOption);
        AssertVisibleResultsArePriceAscending();
    }

    public void RestoreDefaultRefinements()
    {
        OpenFilterSheet();
        // Clear may be disabled/no-op on a fresh search. Only clear selected options;
        // otherwise Apply closes the unchanged sheet before restoring the sort.
        string resetAction = FilterResetAction(Driver.PageSource);
        TapByKey(resetAction);
        WaitForRefinementSheetToClose(resetAction);

        TapByKey(NativeSearchAccessibilityIds.ResultsSort);
        IWebElement recommended = WaitForElement(new ByIosNSPredicate(
            $"name == '{NativeSearchAccessibilityIds.RecommendedSortOption}' AND enabled == true"), ScreenTimeout);
        if (!recommended.Selected)
        {
            recommended.Click();
        }
        TapByKey(NativeSearchAccessibilityIds.ApplySort);
        WaitForRefinementSheetToClose(NativeSearchAccessibilityIds.ApplySort);
    }

    public void AssertDepartureTimeSortAndRecommendedReset()
        => AssertSortAndRecommendedReset(
            NativeSearchAccessibilityIds.DepartureTimeSortOption,
            ReadDepartureMinutes,
            "departure time");

    public void AssertDurationSortAndRecommendedReset()
        => AssertSortAndRecommendedReset(
            NativeSearchAccessibilityIds.DurationSortOption,
            ReadDurationMinutes,
            "duration");

    public void AssertSortSwitchingPreservesSelectionAndRecommendedReset()
    {
        string baselineResultSignature = ReadResultSignature();
        foreach (string sortOption in new[]
                 {
                     NativeSearchAccessibilityIds.PriceSortOption,
                     NativeSearchAccessibilityIds.DepartureTimeSortOption,
                     NativeSearchAccessibilityIds.DurationSortOption,
                 })
        {
            ApplySort(sortOption);
            AssertSortOptionSelected(sortOption);
        }

        ApplySort(NativeSearchAccessibilityIds.RecommendedSortOption);
        AssertSortOptionSelected(NativeSearchAccessibilityIds.RecommendedSortOption);
        Wait.Until(_ => ReadResultSignature().Equals(baselineResultSignature, StringComparison.Ordinal));
    }

    public void AssertResultsRouteContains(string origin, string destination)
    {
        IWebElement route = WaitForElementByKey(
            NativeSearchAccessibilityIds.ResultsAirportRoute,
            ScreenTimeout);
        AssertResultsRouteOrder(ReadElementTreeText(route), origin, destination);
    }

    internal static void AssertResultsRouteOrder(string routeText, string origin, string destination)
    {
        (string actualOrigin, string actualDestination) = ReadOrderedSummaryRoute(routeText);
        if (!actualOrigin.Is(origin) || !actualDestination.Is(destination))
        {
            throw new InvalidOperationException(
                $"Search results route did not match ordered route {origin}-{destination}. Actual: '{routeText}'.");
        }
    }

    public DateOnly SelectAnotherFlexibleDateAndAssertResultsRefresh(string origin, string destination)
    {
        AssertResultsRouteContains(origin, destination);
        WaitForElementByKey(NativeSearchAccessibilityIds.FlexibleDateCarousel, ScreenTimeout);
        IWebElement option = Wait.Until(_ =>
        {
            System.Drawing.Size viewport = Driver.Manage().Window.Size;
            return Driver.FindElements(new ByIosNSPredicate(
                    $"type == 'XCUIElementTypeButton' AND name BEGINSWITH " +
                    $"'{NativeSearchAccessibilityIds.FlexibleDatePrefix}' AND enabled == true"))
                .Where(element => !IsFlexibleDateSelected(element))
                .Where(element =>
                    element.Location.X >= 0 &&
                    element.Location.Y >= 0 &&
                    element.Location.X + element.Size.Width <= viewport.Width &&
                    element.Location.Y + element.Size.Height <= viewport.Height)
                .OrderBy(element => Math.Abs(
                    (element.Location.X + (element.Size.Width / 2)) - (viewport.Width / 2)))
                .FirstOrDefault();
        });
        string optionIdentifier = option.GetAttribute("name") ?? string.Empty;
        if (optionIdentifier.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(
                "The flexible-date option did not expose its app-owned accessibility identifier.");
        }

        DateOnly selectedDate = ParseFlexibleDateIdentifier(optionIdentifier);

        option.Click();
        if (!WaitForFlexibleDateSelected(optionIdentifier, TimeSpan.FromSeconds(3)))
        {
            HealingPolicy.ExecuteRecovery(
                MobileHealingType.RerenderRetry,
                $"Select flexible date '{optionIdentifier}'",
                attempt: 1,
                maximumAttempts: 1,
                "SwiftUI may rebuild the flexible-date carousel or consume its first accessibility action. " +
                "The selected date and refreshed results are still verified after the single retry.",
                () =>
                {
                    IWebElement retryOption = WaitForElementByKey(optionIdentifier, ScreenTimeout);
                    Driver.ExecuteScript("mobile: tap", new Dictionary<string, object>
                    {
                        ["x"] = retryOption.Location.X + (retryOption.Size.Width / 2),
                        ["y"] = retryOption.Location.Y + (retryOption.Size.Height / 2),
                    });
                    if (!WaitForFlexibleDateSelected(optionIdentifier, TimeSpan.FromSeconds(10)))
                    {
                        throw new InvalidOperationException(
                            $"Flexible date '{optionIdentifier}' was not selected after one bounded retry. " +
                            $"Accessible carousel state: {ReadFlexibleDateAccessibilityEvidence()}");
                    }
                });
        }

        Wait.Until(_ => IsVisibleByKey(
            NativeSearchAccessibilityIds.ResultsReady,
            TimeSpan.FromMilliseconds(500)));

        AssertResultsRouteContains(origin, destination);
        AssertResultsSelectedDate(selectedDate);
        string refreshedSignature = ReadResultSignature();
        if (refreshedSignature.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(
                $"Flexible date '{optionIdentifier}' completed without exposing refreshed flight results.");
        }

        // Identical inventories and prices are valid on adjacent dates. The app-owned selected
        // state plus a non-empty result signature proves completion without asserting false change.
        return selectedDate;
    }

    public void AssertResultsSelectedDate(DateOnly expectedDate)
    {
        string expectedIdentifier = $"{NativeSearchAccessibilityIds.FlexibleDatePrefix}{expectedDate:yyyyMMdd}";
        try
        {
            Wait.Until(_ => Driver.FindElements(MobileBy.AccessibilityId(expectedIdentifier))
                .Any(IsFlexibleDateSelected));
        }
        catch (WebDriverTimeoutException exception)
        {
            string accessibilityEvidence = ReadFlexibleDateAccessibilityEvidence();
            throw new InvalidOperationException(
                $"Results did not expose flexible date {expectedDate:yyyy-MM-dd} as selected after refresh. " +
                $"Expected identifier: '{expectedIdentifier}'. Accessible carousel state: {accessibilityEvidence}",
                exception);
        }
    }

    private bool WaitForFlexibleDateSelected(string identifier, TimeSpan timeout)
    {
        OpenQA.Selenium.Support.UI.WebDriverWait selectionWait = new(Driver, timeout)
        {
            PollingInterval = TimeSpan.FromMilliseconds(200),
        };
        try
        {
            return selectionWait.Until(_ => Driver.FindElements(MobileBy.AccessibilityId(identifier))
                .Any(IsFlexibleDateSelected));
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    private static bool IsFlexibleDateSelected(IWebElement element)
        => element.Selected || element.GetAttribute("value").Is("1");

    private string ReadFlexibleDateAccessibilityEvidence()
    {
        try
        {
            return string.Join(" | ", XDocument.Parse(Driver.PageSource)
                .Descendants()
                .Where(element =>
                    (element.Attribute("name")?.Value ?? string.Empty)
                        .StartsWith(NativeSearchAccessibilityIds.FlexibleDatePrefix, StringComparison.Ordinal))
                .Select(element => string.Join(", ", new[]
                {
                    $"id={element.Attribute("name")?.Value ?? "<missing>"}",
                    $"selected={element.Attribute("selected")?.Value ?? "<missing>"}",
                    $"visible={element.Attribute("visible")?.Value ?? "<missing>"}",
                    $"label={element.Attribute("label")?.Value ?? "<missing>"}",
                    $"value={element.Attribute("value")?.Value ?? "<missing>"}",
                })));
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.Xml.XmlException or WebDriverException)
        {
            return $"Unable to read page source: {exception.GetType().Name}: {exception.Message}";
        }
    }

    private static DateOnly ParseFlexibleDateIdentifier(string identifier)
    {
        string value = identifier[NativeSearchAccessibilityIds.FlexibleDatePrefix.Length..];
        if (!DateOnly.TryParseExact(
                value,
                "yyyyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly date))
        {
            throw new InvalidOperationException(
                $"Flexible-date identifier '{identifier}' did not contain the expected yyyyMMdd date contract.");
        }

        return date;
    }

    public void AssertEveryAvailableAirlineFilterMatchesResults()
        => AssertEveryEnabledFilterOfTypeAffectsResults("FilterOption-AIRLINE_NAME-");

    public void AssertAvailableAirlineFilterSwitching()
    {
        IReadOnlyList<KeyValuePair<string, string>> filters = DiscoverEnabledFilters(
            "FilterOption-AIRLINE_NAME-", minimumCount: 2);
        ApplyOnlyFilter(filters[0].Key);
        SwitchAppliedFilter(filters[0].Key, filters[1].Key);
        AssertVisibleResultsMatchFilter(filters[1].Key, filters[1].Value);
    }

    public void AssertPriceSortPersistsWithAvailableAirlineFilter()
    {
        KeyValuePair<string, string> filter = DiscoverEnabledFilters(
            "FilterOption-AIRLINE_NAME-", minimumCount: 1)[0];
        AssertPriceSortOrdersResults();
        ApplyOnlyFilter(filter.Key);
        AssertVisibleResultsArePriceAscending();
        AssertVisibleResultsMatchFilter(filter.Key, filter.Value);
    }

    public void AssertClearRemovesAvailableAirlineFilterAndPreservesPriceSort()
    {
        KeyValuePair<string, string> filter = DiscoverEnabledFilters(
            "FilterOption-AIRLINE_NAME-", minimumCount: 1)[0];
        AssertClearRemovesFilterAndPreservesPriceSort(filter.Key);
    }

    public void AssertEnabledConnectionFiltersAffectResults()
        => AssertEveryEnabledFilterOfTypeAffectsResults("FilterOption-CONNECTIONS-");

    public void AssertEnabledDepartureTimeFiltersAffectResults()
        => AssertEveryEnabledFilterOfTypeAffectsResults("FilterOption-DEPARTURE_TIME-");

    public void AssertFilterOptionsCascadeAcrossGroups()
    {
        OpenFilterSheet();
        IReadOnlyDictionary<string, FilterOptionState> baseline = ReadFilterOptionStatesAcrossSheet();
        FilterOptionState? appliedFilter = null;

        foreach (FilterOptionState candidate in baseline.Values.Where(option => option.Enabled))
        {
            TapFilterOption(candidate.Identifier);
            WaitForFilterSelection(candidate.Identifier, expectedSelected: true);

            IReadOnlyDictionary<string, FilterOptionState> cascaded = ReadFilterOptionStatesAcrossSheet();
            bool anotherGroupChanged = cascaded.Values.Any(current =>
                baseline.TryGetValue(current.Identifier, out FilterOptionState? original) &&
                !FilterGroup(current.Identifier).Equals(FilterGroup(candidate.Identifier), StringComparison.Ordinal) &&
                original.Enabled != current.Enabled);
            if (anotherGroupChanged)
            {
                appliedFilter = candidate;
                break;
            }

            TapFilterOption(candidate.Identifier);
            WaitForFilterSelection(candidate.Identifier, expectedSelected: false);
        }

        if (appliedFilter is null)
        {
            throw new InvalidOperationException(
                "No enabled filter caused another filter group to recalculate its enabled options.");
        }

        TapByKey(NativeSearchAccessibilityIds.ApplyFilters);
        WaitForRefinementSheetToClose(NativeSearchAccessibilityIds.ApplyFilters);
        AssertVisibleResultsMatchFilter(appliedFilter.Identifier, appliedFilter.Label);
    }

    public void AssertVirginCabinNamesUseBroadGroups()
    {
        KeyValuePair<string, string> virginFilter = DiscoverEnabledFilters(
                "FilterOption-AIRLINE_NAME-", minimumCount: 1)
            .FirstOrDefault(filter => filter.Key.EndsWith("-VS", StringComparison.OrdinalIgnoreCase));
        if (!virginFilter.Key.IsNullOrWhiteSpace())
        {
            ApplyOnlyFilter(virginFilter.Key);
        }

        if (!TryValidateDisplayedCabinNames(partnerCabin: false))
        {
            throw new InvalidOperationException(
                "No Virgin-operated cabin card was available for naming validation.");
        }
    }

    public void AssertPartnerCabinNamesUseBackendFareNames()
    {
        IReadOnlyList<KeyValuePair<string, string>> partnerFilters = DiscoverEnabledFilters(
                "FilterOption-AIRLINE_NAME-", minimumCount: 2)
            .Where(filter => !filter.Key.EndsWith("-VS", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (partnerFilters.Count == 0)
        {
            throw new InvalidOperationException("No enabled non-Virgin airline filter was available.");
        }

        foreach (KeyValuePair<string, string> partnerFilter in partnerFilters)
        {
            ApplyOnlyFilter(partnerFilter.Key);
            if (TryValidateDisplayedCabinNames(partnerCabin: true))
            {
                return;
            }

            ClearAppliedFilters();
        }

        throw new InvalidOperationException(
            "No enabled partner-airline filter returned a selectable partner cabin card for naming validation. " +
            $"Tested: {string.Join(", ", partnerFilters.Select(filter => filter.Value))}.");
    }

    private bool TryValidateDisplayedCabinNames(bool partnerCabin)
    {
        IReadOnlyDictionary<string, string> resultCards = ReadFlightResultTextFromPageSource();
        int consecutiveMissingResults = 0;
        bool foundResult = false;
        for (int flightIndex = 0; flightIndex < 20; flightIndex++)
        {
            string resultIdentifier = NativeSearchAccessibilityIds.FlightResult(flightIndex);
            IWebElement? result = FindKeyWithScrolling(resultIdentifier, 3);
            if (result is null)
            {
                consecutiveMissingResults++;
                if (foundResult && consecutiveMissingResults >= 3)
                {
                    break;
                }

                continue;
            }

            foundResult = true;
            consecutiveMissingResults = 0;

            string resultText = resultCards.TryGetValue(resultIdentifier, out string? text)
                ? text
                : ReadElementTreeText(result);
            string? operatingCarrierCode = ReadOperatingCarrierCode(resultIdentifier, resultText);
            if (operatingCarrierCode is null)
            {
                continue;
            }

            bool isPartnerOperated = !operatingCarrierCode.Equals("VS", StringComparison.OrdinalIgnoreCase);
            if (isPartnerOperated != partnerCabin)
            {
                continue;
            }

            IReadOnlyList<IWebElement> cabinButtons = Driver.FindElements(new ByIosNSPredicate(
                    $"type == 'XCUIElementTypeButton' AND name BEGINSWITH 'NativeFlightResult{flightIndex}Cabin'"))
                .Where(element => element.Displayed)
                .ToList();
            if (cabinButtons.Count == 0)
            {
                continue;
            }

            if (!partnerCabin)
            {
                string[] supportedNames = ["Economy", "Delight", "Premium", "Upper Class"];
                foreach (IWebElement cabin in cabinButtons)
                {
                    string label = ReadAccessibleLabel(cabin);
                    if (!supportedNames.Any(name => label.Contains(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new InvalidOperationException($"Virgin cabin exposed an unexpected name: '{label}'.");
                    }
                }
            }
            else
            {
                foreach (IWebElement cabin in cabinButtons)
                {
                    string identifier = cabin.GetAttribute("name") ?? string.Empty;
                    string label = ReadAccessibleLabel(cabin);
                    string backendCabinName = ReadPartnerCabinNameFromIdentifier(identifier);
                    if (!IsPartnerCabinIdentifier(identifier) ||
                        backendCabinName.IsNullOrWhiteSpace() ||
                        !NormalizeCabinName(label).Contains(backendCabinName, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"Cabin for operating carrier '{operatingCarrierCode}' did not expose its backend " +
                            $"fare name. Identifier='{identifier}', label='{label}'.");
                    }
                }
            }

            return true;
        }

        return false;
    }

    private static bool IsPartnerCabinIdentifier(string identifier)
        => identifier.Contains("CabinPartnerEconomy", StringComparison.Ordinal) ||
           identifier.Contains("CabinPartnerDelight", StringComparison.Ordinal) ||
           identifier.Contains("CabinPartnerPremium", StringComparison.Ordinal) ||
           identifier.Contains("CabinPartnerUpperClass", StringComparison.Ordinal) ||
           identifier.Contains("CabinOther", StringComparison.Ordinal);

    internal static string? ReadOperatingCarrierCode(string resultIdentifier, string resultText)
    {
        System.Text.RegularExpressions.Match match = RegexCompiled.Match(
            resultText,
            $@"\b{System.Text.RegularExpressions.Regex.Escape(resultIdentifier)}Carrier(?<code>[A-Z0-9]{{2,3}})\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups["code"].Value.ToUpperInvariant();
        }

        return null;
    }

    internal static (string Code, string Name) ReadOperatingCarrier(string pageSource, string resultIdentifier)
    {
        // The app-owned Carrier{code} ID wraps DetailedFlightRowInfoView, not only its
        // airline label. Until that view exposes a dedicated carrier-name ID, use its
        // English accessibility row: departure airport, airline name, arrival airport.
        // Fail closed on missing/ambiguous structure; never substitute the card label.
        List<(string Code, string Name)> carriers = [];
        // SwiftUI can propagate the parent ID onto sibling leaves instead of exposing
        // a container. Reassemble only the exact scoped identifier in document order.
        foreach (IGrouping<string, XElement> group in XDocument.Parse(pageSource).Descendants()
            .GroupBy(element => element.Attribute("name")?.Value ?? string.Empty))
        {
            string identifier = group.Key;
            System.Text.RegularExpressions.Match match = RegexCompiled.Match(identifier,
                $"^{System.Text.RegularExpressions.Regex.Escape(resultIdentifier)}Carrier(?<code>[A-Z0-9]{{2,3}})$");
            if (!match.Success)
            {
                continue;
            }

            string[] labels = group.SelectMany(element => element.DescendantsAndSelf()).Distinct()
                .Where(child => (child.Attribute("type")?.Value).Is("XCUIElementTypeStaticText"))
                .Select(child => child.Attribute("label")?.Value ?? child.Attribute("value")?.Value ?? string.Empty)
                .ToArray();
            for (int index = 0; index + 2 < labels.Length; index++)
            {
                if (RegexCompiled.IsMatch(labels[index], @"^Departure airport\s+[A-Z]{3}$") &&
                    RegexCompiled.IsMatch(labels[index + 2], @"^Arrival airport\s+[A-Z]{3}$") &&
                    !labels[index + 1].IsNullOrWhiteSpace())
                {
                    string name = RegexCompiled.Replace(labels[index + 1], @"\s*\+\s*\d+\s*$", string.Empty).Trim();
                    if (!name.IsNullOrWhiteSpace())
                    {
                        carriers.Add((match.Groups["code"].Value, name));
                    }
                }
            }
        }

        (string Code, string Name)[] distinct = carriers.Distinct().ToArray();
        return distinct.Length == 1 ? distinct[0] : throw new InvalidOperationException(
            $"Result '{resultIdentifier}' must expose one operating carrier in its app-owned carrier row; " +
            $"found {distinct.Length}. A dedicated carrier-name accessibility ID is required if the row structure changes.");
    }

    private static string ReadPartnerCabinNameFromIdentifier(string identifier)
    {
        string[] prefixes =
        [
            "CabinPartnerUpperClass",
            "CabinPartnerEconomy",
            "CabinPartnerDelight",
            "CabinPartnerPremium",
            "CabinOther",
        ];
        string? prefix = prefixes.FirstOrDefault(prefix => identifier.Contains(prefix, StringComparison.Ordinal));
        return prefix is null
            ? string.Empty
            : NormalizeCabinName(identifier[(identifier.IndexOf(prefix, StringComparison.Ordinal) + prefix.Length)..]);
    }

    private static string NormalizeCabinName(string value)
        => new(value.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    public void AssertVirginCabinOffersUpgradeToNextAvailableCabin()
    {
        IReadOnlyDictionary<string, string> resultCards = ReadFlightResultTextFromPageSource();
        foreach ((string resultIdentifier, string resultText) in resultCards.OrderBy(item => ParseFlightIndex(item.Key)))
        {
            if (!resultText.Contains("Virgin Atlantic", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int flightIndex = ParseFlightIndex(resultIdentifier);
            IWebElement? result = FindKeyWithScrolling(resultIdentifier, 3);
            IReadOnlyList<IWebElement> cabins = result is null ? [] : Driver.FindElements(new ByIosNSPredicate(
                    $"type == 'XCUIElementTypeButton' AND name BEGINSWITH 'NativeFlightResult{flightIndex}Cabin'"))
                .Where(element => element.Displayed && element.Enabled)
                .ToList();
            if (cabins.Count < 2)
            {
                continue;
            }

            cabins[0].Click();
            WaitForElementByKey(NativeSearchAccessibilityIds.FareSelectorReady, ScreenTimeout);
            string initialCabin = ReadSelectedFareSelectorCabinIdentifier();
            string expectedUpgradeCabin = ReadNextFareSelectorCabinIdentifier(initialCabin);
            IWebElement upgradeAction = FindUpgradeViewFaresAction()
                ?? throw new NoSuchElementException("The Virgin fare selector did not expose an upgrade card.");
            Driver.ExecuteScript("mobile: tap", new Dictionary<string, object>
            {
                ["x"] = upgradeAction.Location.X + (upgradeAction.Size.Width / 2),
                ["y"] = upgradeAction.Location.Y + (upgradeAction.Size.Height / 2),
            });
            WaitForElement(new ByIosNSPredicate(
                $"type == 'XCUIElementTypeButton' AND name == '{expectedUpgradeCabin}' AND selected == true"),
                ScreenTimeout);
            return;
        }

        throw new InvalidOperationException(
            "No Virgin Atlantic result with at least two available cabins was found for upgrade validation.");
    }

    public void AssertFareDismissalPreservesResults()
    {
        WaitForResults();
        string cardId = NativeSearchAccessibilityIds.FlightResult(0);
        IReadOnlyDictionary<string, string> before = ReadFlightResultTextFromPageSource();
        if (!before.TryGetValue(cardId, out string? originalCard) || originalCard.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("Fare dismissal needs observable first-result evidence.");
        }

        IWebElement cabin = Driver.FindElements(new ByIosNSPredicate(
                $"type == 'XCUIElementTypeButton' AND name BEGINSWITH '{cardId}Cabin'"))
            .FirstOrDefault(element => element.Displayed && element.Enabled)
            ?? throw new NoSuchElementException("The first result has no actionable cabin.");
        string cabinId = cabin.GetAttribute("name") ?? throw new InvalidOperationException("Cabin ID is missing.");
        if (!TryOpenFareSelector(cabinId))
        {
            throw new InvalidOperationException("The result cabin did not open a fare selector.");
        }

        string initialCabin = ReadSelectedFareSelectorCabinIdentifier();
        IWebElement alternative = Driver.FindElements(new ByIosNSPredicate(
                $"type == 'XCUIElementTypeButton' AND name BEGINSWITH '{NativeSearchAccessibilityIds.FareSelectorCabinPrefix}'"))
            .FirstOrDefault(element => element.Displayed && element.Enabled && element.GetAttribute("name") != initialCabin)
            ?? throw new InvalidOperationException("Fare dismissal requires two displayed, selectable cabins.");
        string alternativeId = alternative.GetAttribute("name") ?? throw new InvalidOperationException("Alternative cabin ID is missing.");
        alternative.Click();
        Wait.Until(_ => ReadSelectedFareSelectorCabinIdentifier() == alternativeId);
        TapByKey(NativeSearchAccessibilityIds.FareSelectorDismiss);
        Wait.Until(_ => !Driver.FindElements(MobileBy.AccessibilityId(NativeSearchAccessibilityIds.FareSelectorReady))
            .Any(element => element.Displayed));
        WaitForResults();
        Wait.Until(_ => ReadFlightResultTextFromPageSource().TryGetValue(cardId, out string? observed) &&
            observed == originalCard);

        if (!TryOpenFareSelector(cabinId))
        {
            throw new InvalidOperationException("The original cabin could not be reopened after cancellation.");
        }
        Wait.Until(_ => ReadSelectedFareSelectorCabinIdentifier() == initialCabin);
        TapByKey(NativeSearchAccessibilityIds.FareSelectorDismiss);
        Wait.Until(_ => !Driver.FindElements(MobileBy.AccessibilityId(NativeSearchAccessibilityIds.FareSelectorReady))
            .Any(element => element.Displayed));
        WaitForResults();
    }

    private void AssertClearRemovesFilterAndPreservesPriceSort(string filterIdentifier)
    {
        AssertPriceSortOrdersResults();
        string baselineResultSignature = ReadResultSignature();
        ApplyOnlyFilter(filterIdentifier);
        OpenFilterSheet();
        TapByKey(NativeSearchAccessibilityIds.ClearFilters);
        Wait.Until(_ => !IsVisibleByKey(
            NativeSearchAccessibilityIds.ActiveFilterSummary, TimeSpan.FromMilliseconds(500)));
        AssertSortOptionSelected(NativeSearchAccessibilityIds.PriceSortOption);
        AssertVisibleResultsArePriceAscending();
        Wait.Until(_ => ReadResultSignature().Equals(baselineResultSignature, StringComparison.Ordinal));
    }

    public string ReadPriceContext()
    {
        IWebElement context = WaitForElementByKey(NativeSearchAccessibilityIds.PriceContext, ScreenTimeout);
        return $"{context.GetAttribute("label")} {context.GetAttribute("value")} {context.Text}".Trim();
    }

    public void AssertVisibleResultPricesUseCurrency(string expectedCurrencySymbol)
    {
        IReadOnlyDictionary<string, string> resultCards = ReadFlightResultTextFromPageSource();
        if (resultCards.Count == 0)
        {
            throw new InvalidOperationException(
                "No flight-result cards were available for the currency assertion.");
        }

        string[] displayedCurrencies = resultCards.Values
            .SelectMany(text => System.Text.RegularExpressions.Regex.Matches(text, @"[$£€]\s?\d")
                .Select(match => match.Value[..1]))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (displayedCurrencies.Length == 0)
        {
            throw new InvalidOperationException(
                $"Flight results exposed no recognisable currency symbol. Results: {string.Join(" | ", resultCards.Values)}");
        }

        if (displayedCurrencies.Any(currency => currency != expectedCurrencySymbol))
        {
            throw new InvalidOperationException(
                $"Expected all flight-result prices to use '{expectedCurrencySymbol}', but observed: " +
                string.Join(", ", displayedCurrencies));
        }
    }

    public void SelectFare(string fareName, bool expectFlightSummary, int flightIndex = 0, int fareIndex = 0)
        => SelectFareAndReadLabel(fareName, expectFlightSummary, flightIndex, fareIndex);

    public int SelectRewardFare(
        string fareName,
        bool expectFlightSummary,
        int flightIndex = 0,
        int fareIndex = 0)
    {
        string fareLabel = SelectFareAndReadLabel(
            fareName,
            expectFlightSummary,
            flightIndex,
            fareIndex);
        return ReadRewardPoints(fareLabel);
    }

    internal static int ReadRewardPoints(string fareLabel)
    {
        System.Text.RegularExpressions.Match points = System.Text.RegularExpressions.Regex.Match(
            fareLabel,
            @"(?<sign>[+-]?)(?<points>\d[\d,]*)\s+points",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!points.Success ||
            !int.TryParse(
                points.Groups["points"].Value.Replace(",", string.Empty),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int displayedPoints))
        {
            throw new InvalidOperationException(
                $"The reward fare exposed no readable points value: {fareLabel}");
        }

        return points.Groups["sign"].Value == "-" ? -displayedPoints : displayedPoints;
    }

    public InsufficientPointsSummary ReadInsufficientPointsSummary()
    {
        IWebElement warning = WaitForElementByKey(
            NativeSearchAccessibilityIds.SummaryInsufficientPoints,
            ScreenTimeout);
        IWebElement continueButton = WaitForElementByKey(
            NativeSearchAccessibilityIds.SummaryContinueButton,
            ScreenTimeout);
        return new InsufficientPointsSummary(
            ReadElementTreeText(warning),
            continueButton.Enabled);
    }

    private string SelectFareAndReadLabel(
        string fareName,
        bool expectFlightSummary,
        int flightIndex,
        int fareIndex)
    {
        string cabinSuffix = fareName.Contains("Upper", StringComparison.OrdinalIgnoreCase)
            ? "UpperClass"
            : "Economy";
        string cabinKey = NativeSearchAccessibilityIds.FlightCabin(flightIndex, cabinSuffix);

        IWebElement? cabinButton = FindKeyWithScrolling(cabinKey, 5);
        if (cabinButton is null)
        {
            throw new NoSuchElementException($"No visible {fareName} cabin was found in the flight results.");
        }

        cabinButton.Click();
        IWebElement fareButton = WaitForElementByKey(
            NativeSearchAccessibilityIds.FareSelectButton(fareIndex),
            ScreenTimeout);
        string fareLabel = ReadAccessibleLabel(fareButton);
        fareButton.Click();

        if (expectFlightSummary)
        {
            WaitForFlightSummary();
        }
        else
        {
            WaitForElementByKey(NativeSearchAccessibilityIds.RunningPriceSummary, ScreenTimeout);
            WaitForElementByKey(NativeSearchAccessibilityIds.FlightResult(0), ScreenTimeout);
        }

        return fareLabel;
    }

    private static bool IsToggleSelected(IWebElement toggle)
    {
        return IsSelectedState(toggle.Selected, toggle.GetAttribute("value"));
    }

    private static bool IsSelectedState(bool selected, string? value)
        => selected || value.Is("1") || value.Is("true");

    private static bool IsRewardSearchSelected(IWebElement toggle)
    {
        IWebElement? nativeSwitch = FindRewardSearchSwitch(toggle);
        IEnumerable<IWebElement> candidates = nativeSwitch is null
            ? [toggle]
            : [toggle, nativeSwitch];

        return candidates.Any(IsToggleSelected);
    }

    private static IWebElement? FindRewardSearchSwitch(IWebElement control)
        => control.FindElements(new ByIosClassChain("**/XCUIElementTypeSwitch")).FirstOrDefault();

    internal static string FilterResetAction(string pageSource)
    {
        bool selected = XDocument.Parse(pageSource).Descendants().Any(element =>
            (element.Attribute("name")?.Value ?? string.Empty).StartsWith(
                NativeSearchAccessibilityIds.FilterOptionPrefix, StringComparison.Ordinal) &&
            IsSelectedState((element.Attribute("selected")?.Value).Is("true"), element.Attribute("value")?.Value));
        return selected ? NativeSearchAccessibilityIds.ClearFilters : NativeSearchAccessibilityIds.ApplyFilters;
    }

    internal static (string Origin, string Destination) ResolveSummaryRoute(
        string itineraryText, string? legacyOrigin, string? legacyDestination)
        => legacyOrigin is not null && legacyDestination is not null
            ? (legacyOrigin, legacyDestination)
            : ReadOrderedSummaryRoute(itineraryText);

    public FlightSummary ReadSummary(int itineraryIndex = 0)
    {
        IWebElement? fare = itineraryIndex == 0
            ? TryWaitForElementByKey(NativeSearchAccessibilityIds.SummaryItinerary(0), TimeSpan.FromSeconds(1))
            : FindKeyWithScrolling(NativeSearchAccessibilityIds.SummaryItinerary(itineraryIndex), 3);
        string itineraryText = fare is null ? string.Empty : ReadElementTreeText(fare);
        IWebElement? legacyOrigin = itineraryIndex != 0 ? null : TryWaitForElementByKey(
            "NativeFlightSummaryContext0Button",
            TimeSpan.FromSeconds(1));
        IWebElement? legacyDestination = itineraryIndex != 0 ? null : TryWaitForElementByKey(
            "NativeFlightSummaryContext1Button",
            TimeSpan.FromSeconds(1));
        (string originText, string destinationText) = ResolveSummaryRoute(
            itineraryText,
            legacyOrigin is null ? null : ReadSummaryRow(legacyOrigin),
            legacyDestination is null ? null : ReadSummaryRow(legacyDestination));

        string fareText = itineraryText;

        IWebElement? priceButton = TryWaitForElementByKey(
            NativeSearchAccessibilityIds.SummaryPriceButton,
            TimeSpan.FromSeconds(2));
        IWebElement continueButton = WaitForElementByKey(
            NativeSearchAccessibilityIds.SummaryContinueButton,
            ScreenTimeout);
        return new FlightSummary(
            originText,
            destinationText,
            fareText,
            itineraryText,
            priceButton is null ? string.Empty : ReadElementTreeText(priceButton),
            continueButton.Enabled);
    }

    public void AssertFlightSummaryMatchesSelection(
        FlightSearch search,
        SelectedFlight selected,
        bool expectVirginOperatingCarrier,
        int itineraryIndex = 0)
    {
        FlightSummary summary = ReadSummary(itineraryIndex);
        string summaryText = NormalizeCabinName($"{summary.ItineraryText} {summary.PriceText}");
        AssertSummaryRouteAndFare(summary, search.Origin, search.Destination, selected.CabinName);

        string expectedCarrier = NormalizeCabinName(selected.OperatingCarrierName);
        bool carrierDisplayed = !expectedCarrier.IsNullOrWhiteSpace() && summaryText.Contains(expectedCarrier);
        if (!carrierDisplayed)
        {
            throw new InvalidOperationException(
                $"Flight Summary did not display operating carrier '{selected.OperatingCarrierName}' " +
                $"({selected.OperatingCarrierCode}). Itinerary='{summary.ItineraryText}'.");
        }

        if (expectVirginOperatingCarrier !=
            selected.OperatingCarrierCode.Equals("VS", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Selected operating carrier '{selected.OperatingCarrierCode}' did not match the scenario.");
        }

        AssertSummaryScheduleAndPrice(search, selected, summary);
    }

    internal static void AssertSummaryRouteAndFare(
        FlightSummary summary, string origin, string destination, string cabinName)
    {
        if (!summary.OriginText.Contains(origin, StringComparison.OrdinalIgnoreCase) ||
            !summary.DestinationText.Contains(destination, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Flight Summary did not retain route {origin}-{destination}. " +
                $"Itinerary='{summary.ItineraryText}'.");
        }

        if (cabinName.IsNullOrWhiteSpace() || !summary.FareText.Contains(cabinName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Flight Summary did not retain selected cabin/fare '{cabinName}'. Observed='{summary.FareText}'.");
        }

    }

    private static void AssertSummaryScheduleAndPrice(FlightSearch search, SelectedFlight selected, FlightSummary summary)
    {
        string expectedDay = search.DepartureDate.Day.ToString(CultureInfo.InvariantCulture);
        string expectedMonth = search.DepartureDate.ToString("MMM", CultureInfo.InvariantCulture);
        if (!summary.ItineraryText.Contains(expectedDay, StringComparison.OrdinalIgnoreCase) ||
            !summary.ItineraryText.Contains(expectedMonth, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Flight Summary did not display departure date {search.DepartureDate:dd MMM yyyy}. " +
                $"Itinerary='{summary.ItineraryText}'.");
        }

        string[] resultTimes = System.Text.RegularExpressions.Regex.Matches(
                selected.ResultText,
                @"\b\d{1,2}:\d{2}\b")
            .Select(match => match.Value)
            .Distinct(StringComparer.Ordinal)
            .Take(2)
            .ToArray();
        if (resultTimes.Length < 2 || resultTimes.Any(time =>
                !summary.ItineraryText.Contains(time, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Flight Summary did not retain the selected departure and arrival times. " +
                $"Expected='{string.Join(", ", resultTimes)}', itinerary='{summary.ItineraryText}'.");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(summary.PriceText, @"[$£€]\s?\d") ||
            !summary.ContinueEnabled)
        {
            throw new InvalidOperationException(
                $"Flight Summary did not expose an actionable total price and Continue control. " +
                $"Price='{summary.PriceText}', continueEnabled={summary.ContinueEnabled}.");
        }
    }

    internal static (string Origin, string Destination) ReadOrderedSummaryRoute(string itineraryText)
    {
        // Temporary English route-header contract inside SummaryItinerary{index}.
        // Replace with dedicated origin/destination IDs when supplied by the app.
        const string airport = @"(?:London Heathrow|Heathrow Airport|London|New York JFK|New York|John F Kennedy|Manchester|Lagos|LHR|JFK|MAN|LOS)";
        System.Text.RegularExpressions.Match match = RegexCompiled.Match(itineraryText,
            $@"\b(?<origin>{airport})\s*(?:\bto\b|→|-)\s*(?<destination>{airport})\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            throw new InvalidOperationException(
                "Flight UI did not expose a supported ordered route header; origin/destination IDs are required.");
        }

        return (SummaryAirportCode(match.Groups["origin"].Value), SummaryAirportCode(match.Groups["destination"].Value));
    }

    private static string SummaryAirportCode(string airport)
        => airport.ToUpperInvariant() switch
        {
            "LONDON HEATHROW" or "HEATHROW AIRPORT" or "LONDON" or "LHR" => "LHR",
            "NEW YORK JFK" or "NEW YORK" or "JOHN F KENNEDY" or "JFK" => "JFK",
            "MANCHESTER" or "MAN" => "MAN",
            "LAGOS" or "LOS" => "LOS",
            _ => throw new InvalidOperationException($"Unsupported summary airport '{airport}'."),
        };

    private void SelectSearchCriteria(FlightSearch search, string journeyType)
    {
        SelectAirport(
            NativeSearchAccessibilityIds.FlyingFromButton,
            NativeSearchAccessibilityIds.OriginAirportSearchField,
            NativeSearchAccessibilityIds.OriginPrefix,
            search.Origin);
        SelectAirport(
            NativeSearchAccessibilityIds.FlyingToButton,
            NativeSearchAccessibilityIds.DestinationAirportSearchField,
            NativeSearchAccessibilityIds.DestinationPrefix,
            search.Destination);

        WaitForElementByKey(NativeSearchAccessibilityIds.JourneyTypePicker, ScreenTimeout);
        WaitForElement(new ByIosNSPredicate(
            $"type == 'XCUIElementTypeButton' AND (label == '{journeyType}' OR name == '{journeyType}')"),
            ScreenTimeout).Click();
    }

    private void SelectAirport(
        string buttonId,
        string searchFieldId,
        string resultPrefix,
        string airportCode)
    {
        TapByKey(buttonId);
        IWebElement searchField = WaitForAirportSearchInput(searchFieldId);
        searchField.Clear();
        searchField.SendKeys(airportCode);
        TapByKey($"{resultPrefix}{airportCode.ToUpperInvariant()}");
    }

    private IWebElement WaitForAirportSearchInput(string accessibilityId)
        => Wait.Until(_ =>
        {
            List<AppiumElement> identified = Driver.FindElements(MobileBy.AccessibilityId(accessibilityId))
                .Where(element => element.Displayed && element.Enabled).ToList();
            if (identified.Count > 0)
            {
                return identified.Count == 1 ? identified[0] : null;
            }

            // Interim iOS .searchable contract: accept exactly one visible enabled field
            // when the app ID is absent. Never use this interaction fallback for Figma crops.
            List<AppiumElement> fields = Driver.FindElements(new ByIosClassChain("**/XCUIElementTypeSearchField"))
                .Where(element => element.Displayed && element.Enabled).ToList();
            return fields.Count == 1 ? fields[0] : null;
        });

    public void SelectDates(DateOnly departureDate, DateOnly? returnDate = null)
    {
        TapByKey(NativeSearchAccessibilityIds.WhenButton);
        if (!returnDate.HasValue)
        {
            string existingDeparture = ReadControlText(
                NativeSearchAccessibilityIds.CalendarDepartureDateValue);
            if (!IsExpectedDate(existingDeparture, departureDate))
            {
                SelectCalendarDay(departureDate);
                WaitForSelectedDate(
                    "departure",
                    NativeSearchAccessibilityIds.CalendarDepartureDateValue,
                    departureDate);
            }
        }
        else
        {
            CalendarDateSelection existingSelection = ReadCalendarSelection();
            bool requestedRangeAlreadySelected =
                IsExpectedDate(existingSelection.DepartureText, departureDate) &&
                IsExpectedDate(existingSelection.ReturnText, returnDate.Value);
            if (!requestedRangeAlreadySelected)
            {
                NormalizeRangeDeparture(departureDate);
                SelectCalendarDay(returnDate.Value);
                WaitForSelectedDate("return", NativeSearchAccessibilityIds.CalendarReturnDateValue, returnDate.Value);
            }
        }

        AssertCalendarSelection(departureDate, returnDate);
        ConfirmDateSelection();
    }

    private void NormalizeRangeDeparture(DateOnly departureDate)
    {
        // iOS select(_:): a pending earlier departure first completes a range; a second
        // tap starts a new range. Only tap the requested date, never an invented anchor.
        const int maximumTransitions = 2;
        for (int transition = 0; transition < maximumTransitions; transition++)
        {
            CalendarDateSelection current = ReadCalendarSelection();
            if (IsExpectedDate(current.DepartureText, departureDate) &&
                IsEmptyCalendarSelection(current.ReturnText))
            {
                return;
            }

            SelectCalendarDay(departureDate);
            OpenQA.Selenium.Support.UI.WebDriverWait transitionWait = new(
                Driver,
                TimeSpan.FromSeconds(4))
            {
                PollingInterval = TimeSpan.FromMilliseconds(200),
            };
            CalendarDateSelection updatedSelection = transitionWait.Until(_ =>
            {
                CalendarDateSelection updated = ReadCalendarSelection();
                return updated != current &&
                    (IsExpectedDate(updated.DepartureText, departureDate) ||
                     IsExpectedDate(updated.ReturnText, departureDate)) ? updated : null;
            });
            if (IsExpectedDate(updatedSelection.DepartureText, departureDate) &&
                IsEmptyCalendarSelection(updatedSelection.ReturnText))
            {
                return;
            }
        }

        CalendarDateSelection finalSelection = ReadCalendarSelection();
        throw new InvalidOperationException(
            "The calendar could not normalise its retained range before selecting the return date. " +
            $"Expected departure {departureDate:yyyy-MM-dd} with an empty return; " +
            $"found '{finalSelection.DepartureText}' and '{finalSelection.ReturnText}'.");
    }

    private void WaitForSelectedDate(string field, string accessibilityId, DateOnly expected)
    {
        string actual = string.Empty;
        OpenQA.Selenium.Support.UI.WebDriverWait selectionWait = new(
            Driver,
            TimeSpan.FromSeconds(4))
        {
            PollingInterval = TimeSpan.FromMilliseconds(200),
        };
        try
        {
            selectionWait.Until(_ =>
            {
                actual = ReadControlText(accessibilityId);
                return IsExpectedDate(actual, expected);
            });
        }
        catch (WebDriverTimeoutException)
        {
            AssertSelectedDate(
                field,
                actual,
                expected);
        }
    }

    private void SelectCalendarDay(DateOnly date)
    {
        CalendarDateSelection beforeNavigation = ReadCalendarScrollSelection();
        string dayKey = NativeSearchAccessibilityIds.CalendarDay(date);
        IWebElement? dayElement = FindCalendarDayWithScrolling(date, 12);
        if (dayElement is null)
        {
            throw new NoSuchElementException(
                $"Date {date:yyyy-MM-dd} was not displayed in the native calendar. " +
                $"Visible calendar buttons: {DescribeVisibleCalendarButtons()}.");
        }

        // FindCalendarDayWithScrolling has already placed the target safely above the
        // fixed footer. A second scrollToElement can make SwiftUI recycle the lazy-grid
        // button between lookup and tap, so re-resolve the stable accessibility ID only.
        IWebElement settledDay = WaitForCalendarDayToSettle(dayKey);
        // A delayed SwiftUI selection update can arrive after PerformActions returns.
        // Compare with the pre-navigation baseline, not a new post-scroll baseline.
        AssertCalendarSelectionUnchanged(beforeNavigation, ReadCalendarScrollSelection());
        settledDay.Click();
    }

    private IWebElement WaitForCalendarDayToSettle(string dayKey)
    {
        // Remote observations include several WebDriver round trips. Three seconds
        // can expire before even the first observation completes on BrowserStack.
        // Use the bounded screen budget; successful observations still return early.
        System.Drawing.Rectangle? previousBounds = null;
        int stableObservations = 0;
        OpenQA.Selenium.Support.UI.WebDriverWait stableWait = new(
            Driver,
            ScreenTimeout)
        {
            PollingInterval = TimeSpan.FromMilliseconds(200),
            Message = $"Calendar date '{dayKey}' did not settle inside the unobscured viewport after two matching observations; no date was tapped.",
        };

        return stableWait.Until(_ =>
        {
            try
            {
                (int actionTop, int actionBoundary) = ReadCalendarActionBounds();
                IWebElement[] candidates = Driver.FindElements(MobileBy.AccessibilityId(dayKey))
                    .Where(IsDisplayed)
                    .ToArray();
                if (candidates.Length != 1)
                {
                    previousBounds = null;
                    stableObservations = 0;
                    return null;
                }

                IWebElement candidate = candidates[0];
                System.Drawing.Rectangle bounds = new(candidate.Location, candidate.Size);
                if (!candidate.Enabled || !CalendarDayFits(bounds, actionTop, actionBoundary))
                {
                    previousBounds = null;
                    stableObservations = 0;
                    return null;
                }

                stableObservations = previousBounds == bounds ? stableObservations + 1 : 1;
                previousBounds = bounds;
                return stableObservations >= 2 ? candidate : null;
            }
            catch (StaleElementReferenceException)
            {
                // Lazy-grid recycling invalidates the observation, not the date ID.
                // Re-resolve on the next poll within the same fixed wait budget.
                previousBounds = null;
                stableObservations = 0;
                return null;
            }
        });
    }

    private IWebElement? FindCalendarDayWithScrolling(DateOnly date, int maximumScrolls)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        int monthsAhead = Math.Max(0, ((date.Year - today.Year) * 12) + date.Month - today.Month);
        maximumScrolls = Math.Max(maximumScrolls, (monthsAhead * 2) + 4);

        string dayKey = NativeSearchAccessibilityIds.CalendarDay(date);
        WaitForElementByKey(
            NativeSearchAccessibilityIds.CalendarScrollView,
            ScreenTimeout);
        bool? previousDirection = null;
        int gestureDivisor = 1;
        for (int attempt = 0; attempt <= maximumScrolls; attempt++)
        {
            (int actionTop, int calendarActionBoundary) = ReadCalendarActionBounds();
            IWebElement? day = Driver.FindElements(MobileBy.AccessibilityId(dayKey))
                .Where(IsDisplayed)
                .FirstOrDefault();
            if (day is not null && CalendarDayFits(
                new System.Drawing.Rectangle(day.Location, day.Size), actionTop, calendarActionBoundary))
            {
                return day;
            }

            if (attempt == maximumScrolls)
            {
                break;
            }

            // LazyVGrid removes earlier days from the tree. Absence does not mean
            // "keep scrolling forward": compare the target with observed visible dates.
            bool targetIsAboveViewport = day is not null
                ? day.Location.Y < actionTop
                : CalendarTargetPrecedesVisibleDates(date, Driver.PageSource, actionTop, calendarActionBoundary);
            if (previousDirection.HasValue && previousDirection.Value != targetIsAboveViewport)
            {
                gestureDivisor = Math.Min(8, gestureDivisor * 2);
            }
            ScrollCalendarThroughGutter(targetIsAboveViewport, gestureDivisor);
            previousDirection = targetIsAboveViewport;
        }

        return null;
    }

    private (int Top, int Bottom) ReadCalendarActionBounds()
    {
        IWebElement scroll = WaitForElementByKey(NativeSearchAccessibilityIds.CalendarScrollView, ScreenTimeout);
        IWebElement footer = WaitForElementByKey(NativeSearchAccessibilityIds.CalendarDepartureDateValue, ScreenTimeout);
        System.Drawing.Point scrollLocation = scroll.Location;
        int top = Math.Max(0, scrollLocation.Y) + 1;
        int bottom = Math.Min(Driver.Manage().Window.Size.Height,
            Math.Min(scrollLocation.Y + scroll.Size.Height, footer.Location.Y)) - 1;
        if (bottom <= top)
        {
            throw new InvalidOperationException("Calendar scroll viewport does not expose an unobscured day area.");
        }
        return (top, bottom);
    }

    internal static bool CalendarTargetPrecedesVisibleDates(DateOnly target, string source, int top, int bottom)
    {
        List<DateOnly> dates = [];
        foreach (XElement element in XDocument.Parse(source).Descendants())
        {
            string name = element.Attribute("name")?.Value ?? string.Empty;
            const string prefix = "NativeFlightCalendarDay_";
            if (element.Attribute("visible")?.Value != "true" ||
                !name.StartsWith(prefix, StringComparison.Ordinal) ||
                !DateOnly.TryParseExact(name[prefix.Length..], "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateOnly date) ||
                !double.TryParse(element.Attribute("y")?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double y) ||
                !double.TryParse(element.Attribute("height")?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double height))
            {
                continue;
            }
            double centre = y + height / 2;
            if (height > 0 && centre >= top && centre < bottom)
            {
                dates.Add(date);
            }
        }
        if (dates.Count == 0)
        {
            throw new InvalidOperationException("Calendar exposes no visible dated day IDs inside its scroll viewport; cannot infer scroll direction.");
        }
        return target < dates.Min();
    }

    private void ScrollCalendarThroughGutter(bool towardsEarlierDates, int gestureDivisor)
    {
        CalendarDateSelection before = ReadCalendarScrollSelection();
        IWebElement scroll = WaitForElementByKey(NativeSearchAccessibilityIds.CalendarScrollView, ScreenTimeout);
        (int top, int bottom) = ReadCalendarActionBounds();
        System.Drawing.Rectangle window = new(System.Drawing.Point.Empty, Driver.Manage().Window.Size);
        System.Drawing.Rectangle viewport = System.Drawing.Rectangle.Intersect(
            new System.Drawing.Rectangle(scroll.Location, scroll.Size), window);
        (int gutterX, int upperY, int lowerY) = CalendarGutter(
            Driver.PageSource, viewport, top, bottom);
        // XCTest's drag duration is a hold BEFORE movement. SwiftUI can interpret that
        // as a day press even outside its reported accessibility bounds. Move immediately;
        // the 350ms below is travel time, not a pause on the starting control.
        Driver.PerformActions([CalendarScrollGesture(gutterX, upperY, lowerY, towardsEarlierDates, gestureDivisor)]);
        CalendarDateSelection after = ReadCalendarScrollSelection();
        if (after != before)
        {
            throw new InvalidOperationException(
                "Calendar selection changed during a scroll-only gesture. Stopping before any further date tap. " +
                $"Before: '{before.DepartureText}' / '{before.ReturnText}'; " +
                $"after: '{after.DepartureText}' / '{after.ReturnText}'. " +
                $"Viewport {viewport}; gutter X={gutterX}, Y={upperY}..{lowerY}, earlier={towardsEarlierDates}.");
        }
    }

    internal static ActionSequence CalendarScrollGesture(int x, int upperY, int lowerY, bool earlier, int divisor = 1)
    {
        if (divisor is not (1 or 2 or 4 or 8) || lowerY <= upperY)
        {
            throw new ArgumentOutOfRangeException(nameof(divisor), "Calendar swipe geometry or correction scale is invalid.");
        }
        // Direction reversal means the prior movement overshot. Reduce travel and velocity
        // within the same measured gutter instead of oscillating with a full-size swipe.
        int inset = ((lowerY - upperY) - (lowerY - upperY) / divisor) / 2;
        upperY += inset;
        lowerY -= inset;
        OpenQA.Selenium.Appium.Interactions.PointerInputDevice finger = new(PointerKind.Touch);
        ActionSequence gesture = new(finger);
        gesture.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport,
            x, earlier ? upperY : lowerY, TimeSpan.Zero));
        gesture.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
        gesture.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport,
            x, earlier ? lowerY : upperY, TimeSpan.FromMilliseconds(350)));
        gesture.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));
        return gesture;
    }

    private CalendarDateSelection ReadCalendarScrollSelection()
        => new(ReadControlText(NativeSearchAccessibilityIds.CalendarDepartureDateValue),
            FindVisibleByAccessibilityId(NativeSearchAccessibilityIds.CalendarReturnDateValue) is null
                ? string.Empty // Single-date calendars deliberately have no return summary.
                : ReadControlText(NativeSearchAccessibilityIds.CalendarReturnDateValue));

    internal static void AssertCalendarSelectionUnchanged(CalendarDateSelection before, CalendarDateSelection after)
    {
        if (before != after)
        {
            throw new InvalidOperationException(
                "Calendar selection changed while navigating to the requested date. No date was tapped. " +
                $"Before: '{before.DepartureText}' / '{before.ReturnText}'; " +
                $"after: '{after.DepartureText}' / '{after.ReturnText}'.");
        }
    }

    internal static bool CalendarDayFits(System.Drawing.Rectangle bounds, int top, int bottom)
        => bounds.Width > 0 && bounds.Height > 0 && bounds.Top >= top && bounds.Bottom <= bottom;

    internal static (int X, int UpperY, int LowerY) CalendarGutter(
        string source, System.Drawing.Rectangle viewport, int top, int bottom)
    {
        top = Math.Max(top, viewport.Top);
        bottom = Math.Min(bottom, viewport.Bottom);
        List<System.Drawing.Rectangle> days = [];
        foreach (XElement element in XDocument.Parse(source).Descendants())
        {
            if (element.Attribute("visible")?.Value != "true" ||
                !(element.Attribute("name")?.Value ?? string.Empty).StartsWith(
                    "NativeFlightCalendarDay_", StringComparison.Ordinal))
            {
                continue;
            }
            int[] coordinates = new int[4];
            string[] attributes = ["x", "y", "width", "height"];
            for (int index = 0; index < attributes.Length; index++)
            {
                if (!int.TryParse(element.Attribute(attributes[index])?.Value,
                    NumberStyles.Integer, CultureInfo.InvariantCulture, out coordinates[index]))
                {
                    throw new InvalidOperationException("Calendar day geometry is unavailable; cannot prove a safe scroll gutter.");
                }
            }
            System.Drawing.Rectangle bounds = new(coordinates[0], coordinates[1], coordinates[2], coordinates[3]);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                throw new InvalidOperationException("Calendar day geometry is invalid.");
            }
            if (bounds.IntersectsWith(viewport) && bounds.Bottom > top && bounds.Top < bottom)
            {
                days.Add(bounds);
            }
        }
        if (days.Count == 0 || bottom - top < 80 || viewport.Width <= 0)
        {
            throw new InvalidOperationException("Calendar has no measurable safe scroll area.");
        }
        int left = days.Min(day => day.Left);
        int right = days.Max(day => day.Right);
        int x = left - viewport.Left >= 12 ? viewport.Left + (left - viewport.Left) / 2
            : viewport.Right - right >= 12 ? right + (viewport.Right - right) / 2
            : throw new InvalidOperationException("Calendar day buttons leave no safe horizontal scroll gutter.");
        int inset = Math.Max(12, (bottom - top) / 5);
        return (x, top + inset, bottom - inset);
    }

    private void AssertCalendarSelection(DateOnly departureDate, DateOnly? returnDate)
    {
        AssertSelectedDate(
            "departure",
            ReadControlText(NativeSearchAccessibilityIds.CalendarDepartureDateValue),
            departureDate);
        if (returnDate.HasValue)
        {
            AssertSelectedDate(
                "return",
                ReadControlText(NativeSearchAccessibilityIds.CalendarReturnDateValue),
                returnDate.Value);
        }
    }

    private static void AssertSelectedDate(string field, string actual, DateOnly expected)
    {
        if (IsExpectedDate(actual, expected))
        {
            return;
        }

        string expectedDay = expected.Day.ToString(CultureInfo.InvariantCulture);
        string expectedMonth = expected.ToDateTime(TimeOnly.MinValue)
            .ToString("MMMM", CultureInfo.InvariantCulture);
        throw new InvalidOperationException(
            $"The calendar selected the wrong {field} date. Expected day {expectedDay} " +
            $"and month {expectedMonth}, " +
            $"but the calendar summary showed '{actual}'.");
    }

    private static bool IsExpectedDate(string actual, DateOnly expected)
    {
        string expectedDay = expected.Day.ToString(CultureInfo.InvariantCulture);
        string expectedMonth = expected.ToDateTime(TimeOnly.MinValue)
            .ToString("MMMM", CultureInfo.InvariantCulture);
        return actual
                .Split([' ', ',', '.', '/', '-'], StringSplitOptions.RemoveEmptyEntries)
                .Contains(expectedDay, StringComparer.Ordinal) &&
            actual.Contains(expectedMonth, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEmptyCalendarSelection(string value)
        => value
            .Split([' ', ',', '.', '/'], StringSplitOptions.RemoveEmptyEntries)
            .Any(part => part == "-");

    private static bool IsDisplayed(IWebElement element)
    {
        try
        {
            return element.Displayed;
        }
        catch (StaleElementReferenceException)
        {
            return false;
        }
    }

    private IWebElement? FindVisibleByAccessibilityId(string accessibilityId)
    {
        foreach (IWebElement element in Driver.FindElements(MobileBy.AccessibilityId(accessibilityId)))
        {
            try
            {
                if (element.Displayed)
                {
                    return element;
                }
            }
            catch (StaleElementReferenceException)
            {
                // The calendar rebuilds its accessibility tree during a month scroll.
            }
        }

        return null;
    }

    private string DescribeVisibleCalendarButtons()
    {
        IEnumerable<string> descriptions = Driver
            .FindElements(By.XPath("//XCUIElementTypeButton"))
            .Where(element =>
            {
                try
                { return element.Displayed; }
                catch (StaleElementReferenceException) { return false; }
            })
            .Take(20)
            .Select(element =>
                $"name='{element.GetAttribute("name")}', label='{element.GetAttribute("label")}', value='{element.GetAttribute("value")}'");

        string result = string.Join(" | ", descriptions);
        return result.IsNullOrWhiteSpace() ? "none exposed" : result;
    }

    private void SelectPassengers(PassengerCounts passengers)
    {
        if (passengers.Adults < 1 || passengers.Infants > passengers.Adults || passengers.Total > 9)
        {
            throw new ArgumentOutOfRangeException(nameof(passengers), "Passenger counts violate the app rules.");
        }

        // The search form defaults to one adult. Avoid opening and reconfirming the
        // selector when the requested party already matches that app-owned default.
        if (passengers == PassengerCounts.OneAdult)
        {
            return;
        }

        OpenPassengerSelector();
        WaitForElementByKey(NativeSearchAccessibilityIds.PassengerSelectorNextButton, ScreenTimeout);
        SetPassengerCount(NativeSearchAccessibilityIds.AdultCount,
            NativeSearchAccessibilityIds.AdultIncrementButton,
            NativeSearchAccessibilityIds.AdultDecrementButton, passengers.Adults);
        SetPassengerCount(NativeSearchAccessibilityIds.TeenCount,
            NativeSearchAccessibilityIds.TeenIncrementButton,
            NativeSearchAccessibilityIds.TeenDecrementButton, passengers.Teens);
        SetPassengerCount(NativeSearchAccessibilityIds.ChildCount,
            NativeSearchAccessibilityIds.ChildIncrementButton,
            NativeSearchAccessibilityIds.ChildDecrementButton, passengers.Children);
        SetPassengerCount(NativeSearchAccessibilityIds.InfantCount,
            NativeSearchAccessibilityIds.InfantIncrementButton,
            NativeSearchAccessibilityIds.InfantDecrementButton, passengers.Infants);
        TapByKey(NativeSearchAccessibilityIds.PassengerSelectorNextButton);
    }

    public void ChangeSummaryFlight(int itineraryIndex = 0)
    {
        string identifier = NativeSearchAccessibilityIds.SummaryChange(itineraryIndex);
        IWebElement button = FindKeyWithScrolling(identifier, maximumSwipes: 3)
            ?? throw new NoSuchElementException($"Summary Change action '{identifier}' was not exposed.");
        button.Click();
        WaitForResults();
        WaitForElementByKey(NativeSearchAccessibilityIds.FlightResult(0), ResultsTimeout);
    }

    public SummaryUpgradeOffer ReadOfferedSummaryUpgrade()
    {
        // Read the delivered indexed action IDs, not a hard-coded cabin or translated button label.
        int[] indexes = XDocument.Parse(Driver.PageSource).Descendants()
            .Select(element => element.Attribute("name")?.Value ?? string.Empty)
            .Select(name => RegexCompiled.Match(name, @"^NativeFlightSummaryUpgrade(?<index>\d+)Button$"))
            .Where(match => match.Success)
            .Select(match => int.Parse(match.Groups["index"].Value, CultureInfo.InvariantCulture))
            .Distinct().Order().ToArray();
        if (indexes.Length == 0)
        {
            throw new NoSuchElementException("Summary exposes no upgrade action IDs; an offered upgrade and iOS PR #1244 are required.");
        }
        int index = indexes[0];
        _ = FindKeyWithScrolling(NativeSearchAccessibilityIds.SummaryUpgradeButton(index), maximumSwipes: 6)
            ?? throw new NoSuchElementException($"Summary upgrade {index} could not be brought into view.");
        string title = ReadAccessibleLabel(WaitForElementByKey(NativeSearchAccessibilityIds.SummaryUpgradeTitle(index), ScreenTimeout));
        IWebElement? difference = TryWaitForElementByKey(NativeSearchAccessibilityIds.SummaryUpgradeDifference(index), TimeSpan.FromSeconds(1));
        IWebElement? total = TryWaitForElementByKey(NativeSearchAccessibilityIds.SummaryUpgradeTotal(index), TimeSpan.FromSeconds(1));
        if ((difference is null) == (total is null))
        {
            throw new InvalidOperationException("Summary upgrade must expose exactly one price kind: difference or total.");
        }
        IWebElement price = difference ?? total ?? throw new InvalidOperationException("Upgrade price is missing.");
        return new SummaryUpgradeOffer(index, Assertions.SummaryUpgradeAssertions.FareTitle(title),
            ReadAccessibleLabel(price), difference is not null);
    }

    public void ApplySummaryUpgrade(SummaryUpgradeOffer offer)
    {
        TapByKey(NativeSearchAccessibilityIds.SummaryUpgradeButton(offer.Index));
        Wait.Until(_ =>
        {
            if (IsVisibleByKey(NativeSearchAccessibilityIds.SummaryError, TimeSpan.FromMilliseconds(100)))
            {
                throw new InvalidOperationException("The app displayed a Summary upgrade error; upgrade was not retried.");
            }
            return ReadSummaryFareTitle().Trim().Is(offer.FareTitle) &&
                WaitForElementByKey(NativeSearchAccessibilityIds.SummaryContinueButton, ScreenTimeout).Enabled;
        });
    }

    public string ReadSummaryFareTitle(int itineraryIndex = 0)
        => ReadAccessibleLabel(WaitForElementByKey(NativeSearchAccessibilityIds.SummaryFareTitle(itineraryIndex), ScreenTimeout));

    public void AssertInboundReselectionRequired(FlightSearch search)
    {
        if (!search.IsReturn)
            throw new InvalidOperationException("Return search required.");
        WaitForResults();
        AssertResultsRouteContains(search.Destination, search.Origin);
        // iOS deliberately exposes the flexible-date carousel only for outbound results.
        // The return date is verified on the scoped inbound Summary itinerary after selection.
        if (IsFlightSummaryDisplayed())
        {
            throw new InvalidOperationException("Changing outbound reused the old Summary instead of requiring inbound reselection.");
        }
    }

    public SelectedFlight SelectFirstFlightByCarrier(bool? virginFlight, bool expectFlightSummary, string? requiredCabin = null,
        SelectedFlight? excludedFlight = null, bool captureFareTitle = false, string? requiredFareTitle = null,
        bool requireUnsupportedSeatCarriers = false, string? requiredCabinTitle = null,
        string? requiredOperatingCarrierCode = null, int? requiredConnections = null,
        Func<ConnectingFlightDetails, bool>? requiredLegs = null)
    {
        IReadOnlyDictionary<string, string> resultTextByAccessibilityId = ReadFlightResultTextFromPageSource();
        List<string> attemptedCabins = [];

        for (int flightIndex = 0; flightIndex < 20; flightIndex++)
        {
            string resultAccessibilityId = NativeSearchAccessibilityIds.FlightResult(flightIndex);
            IWebElement? result = FindKeyWithScrolling(resultAccessibilityId, 2);
            if (result is null)
            {
                continue;
            }

            string resultText = resultTextByAccessibilityId.TryGetValue(resultAccessibilityId, out string? pageSourceText)
                ? pageSourceText
                : ReadElementTreeText(result);
            if (excludedFlight is not null &&
                Assertions.SummaryChangeAssertions.Schedule(resultText) ==
                Assertions.SummaryChangeAssertions.Schedule(excludedFlight.ResultText))
            {
                continue;
            }
            (string operatingCarrierCode, string operatingCarrierName) = ReadOperatingCarrier(Driver.PageSource, resultAccessibilityId);
            bool isVirgin = operatingCarrierCode.Is("VS");
            if (virginFlight.HasValue && isVirgin != virginFlight.Value)
            {
                continue;
            }
            if (requiredOperatingCarrierCode is not null && !operatingCarrierCode.Is(requiredOperatingCarrierCode))
            {
                continue;
            }
            if (requiredConnections is not null && ReadConnectionCount(resultText) != requiredConnections)
            {
                continue;
            }

            ConnectingFlightDetails? verifiedLegs = null;
            if (requireUnsupportedSeatCarriers || requiredLegs is not null)
            {
                // A card's primary carrier cannot establish every connection's eligibility.
                int? stops = ReadConnectionCount(resultText);
                if (stops is null or < 0 or > 3)
                    continue;
                verifiedLegs = ReadConnectingFlightDetails(
                    NativeSearchAccessibilityIds.ResultFlightDetails(flightIndex), stops.Value);
                if (requireUnsupportedSeatCarriers && !Assertions.SeatCarrierEligibilityAssertions.AllLegsAreKnownUnsupported(verifiedLegs))
                    continue;
                if (requiredLegs is not null && !requiredLegs(verifiedLegs))
                    continue;
            }

            IReadOnlyList<IWebElement> cabinButtons = Driver.FindElements(new ByIosNSPredicate(
                    $"type == 'XCUIElementTypeButton' AND name BEGINSWITH '{resultAccessibilityId}Cabin'"))
                .Where(element => element.Displayed && element.Enabled)
                .ToList();
            foreach (IWebElement cabin in cabinButtons)
            {
                string cabinIdentifier = cabin.GetAttribute("name") ?? string.Empty;
                if (requiredCabin is not null && cabinIdentifier != NativeSearchAccessibilityIds.FlightCabin(flightIndex, requiredCabin))
                {
                    continue;
                }
                attemptedCabins.Add(cabinIdentifier.IsNullOrWhiteSpace()
                    ? $"{resultAccessibilityId}: unnamed cabin"
                    : cabinIdentifier);
                if (cabinIdentifier.IsNullOrWhiteSpace() || !TryOpenFareSelector(cabinIdentifier))
                {
                    continue;
                }

                if (requiredCabinTitle is not null)
                {
                    IWebElement[] matchingCabins = Driver.FindElements(new ByIosNSPredicate(
                            $"type == 'XCUIElementTypeButton' AND name BEGINSWITH '{NativeSearchAccessibilityIds.FareSelectorCabinPrefix}'"))
                        .Where(element => element.Displayed && element.Enabled &&
                            ReadAccessibleLabel(element).Trim().Is(requiredCabinTitle)).ToArray();
                    if (matchingCabins.Length != 1)
                        throw new InvalidOperationException($"Expected one offered '{requiredCabinTitle}' cabin; found {matchingCabins.Length}.");
                    matchingCabins[0].Click();
                    Wait.Until(_ => ReadSelectedFareSelectorCabinName().Is(requiredCabinTitle));
                }
                string cabinName = ReadSelectedFareSelectorCabinName();
                int selectedFareIndex = requiredFareTitle is null ? 0 : FindFareIndex(Driver.PageSource, requiredFareTitle);
                string fareTitle = excludedFlight is null && !captureFareTitle && requiredFareTitle is null
                    ? string.Empty : ReadAccessibleLabel(
                        WaitForElementByKey(NativeSearchAccessibilityIds.FareTitle(selectedFareIndex), ScreenTimeout));
                IWebElement selectedFareButton = FindKeyWithScrolling(
                    NativeSearchAccessibilityIds.FareSelectButton(selectedFareIndex), 4)
                    ?? throw new InvalidOperationException("The required fare selection button was not available.");
                string selectedFareLabel = ReadAccessibleLabel(selectedFareButton);
                selectedFareButton.Click();
                if (expectFlightSummary)
                {
                    WaitForFlightSelectionOutcome(cabinIdentifier);
                }
                else
                {
                    WaitForElementByKey(NativeSearchAccessibilityIds.RunningPriceSummary, ScreenTimeout);
                    WaitForElementByKey(NativeSearchAccessibilityIds.FlightResult(0), ScreenTimeout);
                }

                return new SelectedFlight(
                    flightIndex,
                    cabinName,
                    resultText,
                    operatingCarrierCode,
                    operatingCarrierName)
                { FareLabel = selectedFareLabel, FareTitle = fareTitle, Details = verifiedLegs };
            }

            // Some connecting results match the requested operating carrier but do not
            // expose a selectable cabin on the currently displayed itinerary card. Keep
            // scanning instead of treating the first such dynamic result as terminal.
            continue;
        }

        throw new NoSuchElementException(
            $"No selectable {(requiredOperatingCarrierCode ?? (virginFlight switch { true => "Virgin Atlantic", false => "non-Virgin", null => "eligible-carrier" }))} cabin opened its fare selector. " +
            (excludedFlight is null ? string.Empty : "A replacement with a distinct displayed schedule was required. ") +
            $"Attempted cabins: {(attemptedCabins.Count == 0 ? "none" : string.Join(", ", attemptedCabins))}.");
    }

    public IReadOnlyList<string> SelectBackendMatchedFareAndReadBaggage(BackendFareExpectation expected)
    {
        for (int index = 0; index < 20; index++)
        {
            IWebElement? result = FindKeyWithScrolling(NativeSearchAccessibilityIds.FlightResult(index), 2);
            if (result is null)
                break;
            string text = ReadElementTreeText(result);
            int stops = expected.Legs.Count - 1;
            if (!HasVisibleConnectionSummary(text, stops) ||
                !text.Contains(expected.Flight.DepartureTime.ToString("HH:mm", CultureInfo.InvariantCulture), StringComparison.Ordinal))
                continue;
            ConnectingFlightDetails details = ReadConnectingFlightDetails(NativeSearchAccessibilityIds.ResultFlightDetails(index), stops);
            if (!Assertions.BackendFlightMatching.Matches(expected, details))
                continue;
            int fareIndex = OpenBackendMatchedFare(index, expected.FareTitle);
            string farePrice = ReadAccessibleLabel(WaitForElementByKey(NativeSearchAccessibilityIds.FareSelectButton(fareIndex), ScreenTimeout));
            Assertions.BackendFlightMatching.AssertCurrency(expected.Currency, farePrice);
            TapByKey(NativeSearchAccessibilityIds.FareSelectButton(fareIndex));
            WaitForFlightSummary();
            Assert.Equal(expected.FareTitle, ReadAccessibleLabel(WaitForElementByKey(NativeSearchAccessibilityIds.SummaryFareTitle(0), ScreenTimeout)), ignoreCase: true);
            Assert.True(Assertions.BackendFlightMatching.Matches(expected,
                ReadConnectingFlightDetails(NativeSearchAccessibilityIds.SummaryFlightDetails(0), stops)),
                "Summary no longer matches the backend-selected flight.");
            IWebElement action = FindKeyWithScrolling(NativeSearchAccessibilityIds.SummaryFlightDetails(0), 4)
                ?? throw new InvalidOperationException("Summary Flight details action unavailable.");
            action.Click();
            if (FindKeyWithScrolling(NativeSearchAccessibilityIds.SummaryBaggage, 8) is null)
                throw new InvalidOperationException("Matched fare Summary baggage section unavailable.");
            return ReadSummaryBaggageAllowances(Driver.PageSource);
        }
        throw new InvalidOperationException("No exact backend itinerary matched within 20 UI results. No substitute flight or fare was selected; compare environment/search data before classifying an app defect.");
    }

    private int OpenBackendMatchedFare(int flightIndex, string fareTitle)
    {
        string resultId = NativeSearchAccessibilityIds.FlightResult(flightIndex);
        FindKeyWithScrolling(resultId, 2);
        string[] cabinIds = Driver.FindElements(new ByIosNSPredicate(
                $"type == 'XCUIElementTypeButton' AND name BEGINSWITH '{resultId}Cabin'"))
            .Where(element => element.Displayed && element.Enabled)
            .Select(element => element.GetAttribute("name") ?? string.Empty)
            .Where(id => !id.IsNullOrWhiteSpace()).Distinct().Take(8).ToArray();
        foreach (string cabinId in cabinIds)
        {
            if (!TryOpenFareSelector(cabinId))
                continue;
            bool titlePresent = XDocument.Parse(Driver.PageSource).Descendants().Any(element =>
                ((string?)element.Attribute("name") ?? string.Empty).StartsWith(NativeSearchAccessibilityIds.FareSelectorFarePrefix, StringComparison.Ordinal) &&
                ((string?)element.Attribute("name") ?? string.Empty).EndsWith("Title", StringComparison.Ordinal) &&
                ((string?)element.Attribute("label") ?? string.Empty).Trim().Is(fareTitle));
            if (titlePresent)
                return FindFareIndex(Driver.PageSource, fareTitle);
            TapByKey(NativeSearchAccessibilityIds.FareSelectorDismiss);
            WaitForElementByKey(resultId, ScreenTimeout);
        }
        throw new InvalidOperationException("The exact backend fare title was not available in the matched flight's displayed cabins. No substitute selected.");
    }

    public FareBaggageObservation SelectFirstFareAndReadBenefitsAndSummaryBaggage()
    {
        for (int flightIndex = 0; flightIndex < 20; flightIndex++)
        {
            string resultIdentifier = NativeSearchAccessibilityIds.FlightResult(flightIndex);
            IWebElement? result = FindKeyWithScrolling(resultIdentifier, maximumSwipes: 2);
            if (result is null)
            {
                continue;
            }

            IReadOnlyList<IWebElement> cabins = Driver.FindElements(new ByIosNSPredicate(
                    $"type == 'XCUIElementTypeButton' AND name BEGINSWITH '{resultIdentifier}Cabin'"))
                .Where(element => element.Displayed && element.Enabled)
                .ToList();
            foreach (IWebElement cabin in cabins)
            {
                string cabinIdentifier = cabin.GetAttribute("name") ?? string.Empty;
                if (cabinIdentifier.IsNullOrWhiteSpace() || !TryOpenFareSelector(cabinIdentifier))
                {
                    continue;
                }

                IWebElement benefits = Wait.Until(_ => Driver.FindElements(new ByIosNSPredicate(
                        "name BEGINSWITH 'NativeFlightFareSelectorFare' AND name ENDSWITH 'Benefits'"))
                    .FirstOrDefault(element => element.Displayed));
                string benefitsIdentifier = benefits.GetAttribute("name") ?? string.Empty;
                System.Text.RegularExpressions.Match fareIndexMatch =
                    RegexCompiled.Match(
                        benefitsIdentifier,
                        @"NativeFlightFareSelectorFare(?<index>\d+)Benefits",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (!fareIndexMatch.Success ||
                    !int.TryParse(fareIndexMatch.Groups["index"].Value, out int fareIndex))
                {
                    throw new InvalidOperationException(
                        $"Displayed fare benefits used an unexpected identifier: '{benefitsIdentifier}'.");
                }

                IReadOnlyList<string> benefitLabels = Wait.Until(_ =>
                {
                    IReadOnlyList<string> labels = ReadDistinctAccessibleLabels(benefits)
                        .Where(label => label.StartsWith("Included,", StringComparison.OrdinalIgnoreCase) ||
                            label.StartsWith("Not included,", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    return labels.Count > 0 ? labels : null;
                });
                IWebElement fareButton = WaitForElementByKey(
                    NativeSearchAccessibilityIds.FareSelectButton(fareIndex), ScreenTimeout);
                string fareLabel = ReadAccessibleLabel(WaitForElementByKey(
                    NativeSearchAccessibilityIds.FareTitle(fareIndex), ScreenTimeout));
                fareButton.Click();
                WaitForFlightSummary();

                string summaryFareLabel = ReadAccessibleLabel(WaitForElementByKey(
                    NativeSearchAccessibilityIds.SummaryFareTitle(0), ScreenTimeout));
                IWebElement details = FindKeyWithScrolling(
                    NativeSearchAccessibilityIds.SummaryFlightDetails(0), maximumSwipes: 4)
                    ?? throw new NoSuchElementException("Summary itinerary 0 has no accessible Flight details action.");
                details.Click();
                if (FindKeyWithScrolling(
                        NativeSearchAccessibilityIds.SummaryBaggage,
                        maximumSwipes: 8) is null)
                {
                    throw new NoSuchElementException(
                        "Flight Summary did not expose its baggage section after bounded scrolling.");
                }
                IReadOnlyList<string> allowances = ReadSummaryBaggageAllowances(Driver.PageSource);
                return new FareBaggageObservation(fareLabel, summaryFareLabel, benefitLabels, allowances);
            }
        }

        throw new NoSuchElementException("No available result exposed a selectable fare for fare and baggage validation.");
    }

    internal static IReadOnlyList<string> ReadSummaryBaggageAllowances(string source)
    {
        // The details sheet renders its allowance rows together in a non-lazy VStack.
        // Inspect every exact indexed ID, including off-screen rows and non-contiguous indexes.
        return XDocument.Parse(source).Descendants()
            .Where(element => RegexCompiled.IsMatch(element.Attribute("name")?.Value ?? string.Empty,
                $"^{NativeSearchAccessibilityIds.SummaryBaggageAllowancePrefix}[0-9]+$"))
            .GroupBy(element => element.Attribute("name")!.Value)
            .Select(group =>
            {
                string[] texts = group.Select(element => string.Join(" ", new[]
                    {
                        element.Attribute("label")?.Value,
                        element.Attribute("value")?.Value,
                    }.Where(value => !value.IsNullOrWhiteSpace()).Distinct(StringComparer.OrdinalIgnoreCase)))
                    .Where(value => !value.IsNullOrWhiteSpace()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                if (texts.Length != 1)
                {
                    throw new InvalidOperationException($"Baggage row '{group.Key}' must expose one unambiguous label/value.");
                }
                return texts[0];
            }).ToList();
    }

    public SelectedFlight SelectFirstMultiCarrierConnectingFlight()
    {
        IReadOnlyDictionary<string, string> resultCards = ReadFlightResultTextFromPageSource();
        int consecutiveMissingResults = 0;
        bool foundResult = false;
        for (int flightIndex = 0; flightIndex < 20; flightIndex++)
        {
            string resultIdentifier = NativeSearchAccessibilityIds.FlightResult(flightIndex);
            IWebElement? result = FindKeyWithScrolling(resultIdentifier, 3);
            if (result is null)
            {
                consecutiveMissingResults++;
                if (foundResult && consecutiveMissingResults >= 3)
                {
                    break;
                }

                continue;
            }

            foundResult = true;
            consecutiveMissingResults = 0;
            string resultText = resultCards.TryGetValue(resultIdentifier, out string? capturedText)
                ? capturedText
                : ReadElementTreeText(result);
            int? connectionCount = ReadConnectionCount(resultText);
            if (!System.Text.RegularExpressions.Regex.IsMatch(resultText, @"\+\s*1") ||
                connectionCount is null or 0)
            {
                continue;
            }

            (string operatingCarrierCode, string operatingCarrierName) = ReadOperatingCarrier(Driver.PageSource, resultIdentifier);
            ConnectingFlightDetails details = ReadConnectingFlightDetails(
                NativeSearchAccessibilityIds.ResultFlightDetails(flightIndex), connectionCount.Value);
            IReadOnlyList<IWebElement> cabins = Driver.FindElements(new ByIosNSPredicate(
                    $"type == 'XCUIElementTypeButton' AND name BEGINSWITH '{resultIdentifier}Cabin'"))
                .Where(element => element.Displayed && element.Enabled)
                .ToList();
            foreach (IWebElement cabin in cabins)
            {
                string cabinIdentifier = cabin.GetAttribute("name") ?? string.Empty;
                if (cabinIdentifier.IsNullOrWhiteSpace() || !TryOpenFareSelector(cabinIdentifier))
                {
                    continue;
                }

                string cabinName = ReadSelectedFareSelectorCabinName();
                TapByKey(NativeSearchAccessibilityIds.FareSelectButton(0));
                WaitForFlightSelectionOutcome(cabinIdentifier);
                return new SelectedFlight(
                    flightIndex,
                    cabinName,
                    resultText,
                    operatingCarrierCode,
                    operatingCarrierName)
                { Details = details };
            }
        }

        throw new NoSuchElementException(
            "No selectable non-direct multi-carrier result displaying '+1' was available.");
    }

    public void AssertMultiCarrierConnectionIsRetainedOnSummary(
        FlightSearch search,
        SelectedFlight selected)
    {
        FlightSummary summary = ReadSummary();
        int expectedConnections = ReadConnectionCount(selected.ResultText)
            ?? throw new InvalidOperationException(
                $"The selected '+1' result exposed no connection count: '{selected.ResultText}'.");
        if (expectedConnections == 0 ||
            !HasVisibleConnectionSummary(summary.ItineraryText, expectedConnections) ||
            !System.Text.RegularExpressions.Regex.IsMatch(summary.ItineraryText, @"\+\s*1"))
        {
            throw new InvalidOperationException(
                $"Flight Summary did not retain the selected multi-carrier connection. " +
                $"Expected {expectedConnections} stop(s) and '+1'; itinerary='{summary.ItineraryText}'.");
        }

        AssertFlightSummaryMatchesSelection(search, selected, expectVirginOperatingCarrier:
            selected.OperatingCarrierCode.Equals("VS", StringComparison.OrdinalIgnoreCase));
    }

    public ConnectingFlightDetails ReadSummaryConnectingFlightDetails(int stops)
        => ReadConnectingFlightDetails(NativeSearchAccessibilityIds.SummaryFlightDetails(0), stops);

    private ConnectingFlightDetails ReadConnectingFlightDetails(string actionId, int stops)
    {
        IReadOnlyList<string> required = FlightDetailsEvidence.RequiredIds(stops);
        IWebElement action = actionId.StartsWith("NativeFlightResult", StringComparison.Ordinal)
            ? FindDisplayedFlightDetailsAction(ParseFlightIndex(actionId[..^"FlightDetails".Length]), 6)
                ?? throw new NoSuchElementException($"Flight details action '{actionId}' was not available on its candidate card.")
            : FindKeyWithScrolling(actionId, maximumSwipes: 4)
            ?? throw new NoSuchElementException($"Flight details action '{actionId}' was not available.");
        action.Click();
        WaitForElementByKey(NativeSearchAccessibilityIds.FlightDetails, ScreenTimeout);
        Dictionary<string, string> values = new(StringComparer.Ordinal);
        for (int attempt = 0; attempt <= 8; attempt++)
        {
            FlightDetailsEvidence.Capture(Driver.PageSource, stops, values);
            string? missing = required.FirstOrDefault(id => !values.ContainsKey(id));
            if (missing is null)
            {
                ConnectingFlightDetails details = FlightDetailsEvidence.Read(values, stops);
                TapByKey(NativeSearchAccessibilityIds.FlightDetailsClose);
                Wait.Until(_ => !IsVisibleByKey(NativeSearchAccessibilityIds.FlightDetails, TimeSpan.FromMilliseconds(250)));
                return details;
            }
            if (attempt == 8)
                throw new InvalidOperationException($"Flight details did not expose '{missing}' after bounded reading.");
            ScrollWithinContainerToKey(NativeSearchAccessibilityIds.FlightDetails, missing,
                maximumScrolls: 2, useNativeSwipe: true);
        }
        throw new InvalidOperationException("Flight details reading exceeded its bound.");
    }

    private bool TryOpenFareSelector(string cabinIdentifier)
    {
        for (int attempt = 0; attempt < 2; attempt++)
        {
            IWebElement? cabin = FindKeyWithScrolling(cabinIdentifier, maximumSwipes: 2);
            if (cabin is null || !cabin.Displayed || !cabin.Enabled)
            {
                return false;
            }

            cabin.Click();
            if (TryWaitForElementByKey(
                    NativeSearchAccessibilityIds.FareSelectorReady,
                    TimeSpan.FromSeconds(5)) is not null)
            {
                return true;
            }
        }

        return false;
    }

    private void WaitForFlightSelectionOutcome(string cabinIdentifier)
    {
        try
        {
            Wait.Until(_ =>
            {
                if (IsFlightSummaryDisplayed())
                {
                    return true;
                }

                foreach (string errorIdentifier in new[]
                {
                    NativeSearchAccessibilityIds.ResultsSelectionError,
                    NativeSearchAccessibilityIds.FareSelectorSelectionError,
                    NativeSearchAccessibilityIds.FareSelectorUnavailable,
                })
                {
                    IWebElement? error = TryWaitForElementByKey(
                        errorIdentifier,
                        TimeSpan.FromMilliseconds(250));
                    if (error is not null && error.Displayed)
                    {
                        throw new InvalidOperationException(
                            $"Partner cabin '{cabinIdentifier}' could not be selected: " +
                            $"{ReadAccessibleLabel(error)}");
                    }
                }

                return false;
            });
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new WebDriverTimeoutException(
                $"Selecting partner cabin '{cabinIdentifier}' did not open the flight summary. " +
                $"Visible controls: {DescribeVisibleControls()}.",
                exception);
        }
    }

    private void WaitForFlightSummary()
        => Wait.Until(_ => IsFlightSummaryDisplayed());

    private bool IsFlightSummaryDisplayed()
        => IsVisibleByKey(
               NativeSearchAccessibilityIds.SummaryTitle,
               TimeSpan.FromMilliseconds(250)) ||
           IsVisibleByKey(
               NativeSearchAccessibilityIds.SummaryItinerary(0),
               TimeSpan.FromMilliseconds(250));

    private string DescribeVisibleControls()
    {
        try
        {
            XDocument source = XDocument.Parse(Driver.PageSource);
            string[] controls = source.Descendants()
                .Where(element => element.Attribute("visible")?.Value
                    .Equals("true", StringComparison.OrdinalIgnoreCase) == true)
                .SelectMany(element => new[]
                {
                    element.Attribute("name")?.Value,
                    element.Attribute("label")?.Value,
                    element.Attribute("value")?.Value,
                })
                .Where(value => !value.IsNullOrWhiteSpace())
                .Distinct(StringComparer.Ordinal)
                .Take(40)
                .ToArray()!;
            return controls.Length == 0 ? "none exposed" : string.Join(" | ", controls);
        }
        catch (Exception sourceException)
        {
            return $"unavailable ({sourceException.GetType().Name})";
        }
    }

    private void SetPassengerCount(string countKey, string incrementKey, string decrementKey, int requiredCount)
    {
        IWebElement count = WaitForElementByKey(countKey, ScreenTimeout);
        int currentCount = ParsePassengerCount(count.Text);
        while (currentCount != requiredCount)
        {
            int expectedCount = currentCount + (currentCount < requiredCount ? 1 : -1);
            string actionKey = currentCount < requiredCount ? incrementKey : decrementKey;
            TapByKey(actionKey);
            Wait.Until(_ => ParsePassengerCount(count.Text) == expectedCount);
            currentCount = expectedCount;
        }
    }

    public bool WaitForControlledSearchError(string expectedMessage, bool requestTimeout = false)
    {
        IWebElement error = WaitForElementByKey(NativeSearchAccessibilityIds.SearchSubmissionError,
            requestTimeout ? TimeSpan.FromSeconds(180) : ScreenTimeout);
        IWebElement search = WaitForElementByKey(NativeSearchAccessibilityIds.SearchFlightsButton, ScreenTimeout);
        return error.Displayed && error.Text.Trim() == expectedMessage && search.Displayed && search.Enabled;
    }

    private bool ThrowIfSubmissionFailed()
    {
        IWebElement? error = TryWaitForElementByKey(NativeSearchAccessibilityIds.SearchSubmissionError, TimeSpan.FromMilliseconds(500));
        if (error is null || !error.Displayed)
        {
            return false;
        }

        throw new InvalidOperationException($"Native flight search failed: {error.Text}");
    }

    private IWebElement? FindKeyWithScrolling(string key, int maximumSwipes)
    {
        for (int attempt = 0; attempt <= maximumSwipes; attempt++)
        {
            IWebElement? element;
            try
            {
                element = TryWaitForElementByKey(key, TimeSpan.FromSeconds(1));
            }
            catch (ArgumentException)
            {
                // Some remote Appium responses briefly omit the W3C element reference
                // while SwiftUI is rebuilding the hierarchy. Re-query on the next pass.
                element = null;
            }
            if (element is not null && element.Displayed)
            {
                return element;
            }

            if (attempt < maximumSwipes)
            {
                Driver.ExecuteScript("mobile: scroll", new Dictionary<string, object>
                {
                    ["direction"] = "down",
                });
            }
        }

        return null;
    }

    internal static string FlightDetailsScrollDirection(string source, int flightIndex)
    {
        const string prefix = "NativeFlightResult";
        int[] visibleIndexes = XDocument.Parse(source).Descendants()
            .Where(element => (string?)element.Attribute("visible") == "true")
            .Select(element => (string?)element.Attribute("name") ?? string.Empty)
            .Where(id => id.StartsWith(prefix, StringComparison.Ordinal))
            .Select(id => new string(id[prefix.Length..].TakeWhile(char.IsDigit).ToArray()))
            .Where(index => index.Length > 0)
            .Select(index => int.Parse(index, CultureInfo.InvariantCulture))
            .ToArray();
        if (visibleIndexes.Length == 0)
            throw new InvalidOperationException("No visible flight result IDs; cannot safely locate the candidate's Flight details.");
        return visibleIndexes.Min() > flightIndex ? "up" : "down";
    }

    private IWebElement? FindTappableFilterOption(string key, int maximumSwipes)
    {
        IWebElement applyButton = WaitForElementByKey(NativeSearchAccessibilityIds.ApplyFilters, ScreenTimeout);
        System.Drawing.Size viewport = Driver.Manage().Window.Size;

        for (int attempt = 0; attempt <= maximumSwipes; attempt++)
        {
            IWebElement? option = TryWaitForElementByKey(key, TimeSpan.FromSeconds(1));
            if (option is not null && option.Displayed &&
                option.Location.Y + option.Size.Height < applyButton.Location.Y - 80)
            {
                return option;
            }

            if (attempt < maximumSwipes)
            {
                int x = viewport.Width / 2;
                bool optionIsAboveViewport = option is not null && option.Location.Y < 0;
                Driver.ExecuteScript("mobile: dragFromToForDuration", new Dictionary<string, object>
                {
                    ["duration"] = 0.35,
                    ["fromX"] = x,
                    ["fromY"] = optionIsAboveViewport
                        ? (int)(viewport.Height * 0.32)
                        : (int)(viewport.Height * 0.68),
                    ["toX"] = x,
                    ["toY"] = optionIsAboveViewport
                        ? (int)(viewport.Height * 0.68)
                        : (int)(viewport.Height * 0.32),
                });
            }
        }

        return null;
    }

    private bool ThrowIfResultsUnavailable()
    {
        IWebElement? unavailable = TryWaitForElementByKey(
            NativeSearchAccessibilityIds.ResultsUnavailable,
            TimeSpan.FromMilliseconds(500));
        if (unavailable is null || !unavailable.Displayed)
        {
            return false;
        }

        throw new InvalidOperationException($"Native flight results were unavailable: {unavailable.Text}");
    }

    private static string ReadSummaryRow(IWebElement row)
    {
        return $"{row.GetAttribute("label")} {row.Text}".Trim();
    }

    private static int ParsePassengerCount(string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int count))
        {
            throw new InvalidOperationException("The passenger selector did not expose a numeric count.");
        }

        return count;
    }

    private static (string Count, string Increment, string Decrement) PassengerKeys(
        PassengerCategory category) => category switch
        {
            PassengerCategory.Adult => (
                NativeSearchAccessibilityIds.AdultCount,
                NativeSearchAccessibilityIds.AdultIncrementButton,
                NativeSearchAccessibilityIds.AdultDecrementButton),
            PassengerCategory.Teen => (
                NativeSearchAccessibilityIds.TeenCount,
                NativeSearchAccessibilityIds.TeenIncrementButton,
                NativeSearchAccessibilityIds.TeenDecrementButton),
            PassengerCategory.Child => (
                NativeSearchAccessibilityIds.ChildCount,
                NativeSearchAccessibilityIds.ChildIncrementButton,
                NativeSearchAccessibilityIds.ChildDecrementButton),
            PassengerCategory.Infant => (
                NativeSearchAccessibilityIds.InfantCount,
                NativeSearchAccessibilityIds.InfantIncrementButton,
                NativeSearchAccessibilityIds.InfantDecrementButton),
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null),
        };

    private bool IsCalendarVisible()
    {
        foreach (IWebElement element in Driver.FindElements(MobileBy.AccessibilityId(NativeSearchAccessibilityIds.Calendar)))
        {
            try
            {
                if (element.Displayed)
                {
                    return true;
                }
            }
            catch (StaleElementReferenceException)
            {
                // The sheet can rebuild while it is dismissing.
            }
        }

        return false;
    }

    private string ReadControlText(string accessibilityId)
    {
        IWebElement element = WaitForElementByKey(accessibilityId, ScreenTimeout);
        return string.Join(" ", new[]
            {
                element.Text,
                element.GetAttribute("label"),
                element.GetAttribute("value"),
            }
            .Where(value => !value.IsNullOrWhiteSpace())
            .Distinct(StringComparer.Ordinal));
    }

    private void AssertVisibleResultsArePriceAscending()
    {
        IReadOnlyDictionary<string, string> resultText = ReadFlightResultTextFromPageSource();
        List<decimal> prices = resultText
            .OrderBy(item => ParseFlightIndex(item.Key))
            .Select(item => ReadMinimumPrice(item.Value))
            .Where(price => price.HasValue)
            .Select(price => price!.Value)
            .Take(6)
            .ToList();

        if (prices.Count < 2 || prices.Where((price, index) => index > 0 && price < prices[index - 1]).Any())
        {
            throw new InvalidOperationException(
                $"Price sort did not expose at least two ascending results. Prices: {string.Join(", ", prices)}.");
        }
    }

    private static bool ResultContainsCarrierLeg(string result, string carrierCode)
        => result.Contains($"Carrier{carrierCode}", StringComparison.Ordinal) ||
            System.Text.RegularExpressions.Regex.IsMatch(
                result,
                $@"\b{System.Text.RegularExpressions.Regex.Escape(carrierCode)}\s?\d{{1,4}}\b",
                System.Text.RegularExpressions.RegexOptions.CultureInvariant);

    private void AssertSortAndRecommendedReset(
        string sortOptionAccessibilityId,
        Func<string, int?> readValue,
        string description)
    {
        string baselineResultSignature = ReadResultSignature();
        ApplySort(sortOptionAccessibilityId);
        AssertVisibleResultsAreOrdered(readValue, description);
        ApplySort(NativeSearchAccessibilityIds.RecommendedSortOption);
        Wait.Until(_ => ReadResultSignature().Equals(baselineResultSignature, StringComparison.Ordinal));
    }

    private void ApplyOnlyFilter(string filterIdentifier)
    {
        OpenFilterSheet();
        TapFilterOption(filterIdentifier);
        WaitForFilterSelection(filterIdentifier, expectedSelected: true);
        TapByKey(NativeSearchAccessibilityIds.ApplyFilters);
        WaitForRefinementSheetToClose(NativeSearchAccessibilityIds.ApplyFilters);
    }

    private void SwitchAppliedFilter(string selectedFilterIdentifier, string nextFilterIdentifier)
    {
        OpenFilterSheet();
        TapFilterOption(selectedFilterIdentifier);
        WaitForFilterSelection(selectedFilterIdentifier, expectedSelected: false);
        TapFilterOption(nextFilterIdentifier);
        WaitForFilterSelection(nextFilterIdentifier, expectedSelected: true);
        TapByKey(NativeSearchAccessibilityIds.ApplyFilters);
        WaitForRefinementSheetToClose(NativeSearchAccessibilityIds.ApplyFilters);
    }

    private void AssertEveryEnabledFilterOfTypeAffectsResults(string filterIdentifierFragment)
    {
        HashSet<string> testedFilters = new(StringComparer.Ordinal);
        HashSet<string> unavailableFilters = new(StringComparer.Ordinal);
        while (true)
        {
            OpenFilterSheet();
            KeyValuePair<string, string>? nextFilter = ReadEnabledFilterOptions()
                .Where(option => option.Key.Contains(filterIdentifierFragment, StringComparison.Ordinal))
                .Where(option => !testedFilters.Contains(option.Key))
                .Where(option => !unavailableFilters.Contains(option.Key))
                .Select(option => (KeyValuePair<string, string>?)option)
                .FirstOrDefault();
            if (nextFilter is null)
            {
                TapByKey(NativeSearchAccessibilityIds.ApplyFilters);
                WaitForRefinementSheetToClose(NativeSearchAccessibilityIds.ApplyFilters);
                break;
            }

            (string filterIdentifier, string filterLabel) = nextFilter.Value;
            // Select the option in the same sheet in which it was discovered. Filter
            // availability is backend-driven and cascading, so closing and reopening here
            // can invalidate an otherwise enabled option before Appium acts on it.
            TapFilterOption(filterIdentifier);
            if (!WaitForFilterSelectionOrUnavailable(filterIdentifier))
            {
                // Filter counts are recalculated as the sheet settles and after earlier
                // filters are cleared. An option discovered as enabled can therefore
                // become disabled before its accessibility action completes. It is no
                // longer a valid assertion target for this result set.
                unavailableFilters.Add(filterIdentifier);
                TapByKey(NativeSearchAccessibilityIds.ApplyFilters);
                WaitForRefinementSheetToClose(NativeSearchAccessibilityIds.ApplyFilters);
                continue;
            }
            TapByKey(NativeSearchAccessibilityIds.ApplyFilters);
            WaitForRefinementSheetToClose(NativeSearchAccessibilityIds.ApplyFilters);
            AssertVisibleResultsMatchFilter(filterIdentifier, filterLabel);
            testedFilters.Add(filterIdentifier);
            ClearAppliedFilters();
        }

        if (testedFilters.Count == 0)
        {
            throw new InvalidOperationException(
                $"No enabled filter button containing '{filterIdentifierFragment}' was discovered.");
        }

    }

    private bool WaitForFilterSelectionOrUnavailable(string filterIdentifier)
    {
        OpenQA.Selenium.Support.UI.WebDriverWait stateWait = new(
            Driver,
            TimeSpan.FromSeconds(5))
        {
            PollingInterval = TimeSpan.FromMilliseconds(200),
        };

        string state = stateWait.Until(_ =>
        {
            try
            {
                if (IsFilterSelectedInPageSource(filterIdentifier))
                {
                    return "selected";
                }

                IWebElement? option = Driver.FindElements(MobileBy.AccessibilityId(filterIdentifier))
                    .FirstOrDefault();
                if (option is null)
                {
                    return null;
                }

                if (IsToggleSelected(option))
                {
                    return "selected";
                }

                return option.Enabled ? null : "unavailable";
            }
            catch (WebDriverException)
            {
                return null;
            }
        });

        return state.Equals("selected", StringComparison.Ordinal);
    }

    private void ClearAppliedFilters()
    {
        OpenFilterSheet();
        TapByKey(NativeSearchAccessibilityIds.ClearFilters);
        WaitForRefinementSheetToClose(NativeSearchAccessibilityIds.ClearFilters);
        Wait.Until(_ => !IsVisibleByKey(
            NativeSearchAccessibilityIds.ActiveFilterSummary, TimeSpan.FromMilliseconds(500)));
    }

    private IReadOnlyList<KeyValuePair<string, string>> DiscoverEnabledFilters(
        string filterIdentifierFragment,
        int minimumCount)
    {
        OpenFilterSheet();
        IReadOnlyList<KeyValuePair<string, string>> filters = ReadEnabledFilterOptions()
            .Where(option => option.Key.Contains(filterIdentifierFragment, StringComparison.Ordinal))
            .ToList();
        TapByKey(NativeSearchAccessibilityIds.ApplyFilters);
        WaitForRefinementSheetToClose(NativeSearchAccessibilityIds.ApplyFilters);

        if (filters.Count < minimumCount)
        {
            throw new InvalidOperationException(
                $"Expected at least {minimumCount} enabled filter(s) containing " +
                $"'{filterIdentifierFragment}', but discovered {filters.Count}.");
        }

        return filters;
    }

    private void WaitForFilterSelection(string filterIdentifier, bool expectedSelected)
    {
        try
        {
            Wait.Until(_ =>
            {
                try
                {
                    IWebElement? option = Driver.FindElements(MobileBy.AccessibilityId(filterIdentifier))
                        .FirstOrDefault();
                    return option is not null && IsToggleSelected(option) == expectedSelected ||
                        IsFilterPresentInPageSource(filterIdentifier) &&
                        IsFilterSelectedInPageSource(filterIdentifier) == expectedSelected;
                }
                catch (WebDriverException)
                {
                    return false;
                }
            });
        }
        catch (WebDriverTimeoutException exception)
        {
            IWebElement? option = Driver.FindElements(MobileBy.AccessibilityId(filterIdentifier))
                .FirstOrDefault(IsDisplayed);
            string actual = option is null
                ? "the option was no longer displayed"
                : $"selected={IsToggleSelected(option)}, enabled={option.Enabled}";
            throw new InvalidOperationException(
                $"Filter '{filterIdentifier}' did not become selected={expectedSelected} after its " +
                $"native accessibility action ({actual}).",
                exception);
        }
    }

    private void AssertEveryResultContainsOperatedCarrier(string carrierCode, string carrierName)
    {
        IReadOnlyDictionary<string, string> resultCards = ReadFlightResultTextFromPageSource();
        if (resultCards.Count == 0)
        {
            throw new InvalidOperationException("The airline filter returned no result cards to validate.");
        }

        foreach ((string resultAccessibilityId, string cardText) in resultCards
            .OrderBy(item => ParseFlightIndex(item.Key)))
        {
            int flightIndex = ParseFlightIndex(resultAccessibilityId);
            if (CardShowsCarrier(cardText, carrierCode, carrierName))
            {
                continue;
            }

            IWebElement detailsAction = FindDisplayedFlightDetailsAction(flightIndex, 5)
                ?? throw new NoSuchElementException(
                    $"Flight result {flightIndex} did not expose the visible, enabled accessibility action " +
                    $"'{NativeSearchAccessibilityIds.ResultFlightDetails(flightIndex)}' after bounded scrolling. No label fallback was used.");
            detailsAction.Click();
            WaitForElementByKey(NativeSearchAccessibilityIds.FlightDetails, ScreenTimeout);
            string details = Driver.PageSource;
            if (!details.Contains($"Operated by {carrierName}", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Flight result {flightIndex} did not contain a leg operated by {carrierName}.");
            }

            IWebElement close = WaitForElementByKey(
                NativeSearchAccessibilityIds.FlightDetailsClose,
                ScreenTimeout);
            close.Click();
            Wait.Until(_ => !IsVisibleByKey(
                NativeSearchAccessibilityIds.FlightDetails,
                TimeSpan.FromMilliseconds(500)));
        }
    }

    private static bool CardShowsCarrier(string cardText, string carrierCode, string carrierName)
        => cardText.Contains($"Carrier{carrierCode}", StringComparison.Ordinal) ||
            cardText.Contains(carrierName, StringComparison.OrdinalIgnoreCase);

    private IWebElement? FindDisplayedFlightDetailsAction(int flightIndex, int maximumSwipes)
    {
        string actionId = NativeSearchAccessibilityIds.ResultFlightDetails(flightIndex);
        for (int attempt = 0; attempt <= maximumSwipes; attempt++)
        {
            try
            {
                IWebElement[] actions = Driver.FindElements(MobileBy.AccessibilityId(actionId))
                    .Where(element => element.Displayed && element.Enabled).ToArray();
                if (actions.Length == 1)
                {
                    return actions[0];
                }
            }
            catch (StaleElementReferenceException)
            {
                // Re-resolve the exact action after SwiftUI recycles a result card.
                continue;
            }

            if (attempt < maximumSwipes)
            {
                Driver.ExecuteScript("mobile: scroll", new Dictionary<string, object>
                {
                    ["direction"] = FlightDetailsScrollDirection(Driver.PageSource, flightIndex),
                    ["distance"] = 0.3,
                });
            }
        }

        return null;
    }

    private void OpenFilterSheet()
    {
        // Selective Flight Details validation can leave the results list below its
        // refinement controls. Recover through Appium scrolling before opening the
        // next discovered filter; do not rely on timing or retained scroll position.
        IWebElement filterControl = FindRefinementControl(
            NativeSearchAccessibilityIds.ResultsFilter,
            maximumSwipes: 8)
            ?? throw new NoSuchElementException(
                "Results filter control was not displayed after scrolling to the refinement controls.");
        Wait.Until(_ => filterControl.Displayed && filterControl.Enabled);
        filterControl.Click();
        WaitForElementByKey(NativeSearchAccessibilityIds.ApplyFilters, ScreenTimeout);
    }

    private IWebElement? FindRefinementControl(string accessibilityId, int maximumSwipes)
    {
        for (int attempt = 0; attempt <= maximumSwipes; attempt++)
        {
            IWebElement? control = Driver.FindElements(MobileBy.AccessibilityId(accessibilityId))
                .FirstOrDefault(IsDisplayed);
            if (control is not null)
            {
                return control;
            }

            if (attempt < maximumSwipes)
            {
                Driver.ExecuteScript("mobile: scroll", new Dictionary<string, object>
                {
                    ["direction"] = "up",
                });
            }
        }

        return null;
    }

    private void TapFilterOption(string key, int maximumSwipes = 12)
    {
        IWebElement option = FindTappableFilterOption(key, maximumSwipes)
            ?? throw new NoSuchElementException($"Filter '{key}' was not displayed.");
        bool initiallySelected = IsToggleSelected(option);
        // Use the element's native accessibility action after scrolling it clear of the
        // footer. WDA's element-relative mobile:tap offsets are not consistent across
        // local and BrowserStack XCUITest versions and can land outside the option.
        option.Click();

        OpenQA.Selenium.Support.UI.WebDriverWait actionWait = new(
            Driver,
            TimeSpan.FromSeconds(3))
        {
            PollingInterval = TimeSpan.FromMilliseconds(200),
        };
        try
        {
            actionWait.Until(_ =>
            {
                IWebElement? current = Driver.FindElements(MobileBy.AccessibilityId(key))
                    .FirstOrDefault(IsDisplayed);
                return current is not null && IsToggleSelected(current) != initiallySelected;
            });
        }
        catch (WebDriverTimeoutException)
        {
            if (IsFilterSelectedInPageSource(key) != initiallySelected)
            {
                return;
            }

            // SwiftUI can rebuild the lower rows as a drag finishes and consume the first
            // accessibility action. Re-resolve and retry once; the caller still verifies
            // the requested final selection state.
            HealingPolicy.ExecuteRecovery(
                MobileHealingType.RerenderRetry,
                $"Tap filter option '{key}'",
                attempt: 1,
                maximumAttempts: 1,
                "The SwiftUI filter sheet rebuilt or consumed the first accessibility action. " +
                "The final selected state remains verified by the calling scenario.",
                () =>
                {
                    ResetFilterSheetToTop();
                    if (!IsVisibleByKey(NativeSearchAccessibilityIds.ApplyFilters, TimeSpan.FromSeconds(1)))
                    {
                        OpenFilterSheet();
                    }

                    IWebElement? retry = FindTappableFilterOption(key, maximumSwipes);
                    if (retry is null)
                    {
                        string remainingFilterIdentifiers = string.Join(", ", XDocument.Parse(Driver.PageSource)
                            .Descendants()
                            .Select(element => element.Attribute("name")?.Value ?? string.Empty)
                            .Where(identifier => identifier.StartsWith(
                                NativeSearchAccessibilityIds.FilterOptionPrefix,
                                StringComparison.Ordinal))
                            .Distinct(StringComparer.Ordinal));
                        throw new NoSuchElementException(
                            $"Filter '{key}' disappeared after its first accessibility action. " +
                            $"Remaining filter identifiers: {remainingFilterIdentifiers}.");
                    }

                    retry.Click();
                });
        }
    }

    private bool IsFilterSelectedInPageSource(string key)
        => IsFilterSelectedInPageSource(Driver.PageSource, key);

    internal static bool IsFilterSelectedInPageSource(string pageSource, string key)
    {
        XElement? option = XDocument.Parse(pageSource)
            .Descendants()
            .FirstOrDefault(element =>
                (element.Attribute("name")?.Value ?? string.Empty).Equals(key, StringComparison.Ordinal));
        return IsSelectedState((option?.Attribute("selected")?.Value).Is("true"), option?.Attribute("value")?.Value);
    }

    private bool IsFilterPresentInPageSource(string key)
        => XDocument.Parse(Driver.PageSource)
            .Descendants()
            .Any(element =>
                (element.Attribute("name")?.Value ?? string.Empty).Equals(key, StringComparison.Ordinal));

    private void ResetFilterSheetToTop()
    {
        System.Drawing.Size viewport = Driver.Manage().Window.Size;
        int x = viewport.Width / 2;
        for (int attempt = 0; attempt < 2; attempt++)
        {
            Driver.ExecuteScript("mobile: dragFromToForDuration", new Dictionary<string, object>
            {
                ["duration"] = 0.25,
                ["fromX"] = x,
                ["fromY"] = (int)(viewport.Height * 0.38),
                ["toX"] = x,
                ["toY"] = (int)(viewport.Height * 0.58),
            });
        }
    }

    private void WaitForRefinementSheetToClose(string sheetActionAccessibilityId)
    {
        Wait.Until(_ =>
            !IsVisibleByKey(sheetActionAccessibilityId, TimeSpan.FromMilliseconds(300)) &&
            IsVisibleByKey(NativeSearchAccessibilityIds.ResultsSort, TimeSpan.FromMilliseconds(500)) &&
            IsVisibleByKey(NativeSearchAccessibilityIds.ResultsFilter, TimeSpan.FromMilliseconds(500)));
    }

    private void AssertSortOptionSelected(string sortOptionAccessibilityId)
    {
        TapByKey(NativeSearchAccessibilityIds.ResultsSort);
        WaitForElement(new ByIosNSPredicate(
            $"name == '{sortOptionAccessibilityId}' AND selected == true"), ScreenTimeout);
        TapByKey(NativeSearchAccessibilityIds.ApplySort);
        WaitForRefinementSheetToClose(NativeSearchAccessibilityIds.ApplySort);
    }

    private void ApplySort(string sortOptionAccessibilityId)
    {
        TapByKey(NativeSearchAccessibilityIds.ResultsSort);
        IWebElement option = WaitForElement(new ByIosNSPredicate(
            $"name == '{sortOptionAccessibilityId}' AND enabled == true"), ScreenTimeout);
        if (!option.Selected)
        {
            option.Click();
        }
        TapByKey(NativeSearchAccessibilityIds.ApplySort);
        WaitForRefinementSheetToClose(NativeSearchAccessibilityIds.ApplySort);
    }

    private void AssertVisibleResultsAreOrdered(Func<string, int?> readValue, string description)
    {
        List<int> values = ReadFlightResultTextFromPageSource()
            .OrderBy(item => ParseFlightIndex(item.Key))
            .Select(item => readValue(item.Value))
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .Take(6)
            .ToList();
        if (values.Count < 2 || values.Where((value, index) => index > 0 && value < values[index - 1]).Any())
        {
            throw new InvalidOperationException(
                $"The {description} sort did not expose at least two ascending results. Values: {string.Join(", ", values)}.");
        }
    }

    private IReadOnlyDictionary<string, string> ReadEnabledFilterOptions()
    {
        XDocument document = XDocument.Parse(Driver.PageSource);
        Dictionary<string, string> options = document
            .Descendants()
            .Where(element => (element.Attribute("type")?.Value ?? string.Empty)
                .Equals("XCUIElementTypeButton", StringComparison.Ordinal))
            .Where(element => (element.Attribute("enabled")?.Value ?? string.Empty)
                .Equals("true", StringComparison.OrdinalIgnoreCase))
            .Select(element => new
            {
                Identifier = element.Attribute("name")?.Value ?? string.Empty,
                Label = element.Attribute("label")?.Value ?? element.Attribute("value")?.Value ?? string.Empty,
            })
            .Where(option => option.Identifier.StartsWith(
                NativeSearchAccessibilityIds.FilterOptionPrefix,
                StringComparison.Ordinal))
            .GroupBy(option => option.Identifier, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(option => option.Label).FirstOrDefault(label => !label.IsNullOrWhiteSpace()) ?? group.Key,
                StringComparer.Ordinal);
        if (options.Count == 0)
        {
            throw new InvalidOperationException("The filter sheet exposed no enabled filter options.");
        }

        return options;
    }

    private IReadOnlyDictionary<string, FilterOptionState> ReadFilterOptionStatesAcrossSheet()
    {
        Dictionary<string, FilterOptionState> options = new(StringComparer.Ordinal);
        XDocument document = XDocument.Parse(Driver.PageSource);
        foreach (XElement element in document.Descendants().Where(element =>
            (element.Attribute("type")?.Value ?? string.Empty)
                .Equals("XCUIElementTypeButton", StringComparison.Ordinal) &&
            (element.Attribute("name")?.Value ?? string.Empty).StartsWith(
                NativeSearchAccessibilityIds.FilterOptionPrefix,
                StringComparison.Ordinal)))
        {
            string identifier = element.Attribute("name")!.Value;
            options[identifier] = new FilterOptionState(
                identifier,
                element.Attribute("label")?.Value ?? element.Attribute("value")?.Value ?? identifier,
                (element.Attribute("enabled")?.Value ?? string.Empty)
                    .Equals("true", StringComparison.OrdinalIgnoreCase));
        }

        if (options.Count == 0)
        {
            throw new InvalidOperationException("The filter sheet exposed no filter options.");
        }

        return options;
    }

    private static string FilterGroup(string filterIdentifier)
    {
        string optionIdentifier = filterIdentifier[NativeSearchAccessibilityIds.FilterOptionPrefix.Length..];
        if (optionIdentifier.StartsWith("AIRLINE_NAME-", StringComparison.Ordinal))
        {
            return "AIRLINE_NAME";
        }
        if (optionIdentifier.StartsWith("CONNECTIONS-", StringComparison.Ordinal))
        {
            return "CONNECTIONS";
        }
        if (optionIdentifier.StartsWith("DEPARTURE_TIME-", StringComparison.Ordinal))
        {
            return "DEPARTURE_TIME";
        }

        return optionIdentifier.Split('-', 2)[0];
    }

    private string ReadSelectedFareSelectorCabinIdentifier()
    {
        return ReadSelectedFareSelectorCabin().GetAttribute("name") ??
            throw new InvalidOperationException("The selected fare-selector cabin exposed no identifier.");
    }

    private string ReadSelectedFareSelectorCabinName()
    {
        string cabinName = ReadAccessibleLabel(ReadSelectedFareSelectorCabin());
        return cabinName.IsNullOrWhiteSpace()
            ? throw new InvalidOperationException("The selected fare-selector cabin exposed no name.")
            : cabinName;
    }

    private string ReadNextFareSelectorCabinIdentifier(string selectedCabinIdentifier)
    {
        string[] cabinOrder = Driver.FindElements(new ByIosNSPredicate(
                $"type == 'XCUIElementTypeButton' AND name BEGINSWITH " +
                $"'{NativeSearchAccessibilityIds.FareSelectorCabinPrefix}'"))
            .Select(element => element.GetAttribute("name") ?? string.Empty)
            .Where(identifier => !identifier.IsNullOrWhiteSpace())
            .ToArray();
        int selectedIndex = Array.IndexOf(cabinOrder, selectedCabinIdentifier);
        if (selectedIndex < 0 || selectedIndex + 1 >= cabinOrder.Length)
        {
            throw new InvalidOperationException(
                $"Selected cabin '{selectedCabinIdentifier}' had no next cabin. Order: {string.Join(", ", cabinOrder)}.");
        }

        return cabinOrder[selectedIndex + 1];
    }

    private IWebElement ReadSelectedFareSelectorCabin()
        => WaitForElement(new ByIosNSPredicate(
            $"type == 'XCUIElementTypeButton' AND name BEGINSWITH " +
            $"'{NativeSearchAccessibilityIds.FareSelectorCabinPrefix}' AND selected == true"), ScreenTimeout);

    private IWebElement? FindUpgradeViewFaresAction()
    {
        System.Drawing.Size viewport = Driver.Manage().Window.Size;
        for (int swipeAttempt = 0; swipeAttempt < 8; swipeAttempt++)
        {
            IWebElement? action = TryWaitForElementByKey(
                NativeSearchAccessibilityIds.FareSelectorUpgradeViewFares,
                TimeSpan.FromSeconds(1));
            if (action is not null && action.Displayed && action.Enabled &&
                action.Location.X >= 0 && action.Location.Y >= 0 &&
                action.Location.X + action.Size.Width <= viewport.Width &&
                action.Location.Y + action.Size.Height <= viewport.Height)
            {
                WaitForElementByKey(NativeSearchAccessibilityIds.FareSelectorUpgradeCard, ScreenTimeout);
                return action;
            }

            Driver.ExecuteScript("mobile: dragFromToForDuration", new Dictionary<string, object>
            {
                ["duration"] = 0.3,
                ["fromX"] = (int)(viewport.Width * 0.82),
                ["fromY"] = (int)(viewport.Height * 0.58),
                ["toX"] = (int)(viewport.Width * 0.18),
                ["toY"] = (int)(viewport.Height * 0.58),
            });
        }

        return null;
    }

    private void AssertVisibleResultsMatchFilter(string filterIdentifier, string filterLabel)
    {
        if (filterIdentifier.Contains("FilterOption-AIRLINE_NAME-", StringComparison.Ordinal))
        {
            const string airlineFilterPrefix = "NativeFlightResultsFilterOption-AIRLINE_NAME-";
            string carrierCode = filterIdentifier[airlineFilterPrefix.Length..];
            AssertEveryResultContainsOperatedCarrier(carrierCode, ReadAirlineName(filterLabel));
            return;
        }

        if (filterIdentifier.Contains("FilterOption-DEPARTURE_TIME-", StringComparison.Ordinal))
        {
            IWebElement? activeFilterSummary = TryWaitForElementByKey(
                NativeSearchAccessibilityIds.ActiveFilterSummary,
                TimeSpan.FromSeconds(2));
            string summary = activeFilterSummary is null
                ? filterLabel
                : ReadAccessibleLabel(activeFilterSummary);
            System.Text.RegularExpressions.Match range = System.Text.RegularExpressions.Regex.Match(
                summary, @"(\d{1,2}):(\d{2})\s*-\s*(\d{1,2}):(\d{2})");
            if (!range.Success)
            {
                throw new InvalidOperationException(
                    $"Departure-time filter summary did not expose a time range: '{summary}'.");
            }

            int start = (int.Parse(range.Groups[1].Value, CultureInfo.InvariantCulture) * 60) +
                int.Parse(range.Groups[2].Value, CultureInfo.InvariantCulture);
            int end = (int.Parse(range.Groups[3].Value, CultureInfo.InvariantCulture) * 60) +
                int.Parse(range.Groups[4].Value, CultureInfo.InvariantCulture);
            AssertVisibleResultsMatch(value => start <= end
                ? value >= start && value <= end
                : value >= start || value <= end, ReadDepartureMinutes,
                $"{filterIdentifier} ({summary})");
            return;
        }

        if (filterIdentifier.Contains("FilterOption-CONNECTIONS-", StringComparison.Ordinal))
        {
            int expectedConnections = ReadExpectedConnectionCount(filterLabel)
                ?? throw new InvalidOperationException(
                    $"Connection filter label did not expose an expected count: '{filterLabel}'.");
            AssertVisibleResultCardsMatch(
                text => HasVisibleConnectionSummary(text, expectedConnections),
                $"{filterIdentifier} ({filterLabel})");
            return;
        }

        throw new InvalidOperationException(
            $"No result validator is registered for enabled filter '{filterIdentifier}'.");
    }

    private void AssertVisibleResultsMatch(
        Func<int, bool> predicate,
        Func<string, int?> readValue,
        string filterIdentifier)
    {
        List<int> observedValues = [];
        try
        {
            Wait.Until(_ =>
            {
                observedValues = ReadFlightResultTextFromPageSource()
                    .OrderBy(item => ParseFlightIndex(item.Key))
                    .Select(item => readValue(item.Value))
                    .Where(value => value.HasValue)
                    .Select(value => value!.Value)
                    .ToList();
                return observedValues.Count > 0 && observedValues.All(predicate);
            });
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new InvalidOperationException(
                $"Displayed results did not all satisfy filter '{filterIdentifier}'. " +
                $"Values: {string.Join(", ", observedValues)}.",
                exception);
        }
    }

    private void AssertVisibleResultCardsMatch(
        Func<string, bool> predicate,
        string filterIdentifier)
    {
        IReadOnlyList<string> observedCards = [];
        try
        {
            Wait.Until(_ =>
            {
                observedCards = ReadFlightResultTextFromPageSource()
                    .OrderBy(item => ParseFlightIndex(item.Key))
                    .Select(item => item.Value)
                    .ToList();
                return observedCards.Count > 0 && observedCards.All(predicate);
            });
        }
        catch (WebDriverTimeoutException exception)
        {
            string evidence = string.Join(" | ", observedCards
                .Select((card, index) => $"card{index}='{SanitiseCardEvidence(card)}'"));
            throw new InvalidOperationException(
                $"Displayed results did not all satisfy filter '{filterIdentifier}'. " +
                $"Matching cards: {observedCards.Count(predicate)} of {observedCards.Count}. " +
                $"Accessibility evidence: {evidence}",
                exception);
        }
    }

    private static string SanitiseCardEvidence(string text)
    {
        string compact = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
        return compact.Length <= 500 ? compact : compact[..500] + "...";
    }

    private string ReadResultSignature()
        => string.Join("|", ReadFlightResultTextFromPageSource()
            .OrderBy(item => ParseFlightIndex(item.Key))
            .Select(item => $"{item.Key}:{ReadMinimumPrice(item.Value)?.ToString(CultureInfo.InvariantCulture) ?? "unknown"}"));

    private static int? ReadDepartureMinutes(string text)
    {
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(
            text, @"(?:Departure time is\s*)?(\d{1,2}):(\d{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success
            ? (int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) * 60) +
                int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture)
            : null;
    }

    private static int? ReadDurationMinutes(string text)
    {
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(
            text, @"(\d+)h(?:\s*(\d+)m)?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success
            ? (int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) * 60) +
                (match.Groups[2].Success ? int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) : 0)
            : null;
    }

    internal static int? ReadConnectionCount(string text)
    {
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(
            text, @"(\d+)\s+(?:connection|stop)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        return text.Contains("Direct", StringComparison.OrdinalIgnoreCase) ? 0 : null;
    }

    internal static bool HasVisibleConnectionSummary(string text, int expectedConnections)
    {
        if (expectedConnections == 0)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(
                text, @"\bDirect\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return System.Text.RegularExpressions.Regex.IsMatch(
            text,
            $@"\b{expectedConnections}\s+(?:connection|stop)s?\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    internal static int? ReadExpectedConnectionCount(string filterLabel)
    {
        if (System.Text.RegularExpressions.Regex.IsMatch(
            filterLabel, @"\bDirect\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            return 0;
        }

        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(
            filterLabel,
            @"\b(\d+)\s+(?:connection|stop)s?\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success
            ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
            : null;
    }

    internal static string ReadAirlineName(string filterLabel)
        => System.Text.RegularExpressions.Regex.Replace(
            filterLabel,
            @"\s*,?\s*\(\d+\)\s*$",
            string.Empty).Trim();

    private static string ReadAccessibleLabel(IWebElement element)
    {
        string label = element.GetAttribute("label") ?? string.Empty;
        return label.IsNullOrWhiteSpace() ? element.Text.Trim() : label.Trim();
    }

    private static int ParseFlightIndex(string accessibilityId)
        => int.Parse(accessibilityId["NativeFlightResult".Length..], CultureInfo.InvariantCulture);

    private static decimal? ReadMinimumPrice(string text)
    {
        System.Text.RegularExpressions.MatchCollection matches =
            System.Text.RegularExpressions.Regex.Matches(text, @"[£$€]\s*([\d,]+(?:\.\d{1,2})?)");
        IEnumerable<decimal> prices = matches
            .Select(match => match.Groups[1].Value.Replace(",", string.Empty, StringComparison.Ordinal))
            .Select(value => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price)
                ? (decimal?)price
                : null)
            .Where(price => price.HasValue)
            .Select(price => price!.Value);
        return prices.Any() ? prices.Min() : null;
    }

    private static string BuildItineraryFingerprint(string resultText)
    {
        System.Text.RegularExpressions.MatchCollection matches =
            System.Text.RegularExpressions.Regex.Matches(
                resultText,
                @"\b\d{1,2}:\d{2}\b|\b[A-Z]{2}\s?\d{2,4}\b|\b[A-Z]{3}\b");
        return string.Join("|", matches.Select(match => match.Value));
    }

    private IReadOnlyDictionary<string, string> ReadFlightResultTextFromPageSource()
    {
        try
        {
            XDocument document = XDocument.Parse(Driver.PageSource);
            return document
                .Descendants()
                .Select(element => new
                {
                    Element = element,
                    AccessibilityId = element.Attribute("name")?.Value ?? element.Attribute("label")?.Value,
                })
                .Where(item => item.AccessibilityId is not null &&
                    item.AccessibilityId.StartsWith("NativeFlightResult", StringComparison.Ordinal) &&
                    item.AccessibilityId.Length > "NativeFlightResult".Length &&
                    item.AccessibilityId["NativeFlightResult".Length..].All(char.IsDigit))
                .GroupBy(item => item.AccessibilityId!, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => string.Join(" ", group
                        .SelectMany(item => ReadElementsWithinResultCard(document, item.Element))
                        .SelectMany(element => new[]
                        {
                            element.Attribute("name")?.Value,
                            element.Attribute("label")?.Value,
                            element.Attribute("value")?.Value,
                        })
                        .Where(value => !value.IsNullOrWhiteSpace())
                        .Distinct(StringComparer.Ordinal)),
                    StringComparer.Ordinal);
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.Xml.XmlException or WebDriverException)
        {
            // This is a performance optimisation only. Retain the existing element-tree
            // read as the correctness fallback when a provider cannot return page source.
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }

    private static IEnumerable<XElement> ReadElementsWithinResultCard(
        XDocument document,
        XElement resultCard)
    {
        HashSet<XElement> elements = resultCard.DescendantsAndSelf().ToHashSet();
        if (!TryReadBounds(resultCard, out (double X, double Y, double Width, double Height) card))
        {
            return elements;
        }

        foreach (XElement element in document.Descendants())
        {
            if (!TryReadBounds(element, out (double X, double Y, double Width, double Height) bounds))
            {
                continue;
            }

            double centreX = bounds.X + (bounds.Width / 2);
            double centreY = bounds.Y + (bounds.Height / 2);
            if (centreX >= card.X && centreX <= card.X + card.Width &&
                centreY >= card.Y && centreY <= card.Y + card.Height)
            {
                elements.Add(element);
            }
        }

        return elements;
    }

    private static bool TryReadBounds(
        XElement element,
        out (double X, double Y, double Width, double Height) bounds)
    {
        double x = 0;
        double y = 0;
        double width = 0;
        double height = 0;
        bool parsed = double.TryParse(element.Attribute("x")?.Value, NumberStyles.Float,
                CultureInfo.InvariantCulture, out x) &&
            double.TryParse(element.Attribute("y")?.Value, NumberStyles.Float,
                CultureInfo.InvariantCulture, out y) &&
            double.TryParse(element.Attribute("width")?.Value, NumberStyles.Float,
                CultureInfo.InvariantCulture, out width) &&
            double.TryParse(element.Attribute("height")?.Value, NumberStyles.Float,
                CultureInfo.InvariantCulture, out height);
        bounds = (x, y, width, height);
        return parsed && width > 0 && height > 0;
    }

    private static string ReadElementTreeText(IWebElement root)
    {
        IEnumerable<IWebElement> elements = new[] { root }.Concat(root.FindElements(By.XPath(".//*")));
        return string.Join(" ", elements.SelectMany(element => new[]
            {
                element.Text,
                element.GetAttribute("name"),
                element.GetAttribute("label"),
                element.GetAttribute("value"),
            })
            .Where(value => !value.IsNullOrWhiteSpace()));
    }

    private static IReadOnlyList<string> ReadDistinctAccessibleLabels(IWebElement root)
        => new[] { root }.Concat(root.FindElements(By.XPath(".//*")))
            .SelectMany(element => new[]
            {
                element.GetAttribute("label"),
                element.GetAttribute("value"),
                element.Text,
            })
            .Where(value => !value.IsNullOrWhiteSpace())
            .Select(value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

}

public enum AirportSearchField
{
    Origin,
    Destination,
}

public sealed record SelectedFlight(
    int FlightIndex,
    string CabinName,
    string ResultText,
    string OperatingCarrierCode,
    string OperatingCarrierName)
{
    public string FareLabel { get; init; } = string.Empty;
    public string FareTitle { get; init; } = string.Empty;
    public ConnectingFlightDetails? Details { get; init; }
}

public sealed record CalendarDateSelection(string DepartureText, string ReturnText);

public sealed record ResultsRefinement(
    string SortLabel,
    string FilterIdentifier,
    string FilterLabel,
    string InitialFilterLabel,
    string BaselineResultSignature);

internal sealed record FilterOptionState(string Identifier, string Label, bool Enabled);

public enum PassengerCategory
{
    Adult,
    Teen,
    Child,
    Infant,
}
