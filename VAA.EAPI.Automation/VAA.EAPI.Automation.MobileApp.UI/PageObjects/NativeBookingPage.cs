using System.Xml.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Text;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

public sealed class NativeBookingPage : NativeAppPage
{
    private static readonly TimeSpan ScreenTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan PayWithPointsLoadTimeout = TimeSpan.FromSeconds(90);
    private decimal? _selectedSeatChargeGbp;

    public NativeBookingPage(AppiumDriver driver)
        : base(driver, ScreenTimeout)
    {
    }

    public void SubmitSummaryExpectingControlledBasketFailure(string expectedMessage)
    {
        IWebElement button = ScrollToSummaryContinueButton();
        Wait.Until(_ => button.Displayed && button.Enabled);
        button.Click();
        Wait.Until(_ =>
        {
            if (IsPassengerDetailsDisplayed())
                throw new InvalidOperationException("Controlled basket failure unexpectedly opened passenger details.");
            try
            {
                IWebElement? error = FindDisplayedByKey(NativeSearchAccessibilityIds.SummaryError);
                IWebElement? currentButton = FindDisplayedByKey(NativeBookingAccessibilityIds.SummaryContinueButton);
                return error is not null && error.Text.Trim() == expectedMessage
                    && currentButton is not null && currentButton.Enabled;
            }
            catch (StaleElementReferenceException)
            {
                // SwiftUI can replace either control during this observation. Re-read both
                // on the next bounded poll; never repeat the submission click.
                return false;
            }
        });
    }

    public void ContinueFromSummary()
    {
        Wait.Until(_ =>
            IsVisibleByKey(NativeBookingAccessibilityIds.SummaryTitle, TimeSpan.FromMilliseconds(250)) ||
            IsVisibleByKey(NativeBookingAccessibilityIds.SummaryItinerary, TimeSpan.FromMilliseconds(250)));
        IWebElement continueButton = ScrollToSummaryContinueButton();
        OpenQA.Selenium.Support.UI.WebDriverWait actionableWait = new(Driver, ScreenTimeout);
        actionableWait.Until(_ => continueButton.Displayed && continueButton.Enabled);
        continueButton.Click();
        try
        {
            OpenQA.Selenium.Support.UI.WebDriverWait passengerDetailsWait = new(Driver, ScreenTimeout)
            {
                PollingInterval = TimeSpan.FromMilliseconds(250),
            };
            passengerDetailsWait.Until(_ => IsPassengerDetailsDisplayed());
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new WebDriverTimeoutException(
                "Flight summary submission did not open Passenger details. " +
                $"Visible controls: {DescribeVisibleControls()}",
                exception);
        }
    }

