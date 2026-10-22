using System.Diagnostics;
using System.Xml.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

public sealed class NativeSeatSelectionPage(AppiumDriver driver, Action<string>? report = null)
    : NativeAppPage(driver, TimeSpan.FromSeconds(30))
{
    public void WaitForChildGuidanceDisplayed()
        => Wait.Until(_ =>
        {
            IWebElement[] sheets = Driver.FindElements(
                    MobileBy.AccessibilityId(NativeBookingAccessibilityIds.SeatSelectionChildGuidanceSheet))
                .Where(element => element.Displayed).ToArray();
            if (sheets.Length > 1)
                throw new InvalidOperationException("Child seat guidance sheet is ambiguous.");
            return sheets.Length == 1;
        });

    public void SubmitSelectionExpectingControlledFailure()
        => FindAction(NativeBookingAccessibilityIds.SeatSelectionContinueButton).Click();

    public void WaitForMapUnavailableMessage()
    {
        const string expectedTitle = "Seat map unavailable";
        const string expectedMessage = "We can’t display the seat map for this flight. Please continue with your booking";
        const string expectedSupportingMessage =
            "Once your booking is complete, you can select your seats anytime in My Trips.";
        Wait.Until(_ =>
        {
            IWebElement[] errorElements = Driver
                .FindElements(MobileBy.AccessibilityId(NativeBookingAccessibilityIds.SeatSelectionError))
                .Where(element => element.Displayed).ToArray();
            string errorText = string.Join(" ", errorElements.Select(element => string.Join(" ",
                element.Text, element.GetAttribute("label"), element.GetAttribute("value"))));
            IWebElement? proceed = VisibleControl(NativeBookingAccessibilityIds.SeatSelectionContinueButton);
            return errorElements.Length > 0 && proceed is not null && proceed.Enabled
                && errorText.Contains(expectedTitle, StringComparison.Ordinal)
                && errorText.Contains(expectedMessage, StringComparison.Ordinal)
                && errorText.Contains(expectedSupportingMessage, StringComparison.Ordinal)
                && VisibleControl(NativeBookingAccessibilityIds.SeatSelectionRetry) is null
                && VisibleControl(NativeBookingAccessibilityIds.SeatSelectionLoading) is null;
        });
    }

    public void ContinuePastUnavailableMap()
    {
        IWebElement proceed = VisibleControl(NativeBookingAccessibilityIds.SeatSelectionContinueButton)
            ?? throw new InvalidOperationException("Unavailable-map Continue action was not visible.");
        if (!proceed.Enabled)
            throw new InvalidOperationException("Unavailable-map Continue action was disabled.");
        proceed.Click();
    }

    public void WaitForAvailableMap()
        => Wait.Until(_ =>
        {
            if (VisibleControl(NativeBookingAccessibilityIds.SeatSelectionError) is not null)
                throw new InvalidOperationException("The next eligible sector also displayed the unavailable-map state.");
            return VisibleControl(NativeBookingAccessibilityIds.SeatSelectionPassengers) is not null
                && VisibleControl(NativeBookingAccessibilityIds.SeatSelectionViewport) is not null
                && VisibleControl(NativeBookingAccessibilityIds.SeatSelectionLoading) is null;
        });

    public void WaitForPaymentHandoff()
        => Wait.Until(_ => VisibleControl(NativeBookingAccessibilityIds.PaymentHandoffTitle) is not null);

    public bool WaitForControlledError(string expectedMessage, bool submission, string? selectedSeat = null)
    {
        string errorId = submission ? NativeBookingAccessibilityIds.SeatSelectionSubmissionError
            : NativeBookingAccessibilityIds.SeatSelectionError;
        string recoveryId = submission ? NativeBookingAccessibilityIds.SeatSelectionContinueButton
            : NativeBookingAccessibilityIds.SeatSelectionRetry;
        return Wait.Until(_ =>
        {
            // Do not call the normal ThrowIfFailed here: this scenario expects one specific error.
            if (VisibleControl(NativeBookingAccessibilityIds.PaymentHandoffTitle) is not null
                || VisibleControl(NativeBookingAccessibilityIds.ConfirmationTitle) is not null)
                throw new InvalidOperationException("Controlled seat failure unexpectedly progressed beyond seat selection. No further actions performed.");
            IWebElement? error = VisibleControl(errorId);
            IWebElement? recovery = VisibleControl(recoveryId);
            if (error is null || recovery is null || !recovery.Enabled
                || VisibleControl(NativeBookingAccessibilityIds.SeatSelectionTitle) is null
                || VisibleControl(NativeBookingAccessibilityIds.SeatSelectionLoading) is not null)
                return false;
            string text = string.Join(" ", error.Text, error.GetAttribute("label"), error.GetAttribute("value"));
            if (!text.Contains(expectedMessage, StringComparison.Ordinal))
                return false;
            // The on-screen assignment is a draft, not evidence of a successful backend save.
            if (submission)
            {
                IWebElement? passenger = VisibleControl(NativeBookingAccessibilityIds.SeatPassengerOption(0));
                return passenger is not null && selectedSeat is not null
                    && NativeBookingPage.PassengerHasSeat(ReadRequiredLabel(passenger), selectedSeat);
            }
            return true;
        });
    }

    private IWebElement? VisibleControl(string identifier)
    {
        IWebElement[] visible = Driver.FindElements(MobileBy.AccessibilityId(identifier))
            .Where(element => element.Displayed).ToArray();
        if (visible.Length > 1)
            throw new InvalidOperationException($"Seat error-state control '{identifier}' is ambiguous.");
        return visible.SingleOrDefault();
    }

    public decimal ReadSeatCharge()
        => SeatPriceEvidence.ReadGbp(ReadRequiredLabel(FindAction(NativeBookingAccessibilityIds.SeatSelectionPrice)));

    public decimal ReadOpenedSeatCharge()
    {
        string label = ReadRequiredLabel(FindAction(NativeBookingAccessibilityIds.SeatSelectionDetailPrice));
        return SeatPriceEvidence.ReadDetailGbp(label);
    }

    public decimal ReadTripTotal()
        => SeatPriceEvidence.ReadGbp(ReadRequiredLabel(FindAction(NativeBookingAccessibilityIds.SeatSelectionTripTotal)));

    public (string Number, decimal Price) OpenPaidSeat(decimal? differentFromPrice = null)
    {
        // One in-memory snapshot avoids materialising every seat as a remote element.
        // Revalidate the discovered owned ID before interaction; never tap offscreen seats.
        for (int scroll = 0; scroll <= 3; scroll++)
        {
            ThrowIfFailed();
            System.Drawing.Rectangle safeArea = Timed("discovery safe area", () => ReadSafeSeatArea(RequireSeatViewport()));
            // Visibility calculation across the entire map is expensive. Omit it only from this
            // discovery snapshot; OpenSeat still requires fresh visibility, enabled and hittable.
            XDocument source = Timed("discovery snapshot", () => XDocument.Parse(
                Driver.ExecuteScript("mobile: source", new Dictionary<string, object>
                {
                    ["format"] = "xml",
                    ["excludedAttributes"] = "visible,accessible",
                }) as string ?? throw new InvalidOperationException("Seat discovery returned no XML source. No tap attempted.")));
            int inspected = 0;
            foreach (XElement node in source.Descendants("XCUIElementTypeButton"))
            {
                string id = (string?)node.Attribute("name") ?? string.Empty;
                string label = (string?)node.Attribute("label") ?? string.Empty;
                if (!id.StartsWith(NativeBookingAccessibilityIds.SeatPrefix, StringComparison.Ordinal) ||
                    !((string?)node.Attribute("enabled")).Is("true") || !label.Contains('£'))
                    continue;
                decimal advertised = SeatPriceEvidence.ReadGbp(label);
                if (advertised <= 0 || advertised == differentFromPrice)
                    continue;
                if (!SnapshotSeatFits(node, safeArea))
                    continue;
                if (++inspected > 8)
                    throw new InvalidOperationException("Seat discovery exceeded eight in-bounds candidates. No further interaction attempted.");
                string number = id[NativeBookingAccessibilityIds.SeatPrefix.Length..];
                if (number.IsNullOrWhiteSpace())
                    throw new InvalidOperationException("Paid seat identifier has no seat number.");
                report?.Invoke($"Seat candidate: id={id}; price=GBP {advertised:F2}; scroll={scroll}.");
                // Snapshot filters candidates only. OpenSeat performs the fresh live safety check once.
                OpenSeat(number);
                decimal detail = SeatPriceEvidence.ReadGbp(ReadRequiredLabel(
                    FindAction(NativeBookingAccessibilityIds.SeatSelectionDetailPrice)));
                if (detail != advertised)
                    throw new InvalidOperationException("Inspected seat price differs from its advertised map price.");
                report?.Invoke($"Seat pricing: discovered paid offer GBP {detail:F2}.");
                return (number, detail);
            }
            if (scroll == 3)
                break;
            ScrollSeatViewport();
        }
        throw new InvalidOperationException(
            "No visible enabled paid GBP seat with the required distinct price. Inventory/owned-scroll contract required; no blind swipe or substitute assertion.");
    }

    public void WaitForPrices(decimal expectedCharge, decimal expectedTotal)
    {
        Timed("price assertion", () => Wait.Until(_ =>
        {
            ThrowIfFailed();
            return TryReadPrice(NativeBookingAccessibilityIds.SeatSelectionPrice) == expectedCharge &&
                TryReadPrice(NativeBookingAccessibilityIds.SeatSelectionTripTotal) == expectedTotal;
        }));
        report?.Invoke($"Seat pricing verified: charge GBP {expectedCharge:F2}; trip total GBP {expectedTotal:F2}.");
    }

    private decimal? TryReadPrice(string identifier)
    {
        IWebElement[] matches = Driver.FindElements(MobileBy.AccessibilityId(identifier))
            .Where(element => element.Displayed && element.Enabled).ToArray();
        if (matches.Length > 1)
            throw new InvalidOperationException($"Seat price '{identifier}' is ambiguous.");
        return matches.Length == 0 ? null : SeatPriceEvidence.ReadGbp(ReadRequiredLabel(matches[0]));
    }

    private T Timed<T>(string stage, Func<T> operation)
    {
        Stopwatch elapsed = Stopwatch.StartNew();
        try
        { return operation(); }
        finally { report?.Invoke($"Seat timing: {stage} = {elapsed.Elapsed.TotalSeconds:F2}s"); }
    }

    public string ReadPassengerAssignment(int passengerIndex = 0)
        => ReadRequiredLabel(FindPassengerState(passengerIndex));

    public int ReadAssignablePassengerCount()
    {
        string? value = FindAction(NativeBookingAccessibilityIds.SeatSelectionPassengers).GetAttribute("value");
        if (!int.TryParse(value, System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out int count) || count < 1)
            throw new InvalidOperationException(
                "Seat passenger container must expose a positive integer count. Rebuild with the PR1273 accessibility contract; visible child count is not a safe fallback.");
        return count;
    }

    public void SelectPassenger(int passengerIndex)
    {
        FindAction(NativeBookingAccessibilityIds.SeatPassengerOption(passengerIndex)).Click();
        WaitForActivePassenger(passengerIndex);
    }

    public void WaitForActivePassenger(int passengerIndex)
        => Wait.Until(_ =>
        {
            ThrowIfFailed();
            return FindPassengerState(passengerIndex).Selected;
        });

    public void WaitForSeatUnavailable(string number)
        => Wait.Until(_ =>
        {
            ThrowIfFailed();
            IWebElement seat = Driver.FindElement(MobileBy.AccessibilityId(NativeBookingAccessibilityIds.SeatPrefix + number));
            return seat.Displayed && !seat.Enabled;
        });

    private IWebElement FindPassengerState(int passengerIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(passengerIndex);
        // Auto-advance can move the previous card partly offscreen. Reading its owned
        // state must not click it and thereby hide a broken automatic transition.
        return Driver.FindElement(MobileBy.AccessibilityId(NativeBookingAccessibilityIds.SeatPassengerOption(passengerIndex)));
    }

    public string OpenAvailableSeat(string? excludingSeat = null)
    {
        Stopwatch duration = Stopwatch.StartNew();
        try
        {
            // Do not predicate on visible: XCTest can hit-test the entire large seat tree.
            // Narrow by the owned identifier/enabled state, then inspect candidates only.
            IWebElement seat = Wait.Until(_ =>
            {
                Stopwatch probe = Stopwatch.StartNew();
                ThrowIfFailed();
                report?.Invoke($"Seat timing: error lookup = {probe.Elapsed.TotalSeconds:F2}s");
                probe.Restart();
                HashSet<string> excluded = [];
                if (excludingSeat is not null)
                {
                    excluded.Add(NativeBookingAccessibilityIds.SeatPrefix + excludingSeat);
                }
                // Returning every seat makes WDA materialise/cache the entire map (127 elements
                // took ~39s locally). Request one candidate, never a guessed fixed seat number.
                for (int inspected = 1; inspected <= 8; inspected++)
                {
                    string exclusions = string.Concat(excluded.Select(id =>
                        " AND name != '" + id.Replace("\\", "\\\\").Replace("'", "\\'") + "'"));
                    IWebElement candidate;
                    try
                    {
                        candidate = Driver.FindElement(MobileBy.IosNSPredicate(
                            $"type == 'XCUIElementTypeButton' AND name BEGINSWITH '{NativeBookingAccessibilityIds.SeatPrefix}' " +
                            "AND enabled == true" + exclusions));
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }
                    string id = candidate.GetAttribute("name") ?? string.Empty;
                    if (IsSafeSeat(candidate))
                    {
                        report?.Invoke($"Seat timing: candidate lookup/visibility = {probe.Elapsed.TotalSeconds:F2}s; inspected={inspected}");
                        return candidate;
                    }
                    excluded.Add(id);
                }
                throw new InvalidOperationException(
                    "No visible offered seat within eight candidates. An owned map-scroll contract is required; no blind swipe attempted.");
            });
            string identifier = seat.GetAttribute("name") ?? string.Empty;
            string number = identifier[NativeBookingAccessibilityIds.SeatPrefix.Length..];
            if (number.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("Offered seat identifier has no seat number.");
            }
            Stopwatch interaction = Stopwatch.StartNew();
            if (!IsSafeSeat(seat))
                throw new InvalidOperationException("Seat is no longer safe to tap. No tap attempted.");
            seat.Click();
            report?.Invoke($"Seat timing: tap = {interaction.Elapsed.TotalSeconds:F2}s");
            interaction.Restart();
            FindAction(NativeBookingAccessibilityIds.SeatSelectionConfirm);
            report?.Invoke($"Seat timing: detail action ready = {interaction.Elapsed.TotalSeconds:F2}s");
            return number;
        }
        finally
        {
            report?.Invoke($"Seat timing: discover/tap/detail ready = {duration.Elapsed.TotalSeconds:F2}s");
        }
    }

    public void OpenSeat(string number)
    {
        IWebElement seat = Timed("seat lookup", () => FindAction(NativeBookingAccessibilityIds.SeatPrefix + number));
        if (!Timed("fresh pre-tap safety", () => IsSafeSeat(seat)))
            throw new InvalidOperationException("Seat is obscured, outside safe bounds or not hittable. No tap attempted.");
        report?.Invoke($"Seat tap: id={NativeBookingAccessibilityIds.SeatPrefix + number}.");
        Timed("seat tap", () => { seat.Click(); return true; });
        Timed("detail ready", () => FindAction(NativeBookingAccessibilityIds.SeatSelectionConfirm));
    }

    private bool IsSafeSeat(IWebElement seat)
    {
        IWebElement viewport = RequireSeatViewport();
        System.Drawing.Rectangle safeArea = ReadSafeSeatArea(viewport);
        return seat.Displayed && seat.Enabled && safeArea.Contains(new System.Drawing.Rectangle(seat.Location, seat.Size)) &&
            seat.GetAttribute("hittable").Is("true");
    }

    private System.Drawing.Rectangle ReadSafeSeatArea(IWebElement viewport)
    {
        IWebElement header = Driver.FindElement(MobileBy.AccessibilityId(NativeBookingAccessibilityIds.SeatSelectionTitle));
        IWebElement cart = Driver.FindElement(MobileBy.AccessibilityId(NativeBookingAccessibilityIds.SeatSelectionPrice));
        System.Drawing.Size window = Driver.Manage().Window.Size;
        if (!header.Displayed || !cart.Displayed)
            throw new InvalidOperationException("Seat safety anchors are not visible. No interaction attempted.");
        // Reserve a measured label-height above the cart row for its padding/material boundary.
        return SafeSeatArea(new System.Drawing.Rectangle(viewport.Location, viewport.Size), window,
            header.Location.Y + header.Size.Height, cart.Location.Y - cart.Size.Height);
    }

    internal static System.Drawing.Rectangle SafeSeatArea(System.Drawing.Rectangle viewport,
        System.Drawing.Size window, int headerBottom, int cartTop)
    {
        if (window.Width <= 0 || window.Height <= 0 || headerBottom < 0 || cartTop <= headerBottom ||
            viewport.Width <= 0 || viewport.Height <= 0)
            throw new InvalidOperationException("Invalid seat safety geometry. No interaction attempted.");
        System.Drawing.Rectangle area = System.Drawing.Rectangle.Intersect(viewport,
            System.Drawing.Rectangle.FromLTRB(0, headerBottom + 1, window.Width, Math.Min(window.Height, cartTop - 1)));
        if (area.Width < 40 || area.Height < 80)
            throw new InvalidOperationException("Insufficient unobscured seat area. No interaction attempted.");
        return area;
    }

    private static System.Drawing.Rectangle SnapshotBounds(XElement node)
    {
        int Read(string attribute) => int.TryParse((string?)node.Attribute(attribute), out int value) ? value : -1;
        return new(Read("x"), Read("y"), Read("width"), Read("height"));
    }

    internal static bool SnapshotSeatFits(XElement node, System.Drawing.Rectangle safeArea)
    {
        System.Drawing.Rectangle bounds = SnapshotBounds(node);
        return bounds.Width > 0 && bounds.Height > 0 && safeArea.Contains(bounds);
    }

    internal static bool SeatBoundsAreSafe(System.Drawing.Rectangle seat, int width, int headerBottom, int cartTop)
        => width > 0 && headerBottom >= 0 && cartTop > headerBottom && seat.Width > 0 && seat.Height > 0 &&
            seat.Left >= 0 && seat.Right <= width && seat.Top > headerBottom && seat.Bottom < cartTop;

    private void ScrollSeatViewport()
    {
        ThrowIfFailed();
        IWebElement viewport = RequireSeatViewport();
        System.Drawing.Rectangle area = ReadSafeSeatArea(viewport);
        report?.Invoke($"Seat discovery: native upward drag in measured safe area {area} (maximum three).");
        // Native drag starts/ends well inside the measured viewport intersection, never the fixed cart.
        // These are runtime geometry-derived gesture points, not fallback tap coordinates.
        int x = area.Left + area.Width / 2;
        Timed("bounded scroll", () => Driver.ExecuteScript("mobile: dragFromToForDuration", new Dictionary<string, object>
        {
            ["duration"] = 0.1,
            ["fromX"] = x,
            ["fromY"] = area.Top + area.Height * 0.75,
            ["toX"] = x,
            ["toY"] = area.Top + area.Height * 0.25,
        }));
        ThrowIfFailed();
    }

    private IWebElement RequireSeatViewport()
    {
        // The grid's full content bounds are not a scroll viewport. Require the owning native scroll view.
        IWebElement[] matches = Driver.FindElements(MobileBy.AccessibilityId(NativeBookingAccessibilityIds.SeatSelectionViewport))
            .Where(element => element.Displayed && element.Enabled).ToArray();
        if (matches.Length != 1 || !matches[0].GetAttribute("type").Is("XCUIElementTypeScrollView"))
            throw new InvalidOperationException("Safe seat interaction requires the app-owned seatSelection.viewport ID on the vertical ScrollView. No tap/gesture attempted.");
        return matches[0];
    }

    public void ConfirmSeat(string number, int passengerIndex = 0)
    {
        Stopwatch duration = Stopwatch.StartNew();
        try
        {
            FindAction(NativeBookingAccessibilityIds.SeatSelectionConfirm).Click();
            Wait.Until(_ => NativeBookingPage.PassengerHasSeat(ReadPassengerAssignment(passengerIndex), number));
        }
        finally
        {
            report?.Invoke($"Seat timing: confirm/assignment = {duration.Elapsed.TotalSeconds:F2}s");
        }
    }

    public void RemoveSeat(string number, string originalAssignment, int passengerIndex = 0)
    {
        OpenSeat(number);
        IWebElement action = FindAction(NativeBookingAccessibilityIds.SeatSelectionConfirm);
        if (!ReadRequiredLabel(action).Is("Remove seat"))
        {
            throw new InvalidOperationException("Reopening the assigned seat did not offer Remove seat.");
        }
        action.Click();
        WaitForAssignment(originalAssignment, passengerIndex);
    }

    public void CloseDetails(string expectedAssignment)
    {
        FindAction(NativeBookingAccessibilityIds.SeatSelectionDetailClose).Click();
        WaitForAssignment(expectedAssignment);
    }

    public void OpenAndCloseKey(string expectedAssignment)
    {
        FindAction(NativeBookingAccessibilityIds.SeatSelectionLegend).Click();
        FindAction(NativeBookingAccessibilityIds.SeatSelectionLegendClose).Click();
        WaitForAssignment(expectedAssignment);
    }

    public void WaitForAssignment(string expectedAssignment, int passengerIndex = 0)
        => Wait.Until(_ => ReadPassengerAssignment(passengerIndex).Is(expectedAssignment));

    internal static bool IsUnassignedPassenger(string? label)
        => (label ?? string.Empty).Split(',').Any(part => part.Trim().Is("Choose a seat"));

    private IWebElement FindAction(string identifier)
    {
        OpenQA.Selenium.Support.UI.WebDriverWait actionWait = new(Driver, TimeSpan.FromSeconds(30))
        {
            Message = $"Seat action '{identifier}' must be uniquely visible and enabled. No coordinate/label fallback used.",
        };
        return actionWait.Until(_ =>
        {
            ThrowIfFailed();
            IWebElement[] matches = Driver.FindElements(MobileBy.AccessibilityId(identifier))
                .Where(element => element.Displayed && element.Enabled).ToArray();
            if (matches.Length > 1)
            {
                throw new InvalidOperationException($"Seat action '{identifier}' is ambiguous ({matches.Length} matches).");
            }
            return matches.SingleOrDefault();
        });
    }

    private void ThrowIfFailed()
    {
        string[] errors = [NativeBookingAccessibilityIds.PaymentHandoffTitle, NativeBookingAccessibilityIds.SeatSelectionError,
            NativeBookingAccessibilityIds.SeatSelectionUnavailable,
            NativeBookingAccessibilityIds.SeatSelectionSubmissionError];
        string names = string.Join(",", errors.Select(id => $"'{id}'"));
        foreach (IWebElement element in Driver.FindElements(MobileBy.IosNSPredicate($"name IN {{{names}}}")))
        {
            if (!element.Displayed)
                continue;
            if (element.GetAttribute("name").Is(NativeBookingAccessibilityIds.PaymentHandoffTitle))
                throw new InvalidOperationException("Unexpected Payment navigation during seat interaction. Stopped without further taps; seat submission/skip outcome is unknown.");
            throw new InvalidOperationException("Seat map exposed an error/unavailable state. No retry or skip attempted.");
        }
    }

    private static string ReadRequiredLabel(IWebElement element)
    {
        string label = element.GetAttribute("label") ?? string.Empty;
        if (label.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("Seat control exposes no readable state.");
        }
        return label;
    }
}
