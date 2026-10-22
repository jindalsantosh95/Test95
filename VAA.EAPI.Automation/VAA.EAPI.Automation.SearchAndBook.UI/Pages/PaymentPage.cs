using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Pages;

public sealed class PaymentPage : BasePage
{
    private readonly IFrameLocator _paymentFrame;

    public PaymentPage(IPage page)
        : base(page)
    {
        _paymentFrame = page.FrameLocator("iframe[name='paymentFrame']");
    }

    public async Task OpenCardPaymentAsync(int timeoutMs)
    {
        await WaitUntilLoadedAsync(timeoutMs);

        ILocator cardNumber = _paymentFrame.Locator(
            "input[formcontrolname='paymentCardNum'], input[name='paymentCardNum']");
        if (await IsCardNumberReadyAsync(cardNumber))
        {
            return;
        }

        // The hosted payment page has one payment-method accordion. Target its actual
        // header container rather than composing nested text selectors inside the iframe.
        ILocator cardHeader = _paymentFrame.Locator("div.accordion-header-container").First;

        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        DateTime recoveryAt = DateTime.UtcNow.AddMilliseconds(timeoutMs / 2d);
        bool reloadedStalledHpp = false;
        Exception? lastClickFailure = null;
        while (DateTime.UtcNow < deadline)
        {
            if (await IsCardNumberReadyAsync(cardNumber))
            {
                return;
            }

            try
            {
                await cardHeader.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 5_000
                });
                await cardHeader.ScrollIntoViewIfNeededAsync(new LocatorScrollIntoViewIfNeededOptions
                {
                    Timeout = 5_000
                });
                // The HPP can be covered by its host page while still accepting its own click event.
                await cardHeader.ClickAsync(new LocatorClickOptions
                {
                    Force = true,
                    Timeout = 5_000
                });
            }
            catch (Exception exception) when (exception is PlaywrightException or TimeoutException)
            {
                lastClickFailure = exception;
            }