    public void EnterTravellerName(int travellerIndex, string firstName, string lastName)
    {
        EnsureTravellerEditorOpen(travellerIndex);
        if (IsRedesignedTravellerEditorDisplayed())
        {
            EnterVerifiedTextAndDismissKeyboard(
                NativeBookingAccessibilityIds.RedesignedTravellerFirstNameText(travellerIndex),
                firstName);
            EnterVerifiedTextAndDismissKeyboard(
                NativeBookingAccessibilityIds.RedesignedTravellerLastNameText(travellerIndex),
                lastName);
            return;
        }

        ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.TravellerCard(travellerIndex));
        EnterVerifiedTextAndDismissKeyboard(
            NativeBookingAccessibilityIds.TravellerFirstNameText(travellerIndex), firstName);
        EnterVerifiedTextAndDismissKeyboard(
            NativeBookingAccessibilityIds.TravellerLastNameText(travellerIndex), lastName);
    }

    public void SelectTravellerGender(int travellerIndex, string gender)
    {
        if (IsRedesignedTravellerEditorDisplayed())
        {
            string redesignedGenderKey =
                NativeBookingAccessibilityIds.RedesignedTravellerGender(travellerIndex);
            ClickRedesignedSelectionRow(redesignedGenderKey);
            // iOS now presents a native Menu, not an inline wheel. Options currently
            // have no app-owned IDs; exact visible button labels are a bounded exception.
            IWebElement option = Wait.Until(_ =>
            {
                IReadOnlyCollection<IWebElement> matches = Driver.FindElements(
                    new ByIosClassChain("**/XCUIElementTypeButton"))
                    .Where(element => element.Displayed && element.Enabled &&
                        IsGenderMenuOption(element.GetAttribute("label"), gender))
                    .ToArray();
                return matches.Count == 1 ? matches.Single() : null;
            });
            option.Click();
            Wait.Until(_ =>
            {
                try
                {
                    return Driver.FindElements(MobileBy.AccessibilityId(redesignedGenderKey))
                        .Any(element => element.Displayed && element.Enabled &&
                            element.GetAttribute("type").Is("XCUIElementTypeButton") &&
                            IsGenderMenuOption(element.GetAttribute("value"), gender));
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
            return;
        }

        string selectorKey = NativeBookingAccessibilityIds.TravellerGenderSelector(travellerIndex);
        IWebElement? selector = TryWaitForElementByKey(selectorKey, TimeSpan.FromSeconds(1));
        if (selector is null)
        {
            ScrollToKeyWithoutSleep(
                NativeBookingAccessibilityIds.LegacyTravellerGenderButton(travellerIndex, gender)).Click();
            return;
        }

        ScrollToKeyWithoutSleep(selectorKey).Click();
        WaitForElementByKey(
            NativeBookingAccessibilityIds.TravellerGenderOption(travellerIndex, gender),
            ScreenTimeout).Click();
    }

    internal static bool IsGenderMenuOption(string? label, string gender)
        => !gender.IsNullOrWhiteSpace() && label.Is(gender);

    public void OpenTravellerDateOfBirthPicker(int travellerIndex)
    {
        if (IsRedesignedTravellerEditorDisplayed())
        {
            ScrollToKeyWithoutSleep(
                NativeBookingAccessibilityIds.RedesignedTravellerDateOfBirth(travellerIndex)).Click();
            WaitForElement(
                new ByIosClassChain("**/XCUIElementTypePickerWheel"),
                ScreenTimeout);
            return;
        }

        ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.TravellerDateOfBirthButton(travellerIndex)).Click();
        WaitForKeyVisibility(
            NativeBookingAccessibilityIds.TravellerDateOfBirthPicker(travellerIndex),
            shouldBeVisible: true);
    }

    public void SelectDefaultTravellerDateOfBirth(int travellerIndex)
    {
        bool redesignedEditor = IsRedesignedTravellerEditorDisplayed();
        OpenTravellerDateOfBirthPicker(travellerIndex);
        string dateFieldKey = redesignedEditor
            ? NativeBookingAccessibilityIds.RedesignedTravellerDateOfBirth(travellerIndex)
            : NativeBookingAccessibilityIds.TravellerDateOfBirthButton(travellerIndex);
        string pickerKey = redesignedEditor
            ? NativeBookingAccessibilityIds.RedesignedDateOfBirthPicker
            : NativeBookingAccessibilityIds.TravellerDateOfBirthPicker(travellerIndex);

        if (redesignedEditor)
        {
            IWebElement dateWheel = WaitForElement(
                new ByIosClassChain("**/XCUIElementTypePickerWheel"),
                ScreenTimeout);
            string originalValue = ReadElementText(dateWheel);
            Driver.ExecuteScript(
                "mobile: selectPickerWheelValue",
                new Dictionary<string, object>
                {
                    ["elementId"] = GetElementReference(dateWheel),
                    ["order"] = "previous",
                    ["offset"] = 0.2,
                });
            Wait.Until(_ => !ReadElementText(dateWheel).Equals(
                originalValue,
                StringComparison.OrdinalIgnoreCase));
            ScrollToKeyWithoutSleep(dateFieldKey).Click();
        }
        else
        {
            // The legacy selector that opened the inline picker is also its close action.
            TapByKey(dateFieldKey);
        }

        WaitForKeyVisibility(
            pickerKey,
            shouldBeVisible: false);

        WaitForElementByKey(dateFieldKey, ScreenTimeout);
    }

    public bool ContactDetailsRequiredBeforeTravellerSave => IsRedesignedTravellerEditorDisplayed();

    public void VerifyMissingFirstNameBlocksTravellerSave()
    {
        EnsureTravellerEditorOpen(0);
        WaitForElementByKey(NativeBookingAccessibilityIds.RedesignedPassengerEditorScrollView);
        IWebElement firstName = WaitForElementByKey(
            NativeBookingAccessibilityIds.RedesignedTravellerFirstNameText(0));
        firstName.Clear();
        DismissKeyboard(firstName);
        Wait.Until(_ =>
        {
            IWebElement error = WaitForElementByKey(NativeBookingAccessibilityIds.RedesignedFirstPassengerNameError);
            return error.Displayed && !ReadAccessibleLabel(error).IsNullOrWhiteSpace();
        });
        ScrollWithinContainerToKey(NativeBookingAccessibilityIds.RedesignedPassengerEditorScrollView,
            NativeBookingAccessibilityIds.RedesignedPassengerSaveValidation, 8, 0.55, useNativeSwipe: true).Click();
        WaitForElementByKey(NativeBookingAccessibilityIds.RedesignedPassengerEditorScrollView);
        if (Driver.FindElements(MobileBy.AccessibilityId(NativeBookingAccessibilityIds.RedesignedPassengerSaveAndContinue))
            .Any(element => element.Displayed && element.Enabled))
        {
            throw new InvalidOperationException("A traveller with an empty required first name can be saved.");
        }
    }

    public IReadOnlyDictionary<string, string> ReadLeadTravellerEditor()
    {
        EnsureTravellerEditorOpen(0);
        if (!IsRedesignedTravellerEditorDisplayed())
        {
            throw new InvalidOperationException("Member-not-travelling requires the redesigned passenger editor.");
        }
        Dictionary<string, string> keys = new()
        {
            ["firstName"] = NativeBookingAccessibilityIds.RedesignedTravellerFirstNameText(0),
            ["lastName"] = NativeBookingAccessibilityIds.RedesignedTravellerLastNameText(0),
            ["dateOfBirth"] = NativeBookingAccessibilityIds.RedesignedTravellerDateOfBirth(0),
            ["gender"] = NativeBookingAccessibilityIds.RedesignedTravellerGender(0),
            ["membership"] = NativeBookingAccessibilityIds.RedesignedTravellerMembershipNumberText(0),
            ["email"] = NativeBookingAccessibilityIds.RedesignedContactEmailText,
            ["telephone"] = NativeBookingAccessibilityIds.RedesignedContactTelephoneText,
        };
        // All editor fields are exposed in the native hierarchy. Reading values must not
        // gesture across interactive controls (in particular the country-code picker).
        return ReadEditorFieldValues(Driver.PageSource, keys);
    }

    internal static IReadOnlyDictionary<string, string> ReadEditorFieldValues(
        string source, IReadOnlyDictionary<string, string> keys)
    {
        XElement[] editors = XDocument.Parse(source).Descendants().Where(element =>
            (element.Attribute("name")?.Value).Is(NativeBookingAccessibilityIds.RedesignedPassengerEditorScrollView) &&
            (element.Attribute("visible")?.Value).Is("true")).ToArray();
        if (editors.Length != 1)
        {
            throw new InvalidOperationException("Expected one visible passenger editor; an overlay or missing editor prevents verification.");
        }
        Dictionary<string, string> values = new();
        foreach ((string name, string key) in keys)
        {
            XElement[] candidates = editors[0].Descendants().Where(element =>
                ((element.Attribute("name")?.Value).Is(key) ||
                 (element.Attribute("name")?.Value).Is($"{key}-{key}"))).ToArray();
            XElement[] controls = candidates.Where(element =>
                (element.Attribute("type")?.Value).Is("XCUIElementTypeButton") ||
                (element.Attribute("type")?.Value).Is("XCUIElementTypeTextField")).ToArray();
            // WebDriverAgent normalizes empty native values to nil and omits them from XML.
            // Only an identified native control proves emptiness; a missing node never does.
            IEnumerable<XElement> evidence = controls.Length > 0
                ? controls : candidates.Where(element => element.Attribute("value") is not null);
            string[] observed = evidence.Select(element => element.Attribute("value")?.Value ?? "")
                .Distinct(StringComparer.Ordinal).ToArray();
            if (observed.Length != 1)
            {
                string shape = string.Join(", ", candidates.Select(element =>
                    $"{element.Attribute("type")?.Value}:hasValue={element.Attribute("value") is not null}"));
                throw new InvalidOperationException($"Passenger field '{name}' exposed missing or ambiguous value evidence. " +
                    $"Matches={candidates.Length}; distinctValues={observed.Length}; controls=[{shape}].");
            }
            values[name] = observed[0];
        }
        return values;
    }

    public void WaitForMemberTravellerProfile(string expectedFirstName)
    {
        EnsureTravellerEditorOpen(0);
        Wait.Until(_ => ReadFieldValue(NativeBookingAccessibilityIds.RedesignedTravellerFirstNameText(0))
            .Is(expectedFirstName));
    }

    public void SelectMemberNotTravelling()
    {
        EnsureTravellerEditorOpen(0);
        ClickRedesignedSelectionRow(NativeBookingAccessibilityIds.MemberSelector);
        WaitForElementByKey(NativeBookingAccessibilityIds.MemberSelfOption, ScreenTimeout);
        TapByKey(NativeBookingAccessibilityIds.MemberNotTravellingOption);
        Wait.Until(_ => AccessibleTextContainsOption(
            ReadSelectionRowText(NativeBookingAccessibilityIds.MemberSelector), "I'm not travelling"));
    }

    public bool IsMemberNotTravelling()
    {
        EnsureTravellerEditorOpen(0);
        return AccessibleTextContainsOption(ReadSelectionRowText(NativeBookingAccessibilityIds.MemberSelector),
            "I'm not travelling");
    }

    public void SetBookingTermsAccepted(bool accepted)
    {
        IWebElement terms = ScrollWithinContainerToKey(NativeBookingAccessibilityIds.PassengerDetailsHub,
            NativeBookingAccessibilityIds.RedesignedBookingTermsToggle, 8, 0.55);
        if (ReadBookingTermsState(terms.GetAttribute("value")) != accepted)
        {
            terms.Click();
        }
        Wait.Until(_ => ReadBookingTermsState(WaitForElementByKey(
            NativeBookingAccessibilityIds.RedesignedBookingTermsToggle).GetAttribute("value")) == accepted);
    }

    internal static bool ReadBookingTermsState(string? value)
        => value switch
        {
            "1" or "true" or "Selected" => true,
            "0" or "false" or "Not selected" => false,
            _ => throw new InvalidOperationException("Booking Terms did not expose a recognised selected state."),
        };

    public bool WaitForPassengerContinueState(bool enabled)
    {
        ScrollWithinContainerToKey(NativeBookingAccessibilityIds.PassengerDetailsHub,
            NativeBookingAccessibilityIds.RedesignedPassengerDetailsContinueButton, 8, 0.55);
        Wait.Until(_ => WaitForElementByKey(
            NativeBookingAccessibilityIds.RedesignedPassengerDetailsContinueButton).Enabled == enabled);
        return WaitForElementByKey(NativeBookingAccessibilityIds.RedesignedPassengerDetailsContinueButton).Enabled;
    }

    public void SaveTravellerAndContinue(bool isFinalTraveller)
    {
        if (!IsRedesignedTravellerEditorDisplayed())
        {
            return;
        }

        try
        {
            ScrollWithinContainerToKey(
                NativeBookingAccessibilityIds.RedesignedPassengerEditorScrollView,
                NativeBookingAccessibilityIds.RedesignedPassengerSaveAndContinue,
                maximumScrolls: 8,
                useNativeSwipe: true).Click();
        }
        catch (NoSuchElementException exception)
        {
            bool validationButtonDisplayed = TryWaitForElementByKey(
                NativeBookingAccessibilityIds.RedesignedPassengerSaveValidation,
                TimeSpan.FromSeconds(1)) is not null;
            throw new InvalidOperationException(
                "Passenger details are not valid enough to expose Save and continue. " +
                $"Validation control displayed: {validationButtonDisplayed}.",
                exception);
        }

        Wait.Until(_ => isFinalTraveller
            ? IsDisplayedByKey(NativeBookingAccessibilityIds.PassengerDetailsHub)
            : IsRedesignedTravellerEditorDisplayed());
    }

    public void EnterContactDetails(string emailAddress, string countryCode, string telephoneNumber)
    {
        if (!IsRedesignedTravellerEditorDisplayed())
        {
            ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.ContactEmailText);
            EnterVerifiedTextAndDismissKeyboard(NativeBookingAccessibilityIds.ContactEmailText, emailAddress);
            EnterVerifiedTextAndDismissKeyboard(NativeBookingAccessibilityIds.ContactCountryCodeText, countryCode);
            EnterVerifiedTextAndDismissKeyboard(NativeBookingAccessibilityIds.ContactTelephoneText, telephoneNumber);
            return;
        }

        ScrollWithinContainerToKey(
            NativeBookingAccessibilityIds.RedesignedPassengerEditorScrollView,
            NativeBookingAccessibilityIds.RedesignedContactEmailText,
            maximumScrolls: 8,
            useNativeSwipe: true);
        EnterVerifiedTextAndDismissKeyboard(
            NativeBookingAccessibilityIds.RedesignedContactEmailText,
            emailAddress);
        // Check the selected dial code before attempting to scroll or open its selector.
        // The profile-clear flow normally restores +44, so no interaction is needed.
        SelectRedesignedCountryCode(countryCode);
        ScrollWithinContainerToKey(
            NativeBookingAccessibilityIds.RedesignedPassengerEditorScrollView,
            NativeBookingAccessibilityIds.RedesignedContactTelephoneText,
            maximumScrolls: 3,
            useNativeSwipe: true);
        EnterVerifiedTextAndDismissKeyboard(
            NativeBookingAccessibilityIds.RedesignedContactTelephoneText,
            telephoneNumber);
    }

    private void SelectRedesignedCountryCode(string countryCode)
    {
        string countryOption = countryCode switch
        {
            "+44" => NativeBookingAccessibilityIds.UnitedKingdomCountryOption,
            _ => throw new NotSupportedException(
                $"The redesigned passenger country selector does not yet map '{countryCode}' to a country."),
        };

        if (AccessibleTextContainsOption(ReadSelectionRowText(
            NativeBookingAccessibilityIds.RedesignedContactCountryCode), countryCode))
        {
            return;
        }

        ClickRedesignedSelectionRow(NativeBookingAccessibilityIds.RedesignedContactCountryCode);
        WaitForElementByKey(countryOption, ScreenTimeout).Click();
        Wait.Until(_ => AccessibleTextContainsOption(ReadSelectionRowText(
            NativeBookingAccessibilityIds.RedesignedContactCountryCode), countryCode));
    }

    private void ClickRedesignedSelectionRow(string key)
    {
        ScrollToKeyWithoutSleep(key);
        IWebElement button = Wait.Until(_ => Driver.FindElements(MobileBy.AccessibilityId(key))
            .FirstOrDefault(element => element.Displayed && element.Enabled &&
                element.GetAttribute("type").Is("XCUIElementTypeButton")));
        System.Drawing.Rectangle buttonBounds = new(button.Location, button.Size);
        bool overlapsPicker = button.FindElements(new ByIosClassChain("**/XCUIElementTypePickerWheel"))
            .Any(picker => SelectionTapOverlapsPicker(buttonBounds,
                new System.Drawing.Rectangle(picker.Location, picker.Size)));
        if (overlapsPicker)
        {
            throw new InvalidOperationException(
                $"Accessibility contract violation: '{key}' includes the expanded picker in its tap bounds. " +
                "Tapping its centre can change the selection instead of closing it. " +
                "The iOS ID must be owned by the header button, not the containing group; no tap was attempted.");
        }
        button.Click();
    }

    internal static bool SelectionTapOverlapsPicker(System.Drawing.Rectangle button, System.Drawing.Rectangle picker)
        => button.Width > 0 && button.Height > 0 && picker.Contains(
            button.X + button.Width / 2, button.Y + button.Height / 2);

    private string ReadSelectionRowText(string key)
        => ReadSelectionRowText(Driver.PageSource, key);

    internal static string ReadSelectionRowText(string source, string key)
        => string.Join(" ", XDocument.Parse(source).Descendants()
            .Where(element => (element.Attribute("name")?.Value).Is(key) ||
                (element.Attribute("name")?.Value).Is($"{key}-{key}"))
            .SelectMany(element => element.DescendantsAndSelf())
            .Distinct()
            .Where(element => !element.Ancestors().Any(ancestor =>
                (ancestor.Attribute("type")?.Value).Is("XCUIElementTypePickerWheel")))
            .Where(element => (element.Attribute("type")?.Value).Is("XCUIElementTypeStaticText") ||
                (element.Attribute("type")?.Value).Is("XCUIElementTypeButton"))
            .Select(element => $"{element.Attribute("label")?.Value} {element.Attribute("value")?.Value}"));

    internal static bool AccessibleTextContainsOption(string text, string option)
        => RegexCompiled.IsMatch(text,
            $@"(?<![\w+]){System.Text.RegularExpressions.Regex.Escape(option)}(?!\w)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    public void VerifyPrepopulatedMemberTraveller(
        int travellerIndex,
        string expectedFirstName,
        string expectedFlyingClubNumber)
    {
        ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.TravellerCard(travellerIndex));
        string firstName = ReadFieldValue(
            NativeBookingAccessibilityIds.TravellerFirstNameText(travellerIndex));
        string lastName = ReadFieldValue(
            NativeBookingAccessibilityIds.TravellerLastNameText(travellerIndex));
        string dateOfBirth = ReadFieldValue(
            NativeBookingAccessibilityIds.TravellerDateOfBirthButton(travellerIndex));
        ScrollToKeyWithoutSleep(
            NativeBookingAccessibilityIds.TravellerMembershipNumberText(travellerIndex),
            maximumScrolls: 12);
        string flyingClubNumber = ReadFieldValue(
            NativeBookingAccessibilityIds.TravellerMembershipNumberText(travellerIndex));

        if (!firstName.Equals(expectedFirstName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The prepopulated lead passenger did not match the reserved loyalty account.");
        }

        if (lastName.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(
                "The prepopulated lead passenger did not expose a last name.");
        }
        if (dateOfBirth.IsNullOrWhiteSpace() ||
            dateOfBirth.Equals("Date of birth", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The prepopulated lead passenger did not expose a date of birth.");
        }

        string genderSelectorKey = NativeBookingAccessibilityIds.TravellerGenderSelector(travellerIndex);
        IWebElement? genderSelector = TryWaitForElementByKey(genderSelectorKey, TimeSpan.FromSeconds(1));
        string selectedGender = genderSelector is null
            ? string.Empty
            : ReadAccessibleLabel(ScrollToKeyWithoutSleep(genderSelectorKey, maximumScrolls: 4));
        bool hasSelectedGender = genderSelector is not null
            ? !selectedGender.IsNullOrWhiteSpace() &&
                !selectedGender.Contains("Select gender", StringComparison.OrdinalIgnoreCase)
            : HasSelectedLegacyGender(travellerIndex);
        if (!hasSelectedGender)
        {
            throw new InvalidOperationException(
                "The prepopulated lead passenger did not expose a selected gender.");
        }

        if (!flyingClubNumber.Equals(expectedFlyingClubNumber, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The prepopulated lead passenger Flying Club number did not match the reserved loyalty account.");
        }
    }

    public void VerifyPrepopulatedMemberContactDetailsAndAcceptTerms()
    {
        ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.ContactEmailText, maximumScrolls: 16);
        string email = ReadFieldValue(NativeBookingAccessibilityIds.ContactEmailText);
        ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.ContactCountryCodeText, maximumScrolls: 4);
        string countryCode = ReadFieldValue(NativeBookingAccessibilityIds.ContactCountryCodeText);
        ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.ContactTelephoneText, maximumScrolls: 4);
        string telephone = ReadFieldValue(NativeBookingAccessibilityIds.ContactTelephoneText);
        if (email.IsNullOrWhiteSpace() ||
            countryCode.IsNullOrWhiteSpace() ||
            telephone.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(
                "The authenticated passenger page did not prepopulate all required contact details.");
        }

        IWebElement terms = ScrollToKeyWithoutSleep(
            NativeBookingAccessibilityIds.BookingTermsToggle,
            maximumScrolls: 16);
        terms.Click();
        OpenQA.Selenium.Support.UI.WebDriverWait termsWait = new(Driver, TimeSpan.FromSeconds(5));
        termsWait.Until(_ =>
        {
            IWebElement updatedTerms = WaitForElementByKey(
                NativeBookingAccessibilityIds.BookingTermsToggle,
                TimeSpan.FromSeconds(1));
            string value = updatedTerms.GetAttribute("value") ?? string.Empty;
            return updatedTerms.Selected ||
                value == "1" ||
                value.Equals("true", StringComparison.OrdinalIgnoreCase);
        });
    }

    public void AcceptBookingTerms()
    {
        bool redesignedHub = IsVisibleByKey(
            NativeBookingAccessibilityIds.PassengerDetailsHub,
            TimeSpan.FromSeconds(1));
        string termsKey = redesignedHub
            ? NativeBookingAccessibilityIds.RedesignedBookingTermsToggle
            : NativeBookingAccessibilityIds.BookingTermsToggle;
        // Re-resolve the app-owned container on each bounded scroll rather than
        // asking WebDriverAgent to find a named target in lazy off-screen content.
        IWebElement terms = ScrollWithinContainerToKey(
            PassengerHubScrollContainer(redesignedHub),
            termsKey,
            maximumScrolls: 8,
            distance: 0.55);
        terms.Click();

        OpenQA.Selenium.Support.UI.WebDriverWait selectedWait = new(
            Driver,
            TimeSpan.FromSeconds(5))
        {
            PollingInterval = TimeSpan.FromMilliseconds(200),
        };
        selectedWait.Until(_ =>
        {
            return Driver.FindElements(MobileBy.AccessibilityId(termsKey))
                .Where(element => element.Displayed)
                .Any(element =>
                {
                    string value = element.GetAttribute("value") ?? string.Empty;
                    string accessibleState = ReadElementText(element);
                    return element.Selected ||
                        value == "1" ||
                        value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                        value.Equals("Selected", StringComparison.OrdinalIgnoreCase) ||
                        accessibleState.Contains("Selected", StringComparison.OrdinalIgnoreCase) &&
                        !accessibleState.Contains("Not selected", StringComparison.OrdinalIgnoreCase);
                });
        });
    }

    public void ContinueFromPassengerDetails(bool requireSeatSelection = false, bool stopAtSeatSelection = false,
        bool requireSeatBypass = false)
    {
        _selectedSeatChargeGbp = null;
        if (requireSeatBypass && (requireSeatSelection || stopAtSeatSelection))
            throw new ArgumentException("Seat bypass and required seat selection are mutually exclusive.");
        bool redesignedHub = IsDisplayedByKey(NativeBookingAccessibilityIds.PassengerDetailsHub);
        string submissionErrorKey = redesignedHub
            ? NativeBookingAccessibilityIds.RedesignedPassengerDetailsSubmissionError
            : NativeBookingAccessibilityIds.PassengerDetailsSubmissionError;
        string continueButtonKey = redesignedHub
            ? NativeBookingAccessibilityIds.RedesignedPassengerDetailsContinueButton
            : NativeBookingAccessibilityIds.PassengerDetailsContinueButton;
        IWebElement continueButton = ScrollWithinContainerToKey(
            PassengerHubScrollContainer(redesignedHub), continueButtonKey,
            maximumScrolls: 8, distance: 0.55);
        OpenQA.Selenium.Support.UI.WebDriverWait actionableWait = new(Driver, ScreenTimeout);
        actionableWait.Until(_ => continueButton.Displayed && continueButton.Enabled);
        continueButton.Click();

        OpenQA.Selenium.Support.UI.WebDriverWait destinationWait = new(Driver, ScreenTimeout)
        {
            PollingInterval = TimeSpan.FromMilliseconds(250),
        };
        bool destinationReached = false;
        try
        {
            PassengerDetailsDestination destination = destinationWait.Until(_ =>
            {
                IWebElement? submissionError = Driver
                    .FindElements(MobileBy.AccessibilityId(
                        submissionErrorKey))
                    .FirstOrDefault(element => element.Displayed);
                if (submissionError is not null)
                {
                    throw new InvalidOperationException(
                        $"Passenger details submission failed: {ReadElementText(submissionError)}");
                }

                if (IsDisplayedByKey(NativeBookingAccessibilityIds.PaymentHandoffTitle))
                {
                    return new PassengerDetailsDestinationResult(PassengerDetailsDestination.PaymentHandoff);
                }

                return IsDisplayedByKey(NativeBookingAccessibilityIds.SeatSelectionTitle) ||
                    IsDisplayedByKey(NativeBookingAccessibilityIds.SeatSelectionChildGuidanceSheet)
                    ? new PassengerDetailsDestinationResult(PassengerDetailsDestination.SeatSelection)
                    : null;
            }).Destination;
            destinationReached = true;

            if (destination == PassengerDetailsDestination.SeatSelection)
            {
                if (requireSeatBypass)
                    throw new InvalidOperationException(
                        "Unsupported operating-carrier journey opened seat selection. No Continue/Skip was clicked.");
                if (stopAtSeatSelection)
                {
                    return;
                }
                if (requireSeatSelection)
                {
                    SelectFirstAvailableSeatForSinglePassenger();
                }
                ContinueFromSeatSelection();
                Wait.Until(_ =>
                {
                    ThrowIfSeatSelectionFailed();
                    return IsDisplayedByKey(NativeBookingAccessibilityIds.PaymentHandoffTitle);
                });
            }
            else if (requireSeatSelection || stopAtSeatSelection)
            {
                throw new InvalidOperationException(
                    "Seat selection was required but the app opened payment directly. " +
                    "Check the skip-seat-selection flag and flight eligibility.");
            }
        }
        catch (WebDriverTimeoutException exception) when (
            !destinationReached && IsPassengerDetailsDisplayed())
        {
            throw new InvalidOperationException(
                "The app remained on Passenger details after submitting an enabled form. " +
                "Review the mobile failure evidence in the test output. If response capture was disabled, " +
                "enable BrowserStack network logs and content capture for a targeted rerun.",
                exception);
        }
        catch (WebDriverTimeoutException exception) when (!destinationReached)
        {
            throw new WebDriverTimeoutException(
                "Passenger submission opened neither payment handoff nor seat selection. " +
                $"Visible controls: {DescribeVisibleControls()}",
                exception);
        }
    }

    private bool IsPassengerDetailsDisplayed()
        => IsDisplayedByKey(NativeBookingAccessibilityIds.PassengerDetailsTitle) ||
            IsDisplayedByKey(NativeBookingAccessibilityIds.PassengerDetailsHub);

    private void EnsureTravellerEditorOpen(int travellerIndex)
    {
        if (IsRedesignedTravellerEditorDisplayed())
        {
            return;
        }

        bool redesignedHub = IsDisplayedByKey(NativeBookingAccessibilityIds.PassengerDetailsHub);
        string cardKey = TravellerCardForPage(travellerIndex, redesignedHub);
        IWebElement card = ScrollToKeyWithoutSleep(cardKey, maximumScrolls: 8);
        if (!redesignedHub)
        {
            return;
        }

        card.Click();
        try
        {
            OpenQA.Selenium.Support.UI.WebDriverWait editorWait = new(Driver, ScreenTimeout);
            editorWait.Until(_ =>
                FindDisplayedByKey(NativeBookingAccessibilityIds.TravellerCard(travellerIndex)) is not null ||
                IsRedesignedTravellerEditorDisplayed());
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new WebDriverTimeoutException(
                $"Passenger card {travellerIndex + 1} opened, but its editable form did not expose the " +
                $"app-owned traveller contract. Visible controls: {DescribeVisibleControls()}",
                exception);
        }
    }

    internal static string TravellerCardForPage(int travellerIndex, bool redesignedHub)
        => redesignedHub
            ? NativeBookingAccessibilityIds.RedesignedTravellerCard(travellerIndex)
            : NativeBookingAccessibilityIds.TravellerCard(travellerIndex);

    private bool IsRedesignedTravellerEditorDisplayed()
        // The modal header can scroll off-screen after editing a field. The owned
        // editor viewport remains the screen identity; do not fall back to legacy IDs.
        => IsDisplayedByKey(NativeBookingAccessibilityIds.RedesignedPassengerEditorScrollView) ||
            IsDisplayedByKey(NativeBookingAccessibilityIds.RedesignedPassengerEditorHeader);

    public void SelectSeat(int sectorIndex, int travellerIndex, string seatNumber)
    {
        WaitForElementByKey(NativeBookingAccessibilityIds.SeatSelectionTitle, ScreenTimeout);
        TapByKey(NativeBookingAccessibilityIds.SeatPassengerOption(travellerIndex));
        ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.Seat(sectorIndex, travellerIndex, seatNumber)).Click();
    }

    public void ContinueFromSeatSelection()
    {
        ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.SeatSelectionContinueButton).Click();
    }

    public void SelectFirstAvailableSeatForSinglePassenger()
    {
        NativeSeatSelectionPage seats = new(Driver);
        decimal originalTotal = seats.ReadTripTotal();
        string seatNumber = seats.OpenAvailableSeat();
        decimal selectedCharge = seats.ReadOpenedSeatCharge();
        seats.ConfirmSeat(seatNumber);
        seats.WaitForPrices(selectedCharge, originalTotal + selectedCharge);
        _selectedSeatChargeGbp = selectedCharge;
    }

    internal static bool PassengerHasSeat(string? label, string seatNumber)
        => !seatNumber.IsNullOrWhiteSpace() && (label ?? string.Empty)
            .Split(',').Any(part => part.Trim().Is(seatNumber));

    private void ThrowIfSeatSelectionFailed()
    {
        foreach (string key in new[]
        {
            NativeBookingAccessibilityIds.SeatSelectionError,
            NativeBookingAccessibilityIds.SeatSelectionUnavailable,
            NativeBookingAccessibilityIds.SeatSelectionSubmissionError,
        })
        {
            if (IsDisplayedByKey(key))
            {
                throw new InvalidOperationException(
                    $"Seat selection failed at '{key}'. No retry or skip-to-payment was attempted; inspect failure evidence.");
            }
        }
    }

    private bool IsDisplayedByKey(string accessibilityId)
    {
        try
        {
            return Driver.FindElements(MobileBy.AccessibilityId(accessibilityId))
                .Any(element => element.Displayed);
        }
        catch (StaleElementReferenceException)
        {
            return false;
        }
    }

    private string DescribeVisibleControls()
    {
        try
        {
            XDocument source = XDocument.Parse(Driver.PageSource);
            string[] controls = source.Descendants()
                .Where(element =>
                    element.Attribute("visible")?.Value.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
                .SelectMany(element => new[]
                {
                    element.Attribute("name")?.Value,
                    element.Attribute("label")?.Value,
                })
                .Where(value => !value.IsNullOrWhiteSpace())
                .Distinct(StringComparer.Ordinal)
                .Take(30)
                .ToArray()!;
            return controls.Length == 0 ? "none exposed" : string.Join(" | ", controls);
        }
        catch (Exception exception)
        {
            return $"unavailable ({exception.GetType().Name})";
        }
    }

    public void ContinueToSecurePayment()
    {
        WaitForElementByKey(NativeBookingAccessibilityIds.PaymentHandoffTitle, ScreenTimeout);
        IWebElement continueButton = Wait.Until(_ =>
        {
            NativePaymentFailure.ThrowIfDisplayed(
                locator => Driver.FindElements(locator).Any(element => element.Displayed),
                "during payment preparation");
            IWebElement? button = FindDisplayedByKey(NativeBookingAccessibilityIds.PaymentContinueButton);
            return button is not null && button.Enabled ? button : null;
        });
        continueButton.Click();
    }

    public string ReadSummaryTotalBeforeCheckout()
    {
        IWebElement total = WaitForElementByKey(NativeSearchAccessibilityIds.SummaryPriceButton, ScreenTimeout);
        string? value = total.GetAttribute("value");
        return !value.IsNullOrWhiteSpace() ? value : total.GetAttribute("label") ?? total.Text;
    }

    public IReadOnlyList<string> VerifyReviewTotal(string summaryTotal, bool isMember, bool allowSeats)
    {
        WaitForElementByKey(NativeBookingAccessibilityIds.PaymentHandoffTitle, ScreenTimeout);
        Wait.Until(_ =>
        {
            NativePaymentFailure.ThrowIfDisplayed(
                locator => Driver.FindElements(locator).Any(element => element.Displayed), "while reviewing payment total");
            return FindDisplayedByKey(NativeBookingAccessibilityIds.PaymentContinueButton)?.Enabled == true;
        });
        WaitForElementByKey(NativeBookingAccessibilityIds.PaymentSummaryCard, ScreenTimeout);
        IReadOnlyList<string> rows = ReadPaymentSummaryRows(Driver.PageSource);
        Assertions.ReviewTotalAssertions.Verify(rows, summaryTotal, allowSeats, _selectedSeatChargeGbp);
        if (!isMember && Driver.FindElements(MobileBy.AccessibilityId(NativeBookingAccessibilityIds.PayWithPointsCard)).Count > 0)
        {
            throw new InvalidOperationException("Guest Review total exposed member-only Pay with Points controls.");
        }
        IWebElement conditions = ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.PaymentFareConditionsLink);
        if (!conditions.Enabled)
        {
            throw new InvalidOperationException("Review total fare conditions link was disabled.");
        }
        string conditionsLabel = conditions.GetAttribute("label") ?? string.Empty;
        IWebElement guidance = ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.PaymentGuidance);
        string guidanceLabel = guidance.GetAttribute("label") ?? string.Empty;
        IWebElement continueButton = ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.PaymentContinueButton);
        Assertions.ReviewTotalAssertions.VerifyCopy(conditionsLabel, guidanceLabel,
            continueButton.GetAttribute("label") ?? string.Empty);
        return rows;
    }

    public void ReturnFromReviewToPassengerDetails()
    {
        IWebElement back = WaitForElementByKey(NativeBookingAccessibilityIds.PaymentBackButton, ScreenTimeout);
        Wait.Until(_ => back.Displayed && back.Enabled);
        back.Click();
        Wait.Until(_ => IsPassengerDetailsDisplayed() &&
            !IsDisplayedByKey(NativeBookingAccessibilityIds.PaymentHandoffTitle));
    }

    internal static IReadOnlyList<string> ReadPaymentSummaryRows(string source)
    {
        XDocument document = XDocument.Parse(source);
        XElement[] cards = document.Descendants().Where(element =>
            element.Attribute("name")?.Value == NativeBookingAccessibilityIds.PaymentSummaryCard &&
            element.Attribute("visible")?.Value == "true").ToArray();
        if (cards.Length != 1)
        {
            throw new InvalidOperationException("Review total requires one visible app-owned price-card container.");
        }
        // iOS combines each row's title/value into an accessible element within the owned card.
        // No global label locator or positional row matching is used.
        return cards[0].Descendants().Where(element => element.Attribute("accessible")?.Value == "true")
            .Select(element => element.Attribute("label")?.Value ?? string.Empty)
            .Where(value => !value.IsNullOrWhiteSpace()).ToArray();
    }

    public SelectedPayWithPointsOption SelectFirstPayWithPointsOption()
    {
        try
        {
            WaitForElementByKey(NativeBookingAccessibilityIds.PayWithPointsCard, ScreenTimeout);
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new WebDriverTimeoutException(
                "The logged-in currency journey reached payment without exposing the pay-with-points card. " +
                "Confirm that reward-seat search was disabled and the app retained the pay-with-points payment mode. " +
                $"Visible controls: {DescribeVisibleControls()}",
                exception);
        }

        OpenQA.Selenium.Support.UI.WebDriverWait optionsWait = new(Driver, PayWithPointsLoadTimeout)
        {
            PollingInterval = TimeSpan.FromMilliseconds(500),
        };
        IWebElement selector;
        try
        {
            selector = optionsWait.Until(_ =>
            {
                IWebElement? availableSelector = FindDisplayedByKey(
                    NativeBookingAccessibilityIds.PayWithPointsOptionSelector);
                if (availableSelector is not null)
                {
                    return availableSelector;
                }

                IWebElement? insufficientBalance = FindDisplayedByKey(
                    NativeBookingAccessibilityIds.PayWithPointsInsufficientBalance);
                if (insufficientBalance is not null)
                {
                    throw new InvalidOperationException(
                        "Pay with Points did not offer selectable values because the member balance was insufficient. " +
                        $"App message: {ReadElementText(insufficientBalance)}");
                }

                IWebElement? unavailable = FindDisplayedByKey(
                    NativeBookingAccessibilityIds.PayWithPointsOptionsUnavailable);
                if (unavailable is not null)
                {
                    throw new InvalidOperationException(
                        "Pay with Points finished loading without selectable values. " +
                        $"App message: {ReadElementText(unavailable)}");
                }

                return null;
            });
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new WebDriverTimeoutException(
                $"Pay with Points options did not finish loading within {PayWithPointsLoadTimeout.TotalSeconds:N0} seconds. " +
                $"Visible controls: {DescribeVisibleControls()}",
                exception);
        }
        selector.Click();
        IWebElement option = WaitForElementByKey(
            NativeBookingAccessibilityIds.PayWithPointsOption(0),
            ScreenTimeout);
        string selectedOption = ReadElementText(option);
        option.Click();
        if (selectedOption.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(
                "The selected Pay with Points option did not expose its displayed value.");
        }

        OpenQA.Selenium.Support.UI.WebDriverWait selectionWait = new(Driver, ScreenTimeout);
        selectionWait.Until(_ =>
        {
            IWebElement? updatedSelector = FindDisplayedByKey(
                NativeBookingAccessibilityIds.PayWithPointsOptionSelector);
            return updatedSelector is not null &&
                ReadElementText(updatedSelector).Contains(
                    selectedOption,
                    StringComparison.OrdinalIgnoreCase);
        });

        return new SelectedPayWithPointsOption(selectedOption);
    }

    public void VerifyAndAcceptCardPresentation()
    {
        OpenQA.Selenium.Support.UI.WebDriverWait wait = new(Driver, TimeSpan.FromMinutes(3))
        {
            Message = "Expected card-presentation verification after HPP for the short-notice LOS journey."
        };
        wait.Until(_ =>
        {
            NativePaymentFailure.ThrowIfDisplayed(
                locator => Driver.FindElements(locator).Any(element => element.Displayed),
                "while waiting for card-presentation verification");
            if (IsDisplayedByKey(NativeBookingAccessibilityIds.ConfirmationTitle))
            {
                throw new InvalidOperationException("Booking confirmed without the required card-presentation check.");
            }
            return IsDisplayedByKey(NativeBookingAccessibilityIds.CardPresentationPage);
        });
        string heading = ReadCombinedText(NativeBookingAccessibilityIds.CardPresentationHeading);
        string introduction = ReadCombinedText(NativeBookingAccessibilityIds.CardPresentationIntroduction);
        if (!heading.Contains("We need to do checks at the airport", StringComparison.Ordinal) ||
            !introduction.Contains("Your booking isn’t confirmed yet and action is required.", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Card-presentation heading or unconfirmed-booking guidance did not match the approved design.");
        }
        // SwiftUI exposes the group's ID on each of its four accessible paragraphs.
        // Read every visible match, not just the first paragraph or unrelated page text.
        IReadOnlyList<string> details = Driver.FindElements(
                MobileBy.AccessibilityId(NativeBookingAccessibilityIds.CardPresentationDetails))
            .Where(element => element.Displayed).Select(ReadElementText).ToArray();
        VerifyCardPresentationInstructions(details);
        IWebElement cancel = ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.CardPresentationCancel);
        if (!cancel.Enabled)
        {
            throw new InvalidOperationException("Cancel booking was disabled on card-presentation verification.");
        }
        IWebElement agree = ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.CardPresentationAgree);
        if (!agree.Enabled)
        {
            throw new InvalidOperationException("Agree and confirm booking was disabled.");
        }
        // A single consent action only. Never retry payment or agreement on a failed response.
        agree.Click();
    }

    internal static void VerifyCardPresentationInstructions(IEnumerable<string> paragraphs)
    {
        string details = string.Join(" ", paragraphs);
        string[] requiredInstructions = ["credit card holder", "photo identification", "You won’t be able to travel otherwise.",
            "You won’t be able to check in online", "Your flight(s) are not being held"];
        foreach (string instruction in requiredInstructions)
        {
            if (!details.Contains(instruction, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Card-presentation details did not expose required instruction: {instruction}");
            }
        }
    }

    public BookingConfirmationSummary ReadConfirmation()
    {
        WaitForConfirmationOrPaymentFailure();
        IWebElement travellers = ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.ConfirmationPassengerCount);
        string?[] travellerNamesEvidence =
            [travellers.GetAttribute("label"), travellers.GetAttribute("value"), travellers.Text];
        return new BookingConfirmationSummary(
            ReadCombinedText(NativeBookingAccessibilityIds.ConfirmationBookingReference),
            travellerNamesEvidence,
            ReadCombinedText(NativeBookingAccessibilityIds.ConfirmationTotalPrice));
    }

    public bool WaitForControlledPostHppError(string expectedMessage, bool requestTimeout)
    {
        OpenQA.Selenium.Support.UI.WebDriverWait errorWait = new(Driver,
            requestTimeout ? TimeSpan.FromSeconds(180) : ScreenTimeout)
        {
            Message = "Expected dedicated payment error with exact title/message and enabled NativeFlightPaymentErrorReturnToSearch; no payment retry is permitted."
        };
        return errorWait.Until(_ =>
        {
            if (IsDisplayedByKey(NativeBookingAccessibilityIds.ConfirmationTitle))
                throw new InvalidOperationException("Booking confirmation appeared after an intercepted post-HPP failure.");
            try
            {
                if (!IsDisplayedByKey(NativeBookingAccessibilityIds.PaymentErrorPage))
                    return false;
                if (IsDisplayedByKey(NativeBookingAccessibilityIds.PaymentRetryButton))
                    throw new InvalidOperationException("Captured-payment failure must not offer a payment retry.");
                IWebElement? title = FindDisplayedByKey(NativeBookingAccessibilityIds.PaymentErrorTitle);
                IWebElement? error = FindDisplayedByKey(NativeBookingAccessibilityIds.PaymentErrorMessage);
                IWebElement? returnToSearch = FindDisplayedByKey(NativeBookingAccessibilityIds.PaymentErrorReturnToSearch);
                return title is not null && title.GetAttribute("label")?.Trim() == "We couldn’t confirm your booking"
                    && error is not null && error.GetAttribute("label")?.Trim() == expectedMessage
                    && returnToSearch is not null && returnToSearch.Enabled
                    && returnToSearch.GetAttribute("label")?.Trim() == "Return to flight search";
            }
            catch (StaleElementReferenceException)
            {
                // SwiftUI may replace the controls as the error appears. Re-read them
                // on the next bounded poll; never click retry or resubmit payment.
                return false;
            }
        });
        // Observe only. Never click retry after payment or resubmit a booking.
    }

    private void WaitForConfirmationOrPaymentFailure()
    {
        OpenQA.Selenium.Support.UI.WebDriverWait confirmationWait =
            new(Driver, TimeSpan.FromMinutes(3))
            {
                PollingInterval = TimeSpan.FromMilliseconds(250),
            };
        confirmationWait.Until(_ =>
        {
            NativePaymentFailure.ThrowIfDisplayed(
                locator => Driver.FindElements(locator).Any(element => element.Displayed),
                "while waiting for booking confirmation");
            bool confirmationDisplayed = Driver
                .FindElements(MobileBy.AccessibilityId(NativeBookingAccessibilityIds.ConfirmationTitle))
                .Any(element => element.Displayed);
            if (confirmationDisplayed)
            {
                return true;
            }

            return false;
        });
    }

    public void ReturnToBookingHome()
    {
        ScrollToKeyWithoutSleep(NativeBookingAccessibilityIds.ConfirmationReturnHomeButton).Click();
    }

    private string ReadCombinedText(string key)
    {
        IWebElement element = WaitForElementByKey(key, ScreenTimeout);
        return $"{element.GetAttribute("label")} {element.GetAttribute("value")} {element.Text}".Trim();
    }

    private void EnterVerifiedTextAndDismissKeyboard(string key, string expectedValue)
    {
        for (int attempt = 1; attempt <= 2; attempt++)
        {
            IWebElement field = WaitForElementByKey(key, ScreenTimeout);
            field.Clear();
            field.SendKeys(expectedValue);
            DismissKeyboard(field);

            IWebElement updatedField = WaitForElementByKey(key, ScreenTimeout);
            string actualValue = updatedField.GetAttribute("value") ?? updatedField.Text;
            if (actualValue.Equals(expectedValue, StringComparison.Ordinal))
            {
                return;
            }
        }

        string finalValue = WaitForElementByKey(key, ScreenTimeout).GetAttribute("value") ?? string.Empty;
        throw new InvalidOperationException(
            $"'{key}' did not retain the expected value. Expected '{expectedValue}', found '{finalValue}'.");
    }

    private string ReadFieldValue(string key)
    {
        IWebElement field = WaitForElementByKey(key, ScreenTimeout);
        return field.GetAttribute("value") ?? field.Text ?? string.Empty;
    }

    private static string ReadElementText(IWebElement element)
        => $"{element.GetAttribute("label")} {element.GetAttribute("value")} {element.Text}".Trim();

    private void DismissKeyboard(IWebElement field)
    {
        // BrowserStack's XCUITest endpoint does not implement Appium hideKeyboard.
        // Match the established mobile harness by using the visible iOS Return key.
        field.SendKeys(Keys.Return);
    }

    private void WaitForKeyVisibility(string key, bool shouldBeVisible)
    {
        OpenQA.Selenium.Support.UI.WebDriverWait visibilityWait = new(Driver, ScreenTimeout)
        {
            PollingInterval = TimeSpan.FromMilliseconds(250),
        };
        visibilityWait.Until(_ =>
        {
            bool isVisible = Driver.FindElements(MobileBy.AccessibilityId(key))
                .Any(element => element.Displayed);
            return isVisible == shouldBeVisible;
        });
    }

    private static string ReadAccessibleLabel(IWebElement element)
    {
        string label = element.GetAttribute("label") ?? string.Empty;
        return label.IsNullOrWhiteSpace() ? element.Text.Trim() : label.Trim();
    }

    private IWebElement? FindDisplayedByKey(string key)
    {
        return Driver.FindElements(MobileBy.AccessibilityId(key))
            .FirstOrDefault(element =>
            {
                try
                {
                    return element.Displayed;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
    }

    private bool HasSelectedLegacyGender(int travellerIndex)
    {
        foreach (string gender in new[] { "Female", "Male", "Undisclosed" })
        {
            IWebElement button = ScrollToKeyWithoutSleep(
                NativeBookingAccessibilityIds.LegacyTravellerGenderButton(travellerIndex, gender),
                maximumScrolls: 4);
            string value = button.GetAttribute("value") ?? string.Empty;
            if (button.Selected || value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    internal static string PassengerHubScrollContainer(bool redesigned)
        => redesigned ? NativeBookingAccessibilityIds.PassengerDetailsHub
            : NativeBookingAccessibilityIds.PassengerDetailsScrollView;

    private IWebElement ScrollToKeyWithoutSleep(string key, int maximumScrolls = 8)
    {
        IWebElement? visibleElement = TryWaitForElementByKey(key, TimeSpan.FromSeconds(1));
        if (visibleElement is not null && visibleElement.Displayed)
        {
            return visibleElement;
        }

        try
        {
            Driver.ExecuteScript("mobile: scroll", new Dictionary<string, object>
            {
                ["name"] = key,
                ["toVisible"] = true,
            });
            visibleElement = TryWaitForElementByKey(key, TimeSpan.FromSeconds(1));
            if (visibleElement is not null && visibleElement.Displayed)
            {
                return visibleElement;
            }
        }
        catch (WebDriverException)
        {
            // Older provider versions may not support name-targeted iOS scrolling.
            // Continue with the bounded directional fallback below.
        }

        for (int attempt = 0; attempt <= maximumScrolls; attempt++)
        {
            visibleElement = TryWaitForElementByKey(key, TimeSpan.FromSeconds(1));
            if (visibleElement is not null && visibleElement.Displayed)
            {
                return visibleElement;
            }

            if (attempt < maximumScrolls)
            {
                Driver.ExecuteScript("mobile: scroll", new Dictionary<string, object>
                {
                    ["direction"] = "down",
                });
            }
        }

        throw new NoSuchElementException($"Could not find '{key}' after {maximumScrolls} semantic scrolls.");
    }

    private IWebElement ScrollToSummaryContinueButton()
    {
        const string key = NativeBookingAccessibilityIds.SummaryContinueButton;
        const int maximumScrolls = 4;

        for (int attempt = 0; attempt <= maximumScrolls; attempt++)
        {
            IWebElement? continueButton = TryWaitForElementByKey(key, TimeSpan.FromMilliseconds(250));
            if (continueButton is not null && continueButton.Displayed)
            {
                return continueButton;
            }

            if (attempt < maximumScrolls)
            {
                Driver.ExecuteScript("mobile: scroll", new Dictionary<string, object>
                {
                    ["direction"] = "down",
                });
            }
        }

        throw new NoSuchElementException(
            $"Could not find the flight-summary continue button after {maximumScrolls} bounded scrolls.");
    }

}

public sealed record SelectedPayWithPointsOption(string DisplayText);

internal enum PassengerDetailsDestination
{
    PaymentHandoff,
    SeatSelection
}

internal sealed record PassengerDetailsDestinationResult(PassengerDetailsDestination Destination);

public sealed record BookingConfirmationSummary(
    string BookingReferenceText,
    IReadOnlyList<string?> TravellerNamesEvidence,
    string TotalPriceText);
