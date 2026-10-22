using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interfaces;
using OpenQA.Selenium.Support.UI;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.UI.PageObjects.DotCom;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

public sealed class NativeHostedPaymentPage
{
    private readonly AppiumDriver _driver;

    public NativeHostedPaymentPage(AppiumDriver driver)
    {
        _driver = driver;
    }

    public void Pay(PaymentCardDetails payment, Action? beforePaymentSubmission = null)
    {
        OpenQA.Selenium.IWebElement? card = WaitForCardOrPaymentFailure();
        if (card is not null)
        {
            PayThroughNativeAccessibility(card, payment, beforePaymentSubmission);
            return;
        }

        PayThroughWebView(payment, beforePaymentSubmission);
    }

    private OpenQA.Selenium.IWebElement? WaitForCardOrPaymentFailure()
    {
        WebDriverWait paymentWait = new(_driver, TimeSpan.FromSeconds(60));
        return paymentWait.Until(_ =>
        {
            NativePaymentFailure.ThrowIfDisplayed(
                locator => _driver.FindElements(locator).Any(element => element.Displayed),
                "while waiting for HPP to open");
            OpenQA.Selenium.IWebElement? card = _driver
                .FindElements(MobileBy.AccessibilityId("Card"))
                .FirstOrDefault(element => element.Displayed);
            if (card is not null)
            {
                return card;
            }

            return null;
        });
    }

    private void PayThroughWebView(PaymentCardDetails payment, Action? beforePaymentSubmission)
    {
        IContextAware context = (IContextAware)_driver;
        WebDriverWait contextWait = new(_driver, TimeSpan.FromSeconds(45));
        string? webView = contextWait.Until(_ =>
            context.Contexts.FirstOrDefault(candidate =>
                !candidate.Equals("NATIVE_APP", StringComparison.OrdinalIgnoreCase)));
        if (webView is null)
        {
            throw new InvalidOperationException("The hosted payment WebView was not exposed to Appium.");
        }

        context.Context = webView;
        try
        {
            HppForm form = new(_driver);
            form.ClickOnCard();
            form.EnterNameOnCard(payment.NameOnCard);
            form.EnterCardNumber(payment.CardNumber);
            form.EnterExpiryDate(payment.ExpiryDate);
            form.EnterCvv(payment.Cvv);
            form.EnterAddressLine1(payment.AddressLine1);
            form.EnterTownCity(payment.TownCity);
            form.EnterCountry(payment.Country);
            form.EnterPostcode(payment.Postcode);
            beforePaymentSubmission?.Invoke();
            form.ClickPayAndConfirmBooking();
        }
        finally
        {
            context.Context = "NATIVE_APP";
        }
    }

    private void PayThroughNativeAccessibility(OpenQA.Selenium.IWebElement card, PaymentCardDetails payment, Action? beforePaymentSubmission)
    {
        card.Click();
        OpenQA.Selenium.IWebElement name = WaitForVisible(
            new ByIosClassChain("**/XCUIElementTypeTextField[`name == \"Name on card\"`]"));
        EnterText(name, payment.NameOnCard);
        EnterHppField("Card number", payment.CardNumber);
        EnterHppField("Expiry date", payment.ExpiryDate);
        EnterText(FindWithScrolling(
            new ByIosClassChain("**/XCUIElementTypeSecureTextField"), "HPP CVV"), payment.Cvv);
        DismissKeyboard();
        EnterHppField("Address line 1", payment.AddressLine1, verifyValue: true);
        EnterHppField("Town / City", payment.TownCity, verifyValue: true);
        EnterHppField("Country", payment.Country);
        SelectCountrySuggestion(payment.Country);

        EnterHppField("Postcode", payment.Postcode, verifyValue: true);
        DismissKeyboard();

        // Later field entry must not silently corrupt an earlier field before payment.
        WaitForRetainedValue(HppTextField("Address line 1"), payment.AddressLine1, "HPP Address line 1");
        WaitForRetainedValue(HppTextField("Town / City"), payment.TownCity, "HPP Town / City");
        WaitForRetainedValue(HppTextField("Postcode"), payment.Postcode, "HPP postcode");

        OpenQA.Selenium.IWebElement pay = FindWithScrolling(
            new ByIosClassChain("**/XCUIElementTypeButton[`name == \"Pay and confirm booking\"`]"),
            "Pay and confirm booking");
        WebDriverWait enabledWait = new(_driver, TimeSpan.FromSeconds(10));
        enabledWait.Until(_ => pay.Displayed && pay.Enabled);
        beforePaymentSubmission?.Invoke();
        pay.Click();
    }

    private void EnterHppField(string name, string value, bool verifyValue = false)
    {
        OpenQA.Selenium.By locator = HppTextField(name);
        OpenQA.Selenium.IWebElement field = FindWithScrolling(
            locator,
            $"HPP {name}");
        EnterText(field, value);
        if (verifyValue)
        {
            WaitForRetainedValue(locator, value, $"HPP {name}");
        }
    }

    private static OpenQA.Selenium.By HppTextField(string name)
        => new ByIosClassChain($"**/XCUIElementTypeTextField[`name == \"{name}\"`]");