            try
            {
                await cardNumber.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 3_000
                });
                if (await IsCardNumberReadyAsync(cardNumber))
                {
                    return;
                }
                await Task.Delay(300);
            }
            catch (TimeoutException)
            {
                if (!reloadedStalledHpp && DateTime.UtcNow >= recoveryAt)
                {
                    string bodyText = await Page.Locator("body").InnerTextAsync();
                    if (bodyText.Contains("Loading eligible forms of payment", StringComparison.OrdinalIgnoreCase))
                    {
                        reloadedStalledHpp = true;
                        await Page.ReloadAsync(new PageReloadOptions
                        {
                            WaitUntil = WaitUntilState.DOMContentLoaded,
                            Timeout = Math.Min(timeoutMs, 45_000)
                        });
                        await WaitUntilLoadedAsync(Math.Min(timeoutMs, 60_000));
                    }
                }
                await Task.Delay(300);
            }
        }

        string summary = await GetPageSummaryAsync();
        throw new InvalidOperationException(
            "Card payment accordion did not expose an enabled, editable card-number field inside paymentFrame. " +
            $"HPP recovery reload: {reloadedStalledHpp}. " +
            $"Last click failure: {lastClickFailure?.Message ?? "none"}. {summary}");
    }

    private static async Task<bool> IsCardNumberReadyAsync(ILocator cardNumber)
    {
        return await cardNumber.IsVisibleAsync() &&
            await cardNumber.IsEnabledAsync() &&
            await cardNumber.IsEditableAsync();
    }

    public async Task AssertPayPalVisibleAsync(int timeoutMs)
    {
        await WaitUntilLoadedAsync(timeoutMs);

        ILocator payPal = _paymentFrame.GetByText(
            "PayPal",
            new FrameLocatorGetByTextOptions { Exact = true }).First;
        try
        {
            await payPal.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
        }
        catch (TimeoutException exception)
        {
            throw new InvalidOperationException(
                "PayPal was not displayed on HPP for a booking without an added seat. " +
                await GetPageSummaryAsync(),
                exception);
        }
    }

    public async Task WaitUntilLoadedAsync(int timeoutMs)
    {
        await Page.WaitForFunctionAsync(
            @"() => {
                const bodyText = document.body?.innerText || '';
                const frame = document.querySelector(""iframe[name='paymentFrame']"");
                const isVisible = (element) => {
                    if (!element) return false;
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };

                const applicationError = [...document.querySelectorAll('[role=""dialog""], [aria-modal=""true""]')]
                    .filter(isVisible)
                    .some(element => /Uh oh|something.?s not right|unable to book|encountered an error|payment.*failed/i
                        .test(element.innerText || ''));
                return applicationError ||
                    (/How would you like to pay|Payment options/i.test(bodyText) && isVisible(frame));
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });

        string applicationError = await GetVisibleApplicationErrorAsync();
        if (!applicationError.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(
                $"Payment page displayed an application error before the hosted payment form became ready: " +
                $"{applicationError}. {await GetPageSummaryAsync()}");
        }
    }

    public async Task<string> ApplyPayWithPointsAsync(int timeoutMs)
    {
        await WaitUntilLoadedAsync(timeoutMs);

        ILocator summary = Page.Locator("summary").Filter(new LocatorFilterOptions
        {
            HasTextRegex = new System.Text.RegularExpressions.Regex(
                "Pay with points",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        }).First;
        await summary.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        if (!(await summary.GetAttributeAsync("aria-expanded")).Is("true"))
        {
            await summary.ClickAsync(new LocatorClickOptions { Timeout = 15_000 });
        }

        ILocator pointsSelect = Page.Locator("select:visible").First;
        await pointsSelect.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15_000
        });

        IReadOnlyList<string> values = await pointsSelect.Locator("option").EvaluateAllAsync<string[]>(
            "options => options.map(option => option.value).filter(value => value && value.trim())");
        if (values.Count == 0)
        {
            throw new InvalidOperationException(
                "Pay with Points did not provide a selectable points amount.");
        }

        await pointsSelect.SelectOptionAsync(values[0]);
        string selectedLabel = await pointsSelect.Locator("option:checked").InnerTextAsync();

        ILocator applyButton = Page.Locator("button.pay-with-points__button").First;
        await applyButton.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15_000
        });
        await applyButton.ClickAsync(new LocatorClickOptions { Timeout = 15_000 });

        try
        {
            await Page.WaitForLoadStateAsync(
                LoadState.NetworkIdle,
                new PageWaitForLoadStateOptions { Timeout = 10_000 });
        }
        catch (TimeoutException)
        {
            // Long-lived analytics calls can prevent network-idle after the basket update.
        }

        await Page.WaitForFunctionAsync(
            @"([expectedValue]) => {
                const select = [...document.querySelectorAll('select')]
                    .find(element => {
                        const rect = element.getBoundingClientRect();
                        return rect.width > 0 && rect.height > 0;
                    });
                const bodyText = (document.body && document.body.innerText || '').replace(/\s+/g, ' ');
                return select?.value === expectedValue || /points?\)? applied to your booking/i.test(bodyText);
            }",
            new[] { values[0] },
            new PageWaitForFunctionOptions { Timeout = timeoutMs });

        string? retainedValue = await pointsSelect.IsVisibleAsync()
            ? await pointsSelect.InputValueAsync()
            : null;
        if (retainedValue is not null &&
            retainedValue != values[0])
        {
            throw new InvalidOperationException(
                $"Pay with Points selection was not retained. Expected '{values[0]}', actual '{retainedValue}'.");
        }

        return $"Applied Pay with Points option '{selectedLabel.Trim()}' (value '{retainedValue ?? values[0]}').";
    }

    public async Task<string> FillNonThreeDsCardDetailsAsync(int timeoutMs)
    {
        await OpenCardPaymentAsync(timeoutMs);

        await FillAsync("input[formcontrolname='name']", "Automation Test");
        await FillAsync("input[formcontrolname='paymentCardNum']", "4000000000002719");
        await FillAsync("input[formcontrolname='expiryDate']", "12/30");
        await FillAsync("input[formcontrolname='cardVerificationValueNum']", "123");

        ILocator cardNumber = _paymentFrame.Locator("input[formcontrolname='paymentCardNum']");
        string value = await cardNumber.InputValueAsync();
        string digits = new string(value.Where(char.IsDigit).ToArray());
        if (!digits.EndsWith("2719", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Card number was not retained by the hosted payment form after entry.");
        }

        return $"HPP card form accepted non-3DS test data; card ending {digits[^4..]}.";
    }

    public async Task<string> OpenCardholderOfferApplicationAsync(int timeoutMs)
    {
        await WaitUntilLoadedAsync(timeoutMs);

        ILocator summary = Page.Locator("summary").Filter(new LocatorFilterOptions
        {
            HasTextRegex = new System.Text.RegularExpressions.Regex(
                "New Cardholder Offer.*Virgin Red Rewards Mastercard",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        }).First;
        await summary.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        string offerText = NormalizeWhitespace(await summary.InnerTextAsync());
        if (!System.Text.RegularExpressions.Regex.IsMatch(
                offerText,
                @"60K\s+Bonus\s+Points.*\$100\s+Statement\s+Credit",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            throw new InvalidOperationException(
                $"Cardholder offer text was not as expected. Actual text: '{offerText}'.");
        }

        string bannerText = await GetCardholderOfferErrorBannerAsync();
        if (!bannerText.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(
                $"Cardholder offer application is unavailable on the payment page: '{bannerText}'.");
        }

        if (!(await summary.GetAttributeAsync("aria-expanded")).Is("true"))
        {
            await summary.ClickAsync(new LocatorClickOptions { Timeout = 15_000 });
        }

        ILocator applyButton = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
        {
            Name = "Apply now",
            Exact = true
        });
        await applyButton.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15_000
        });

        int pageCountBefore = Page.Context.Pages.Count;
        await Page.EvaluateAsync(
            @"() => {
                if (window.__sabCardholderWindowOpenWrapped) return;
                window.__sabCardholderWindowOpenWrapped = true;
                window.__sabCardholderOpenedUrl = '';
                const originalOpen = window.open.bind(window);
                window.open = (url, target, features) => {
                    window.__sabCardholderOpenedUrl = String(url || '');
                    return originalOpen(url, target, features);
                };
            }");
        await applyButton.EvaluateAsync("button => button.click()");

        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        string observedUrl = string.Empty;
        while (DateTime.UtcNow < deadline)
        {
            IPage? synchronyPage = Page.Context.Pages.FirstOrDefault(
                page => page.Url.Contains("uapply.syf.com", StringComparison.OrdinalIgnoreCase));
            if (synchronyPage is not null)
            {
                observedUrl = synchronyPage.Url;
                break;
            }

            if (Page.Url.Contains("uapply.syf.com", StringComparison.OrdinalIgnoreCase))
            {
                observedUrl = Page.Url;
                break;
            }

            observedUrl = await Page.EvaluateAsync<string>(
                "() => String(window.__sabCardholderOpenedUrl || '')");
            if (observedUrl.Contains("uapply.syf.com", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            bannerText = await GetCardholderOfferErrorBannerAsync();
            if (!bannerText.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException(
                    $"Cardholder offer application is unavailable after clicking Apply now: '{bannerText}'.");
            }

            await Task.Delay(250);
        }

        if (!observedUrl.Contains("uapply.syf.com", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Clicking the cardholder offer Apply now button did not open or target the Synchrony application. " +
                $"Pages before: {pageCountBefore}; pages after: {Page.Context.Pages.Count}; " +
                $"window.open URL: '{observedUrl}'.");
        }

        return $"Cardholder offer targeted the Synchrony application: {observedUrl}";
    }

    public async Task<string?> SubmitNonThreeDsCardPaymentAsync(int timeoutMs)
    {
        await FillNonThreeDsCardDetailsAsync(timeoutMs);
        await FillAsync("input[formcontrolname='addressLine1Text']", "1 Manchester Square");
        await FillAsync("input[formcontrolname='cityLocalityName']", "London");
        await SelectCountryAsync("United Kingdom");
        await FillAsync("input[formcontrolname='postalCode']", "W1U 3PE");

        await AssertInputValueAsync("input[formcontrolname='addressLine1Text']", "1 Manchester Square");
        await AssertInputValueAsync("input[formcontrolname='cityLocalityName']", "London");
        await AssertInputValueAsync("input[formcontrolname='countryCode']", "United Kingdom");
        await AssertInputValueAsync("input[formcontrolname='postalCode']", "W1U 3PE");

        ILocator payButton = _paymentFrame.Locator(
            "button[data-id='payment-submit-btn'], button[type='submit']").First;
        await payButton.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 20_000
        });

        bool enabled = await payButton.EvaluateAsync<bool>(
            @"(button) => !button.disabled && button.getAttribute('aria-disabled') !== 'true' &&
                window.getComputedStyle(button).pointerEvents !== 'none'");
        if (!enabled)
        {
            throw new InvalidOperationException("Pay and confirm booking is disabled after valid card and billing details were entered.");
        }

        // The HPP iframe is taller than the viewport and the host page's sticky footer can
        // intercept a geometry-based click. This remains guarded by the enabled check above.
        IRequest capturedRedirect = await Page.RunAndWaitForRequestAsync(
            async () => await payButton.EvaluateAsync("button => button.click()"),
            request => Uri.TryCreate(request.Url, UriKind.Absolute, out Uri? redirectUri) &&
                redirectUri.AbsolutePath.Contains("/payment/captured", StringComparison.OrdinalIgnoreCase),
            new PageRunAndWaitForRequestOptions { Timeout = timeoutMs });

        return GetPaymentReferenceId(capturedRedirect.Url);
    }

    public async Task<string> GetPageSummaryAsync()
    {
        return await Page.EvaluateAsync<string>(
            @"() => {
                const frameCount = document.querySelectorAll(""iframe[name='paymentFrame']"").length;
                const paypalCount = document.querySelectorAll('[data-testid*=""paypal"" i], [aria-label*=""paypal"" i]').length;
                const cardActionCount = document.querySelectorAll('[data-testid*=""card"" i], button[id*=""card"" i]').length;
                return `URL: ${location.origin}${location.pathname}; paymentFrames: ${frameCount}; ` +
                    `paypalTargets: ${paypalCount}; cardActionTargets: ${cardActionCount}`;
            }");
    }

    private async Task<string> GetVisibleApplicationErrorAsync()
    {
        return await Page.EvaluateAsync<string>(
            @"() => {
                const isVisible = element => {
                    const rect = element.getBoundingClientRect();
                    const style = getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                const error = [...document.querySelectorAll('[role=""dialog""], [aria-modal=""true""]')]
                    .filter(isVisible)
                    .find(element => /Uh oh|something.?s not right|unable to book|encountered an error|payment.*failed/i
                        .test(element.innerText || ''));
                return (error?.innerText || '').replace(/\s+/g, ' ').trim().slice(0, 500);
            }");
    }

    private static string? GetPaymentReferenceId(string redirectUrl)
    {
        if (!Uri.TryCreate(redirectUrl, UriKind.Absolute, out Uri? redirectUri))
        {
            return null;
        }

        foreach (string queryPart in redirectUri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] keyValue = queryPart.Split('=', 2);
            if (keyValue.Length == 2 &&
                keyValue[0].Is("paymentReferenceId") &&
                !keyValue[1].IsNullOrWhiteSpace())
            {
                return Uri.UnescapeDataString(keyValue[1].Replace('+', ' '));
            }
        }

        return null;
    }

    private static async Task<bool> IsVisibleAsync(ILocator locator)
    {
        try
        {
            return await locator.IsVisibleAsync();
        }
        catch (PlaywrightException)
        {
            return false;
        }
    }

    private async Task FillAsync(string selector, string value)
    {
        ILocator field = _paymentFrame.Locator(selector);
        await field.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15_000
        });
        await field.FillAsync(value);
    }

    private async Task SelectCountryAsync(string country)
    {
        ILocator countryInput = _paymentFrame.Locator("input[formcontrolname='countryCode']");
        await countryInput.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15_000
        });
        await countryInput.FillAsync(country);

        ILocator option = _paymentFrame.Locator($"mat-option:has-text('{country}')").First;
        await option.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Attached,
            Timeout = 15_000
        });

        // Angular Material can position the iframe's autocomplete overlay beyond the
        // host viewport even though the option is active. Dispatch inside the HPP DOM.
        await option.EvaluateAsync("element => element.click()");
        await AssertInputValueAsync("input[formcontrolname='countryCode']", country);
    }

    private async Task AssertInputValueAsync(string selector, string expectedValue)
    {
        ILocator field = _paymentFrame.Locator(selector);
        string actualValue = await field.InputValueAsync();
        if (!actualValue.Contains(expectedValue, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Hosted payment field '{selector}' did not retain '{expectedValue}'. Actual value: '{actualValue}'.");
        }
    }

    private async Task<string> GetCardholderOfferErrorBannerAsync()
    {
        string text = await Page.EvaluateAsync<string>(
            @"() => {
                const outage =
                    /trouble with the credit card application right now|choose a different payment method/i;
                const visible = element => {
                    const style = getComputedStyle(element);
                    const rect = element.getBoundingClientRect();
                    return rect.width > 0 && rect.height > 0 && style.display !== 'none' &&
                        style.visibility !== 'hidden' && Number(style.opacity || 1) !== 0;
                };
                const selectors = '[role=""alert""], .errorMessageConatiner, .errorMessageContainer, ' +
                    '[class*=""error""], [class*=""alert""], [class*=""banner""]';
                for (const element of document.querySelectorAll(selectors)) {
                    const text = (element.innerText || '').replace(/\s+/g, ' ').trim();
                    if (visible(element) && outage.test(text)) return text.slice(0, 300);
                }
                return '';
            }");
        return NormalizeWhitespace(text);
    }

    private static string NormalizeWhitespace(string value)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            value ?? string.Empty,
            @"\s+",
            " ").Trim();
    }
}