    private void WaitForRetainedValue(
        OpenQA.Selenium.By locator,
        string value,
        string description)
    {
        WebDriverWait wait = new(_driver, TimeSpan.FromSeconds(5));
        wait.IgnoreExceptionTypes(typeof(OpenQA.Selenium.StaleElementReferenceException));
        try
        {
            wait.Until(_ =>
            {
                OpenQA.Selenium.IWebElement[] fields = _driver.FindElements(locator).ToArray();
                return fields.Length == 1 && HppValueMatches(
                    fields[0].GetAttribute("value"), value, () => fields[0].Text);
            });
        }
        catch (OpenQA.Selenium.WebDriverTimeoutException exception)
        {
            throw new InvalidOperationException(
                $"{description} did not retain the expected value after one entry attempt. " +
                "No retyping or payment submission was attempted; field values omitted.", exception);
        }
    }

    internal static bool HppValueMatches(string? observed, string expected, Func<string?>? readText = null)
        => (observed ?? readText?.Invoke()).Is(expected);

    private void DismissKeyboard()
    {
        OpenQA.Selenium.IWebElement? dismiss = FindVisible(
            MobileBy.AccessibilityId("selected"), TimeSpan.FromSeconds(2))
            ?? FindVisible(MobileBy.AccessibilityId("Done"), TimeSpan.FromSeconds(2));

        if (dismiss is null)
        {
            throw new OpenQA.Selenium.NoSuchElementException(
                "The HPP keyboard dismiss control ('selected' or 'Done') was not visible.");
        }

        dismiss.Click();
    }

    private void SelectCountrySuggestion(string country)
    {
        OpenQA.Selenium.By suggestionLocator = new ByIosNSPredicate(
            $"type != 'XCUIElementTypeTextField' AND " +
            $"(name == {ToPredicateLiteral(country)} OR label == {ToPredicateLiteral(country)} " +
            $"OR value == {ToPredicateLiteral(country)})");

        // The HPP autocomplete can expose its suggestion either above the keyboard or
        // only after the keyboard is dismissed, depending on the iOS/WebKit version.
        OpenQA.Selenium.IWebElement? suggestion = FindVisible(
            suggestionLocator, TimeSpan.FromSeconds(5));
        if (suggestion is null)
        {
            // BrowserStack's iOS native tree does not consistently expose WebKit
            // autocomplete options. Return accepts the highlighted HPP suggestion.
            _driver.SwitchTo().ActiveElement().SendKeys(OpenQA.Selenium.Keys.Enter);
            return;
        }

        suggestion.Click();
    }

    private static void EnterText(OpenQA.Selenium.IWebElement field, string value)
    {
        field.Click();
        field.Clear();
        field.SendKeys(value);
    }

    private OpenQA.Selenium.IWebElement FindWithScrolling(OpenQA.Selenium.By locator, string description)
    {
        // WebKit may report a keyboard-covered input as Displayed. Remove that obstruction
        // before resolving the target, rather than sending keys to the previously focused input.
        if (KeyboardIsVisible())
        {
            DismissKeyboard();
            new WebDriverWait(_driver, TimeSpan.FromSeconds(5)).Until(_ => !KeyboardIsVisible());
        }
        for (int attempt = 0; attempt <= 6; attempt++)
        {
            OpenQA.Selenium.IWebElement? element = FindVisible(locator, TimeSpan.FromMilliseconds(500));
            if (element is not null && element.Enabled &&
                HppBoundsAreSafe(new System.Drawing.Rectangle(element.Location, element.Size),
                    _driver.Manage().Window.Size))
            {
                return element;
            }

            if (attempt < 6)
            {
                _driver.ExecuteScript("mobile: scroll", new Dictionary<string, object>
                {
                    ["direction"] = "down",
                });
            }
        }

        throw new OpenQA.Selenium.NoSuchElementException(
            $"{description} was not visible after scrolling the HPP form.");
    }

    private bool KeyboardIsVisible()
        => _driver.FindElements(new ByIosClassChain("**/XCUIElementTypeKeyboard"))
            .Any(element => element.Displayed);

    internal static bool HppBoundsAreSafe(System.Drawing.Rectangle field, System.Drawing.Size window)
        => field.Width > 0 && field.Height > 0 && field.Left >= 0 && field.Top >= 0 &&
            field.Right <= window.Width && field.Bottom <= window.Height;

    private OpenQA.Selenium.IWebElement WaitForVisible(OpenQA.Selenium.By locator) =>
        FindVisible(locator, TimeSpan.FromSeconds(30))
        ?? throw new OpenQA.Selenium.NoSuchElementException($"HPP element '{locator}' was not visible.");

    private OpenQA.Selenium.IWebElement? FindVisible(OpenQA.Selenium.By locator, TimeSpan timeout)
    {
        WebDriverWait wait = new(_driver, timeout);
        try
        {
            return wait.Until(_ =>
                _driver.FindElements(locator).FirstOrDefault(element => element.Displayed));
        }
        catch (OpenQA.Selenium.WebDriverTimeoutException)
        {
            return null;
        }
    }

    private static string ToPredicateLiteral(string value) =>
        $"'{value.Replace("'", "\\'")}'";
}

public sealed record PaymentCardDetails(
    string NameOnCard,
    string CardNumber,
    string ExpiryDate,
    string Cvv,
    string AddressLine1,
    string TownCity,
    string Country,
    string Postcode);
