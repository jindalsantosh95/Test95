using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.Core.TestDataConfig.APMPayment;
using VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards;

namespace VAA.EAPI.Automation.Core.Playwright.HostedPayment;

public sealed class DirectHostedPaymentPage
{
    private const string PaymentReferenceIdQueryParameter = "paymentReferenceId";
    private readonly IPage _page;

    public DirectHostedPaymentPage(IPage page)
    {
        _page = page;
    }

    public static DirectHostedPaymentPage FromSession(PlaywrightBrowserSession session) => new(session.Page);

    public async Task ValidateLoadedAsync(bool expectPayPal, int timeoutMs)
    {
        ILocator card = _page.GetByText("Card", new PageGetByTextOptions { Exact = true }).First;
        await card.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        ILocator cardNumber = _page.Locator(
            "input[formcontrolname='paymentCardNum'], input[name='paymentCardNum']").First;
        if (!await cardNumber.IsVisibleAsync())
        {
            await card.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });
        }
        try
        {
            await cardNumber.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
            await Microsoft.Playwright.Assertions.Expect(cardNumber).ToBeEditableAsync(
                new LocatorAssertionsToBeEditableOptions { Timeout = timeoutMs });
        }
        catch (Exception exception) when (exception is TimeoutException or PlaywrightException)
        {
            throw new InvalidOperationException(
                "Hosted payment card-number field did not become enabled and editable before timeout.",
                exception);
        }

        ILocator payPal = _page.GetByText("PayPal", new PageGetByTextOptions { Exact = true }).First;
        if (expectPayPal)
        {
            await payPal.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
            return;
        }

        try
        {
            await payPal.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                // Card editability above establishes that HPP method loading has completed.
                // Allow a short grace period for a late PayPal render without delaying every
                // positive-price health check by the full validation timeout.
                Timeout = Math.Min(timeoutMs, 5_000)
            });
        }
        catch (TimeoutException)
        {
            return;
        }

        throw new InvalidOperationException(
            "PayPal was displayed when the booking is not eligible for that payment method.");
    }

    public async Task SubmitCardDetailsAsync(CardDetails cardDetails, int timeoutMs)
    {
        ILocator cardNumber = _page.Locator("input[formcontrolname='paymentCardNum'], input[name='paymentCardNum']").First;
        if (!await cardNumber.IsVisibleAsync())
        {
            ILocator cardHeader = _page.GetByText("Card", new PageGetByTextOptions { Exact = true }).First;
            await cardHeader.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
            await cardHeader.ClickAsync(new LocatorClickOptions { Force = true, Timeout = timeoutMs });
        }

        await cardNumber.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
        await FillAsync("input[formcontrolname='name']", cardDetails.NameOnCard);
        await cardNumber.FillAsync(cardDetails.CardNumber);
        await FillAsync("input[formcontrolname='expiryDate']", cardDetails.ExpiryDate);
        await FillAsync("input[formcontrolname='cardVerificationValueNum']", cardDetails.Cvv);
        await FillAsync("input[formcontrolname='addressLine1Text']", cardDetails.AddressLineOne);
        await FillAsync("input[formcontrolname='addressLine2Text']", cardDetails.AddressLineTwo);
        await FillAsync("input[formcontrolname='cityLocalityName']", cardDetails.TownCity);

        ILocator country = _page.Locator("[formcontrolname='countryCode']").First;
        if (await country.EvaluateAsync<string>("element => element.tagName") == "SELECT")
        {
            await country.SelectOptionAsync(new SelectOptionValue { Label = cardDetails.Country });
        }
        else
        {
            await country.FillAsync(cardDetails.Country);
            await country.PressAsync("Tab");
        }

        if (!cardDetails.State.IsNullOrWhiteSpace())
        {
            ILocator stateProvince = _page.Locator("input[formcontrolname='state']").First;
            await stateProvince.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
            await stateProvince.FillAsync(cardDetails.State);
            await stateProvince.PressAsync("Tab");
        }

        await FillAsync("input[formcontrolname='postalCode']", cardDetails.Postcode);

        ILocator payButton = _page.Locator("button[data-id='payment-submit-btn'], button[type='submit']").First;
        await payButton.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
        if (cardDetails.IsThreeDs)
        {
            await payButton.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });
            IFrameLocator challengeFrame = _page
                .FrameLocator("iframe[title='third-party-bank-content']")
                .FrameLocator("iframe[id*='cardinal-stepUpIframe']");
            ILocator otpInput = challengeFrame.Locator("input[name='challengeDataEntry']");
            await otpInput.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
            await otpInput.FillAsync("1234");

            ILocator otpSubmit = challengeFrame.Locator("input[value='SUBMIT']");
            await WaitForCapturedRedirectAsync(
                _page,
                async () => await otpSubmit.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs }),
                timeoutMs);
            return;
        }

        await WaitForCapturedRedirectAsync(
            _page,
            async () => await payButton.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs }),
            timeoutMs);
    }

    public async Task SubmitExistingPaymentAsync(bool expandCard, int timeoutMs)
    {
        if (expandCard)
        {
            ILocator cardHeader = _page.GetByText("Card", new PageGetByTextOptions { Exact = true }).First;
            await cardHeader.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
            await cardHeader.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });
        }

        ILocator payButton = _page.Locator("button[data-id='payment-submit-btn'], button[type='submit']").First;
        await payButton.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
        await WaitForCapturedRedirectAsync(
            _page,
            async () => await payButton.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs }),
            timeoutMs);
    }

    public async Task<string> SubmitPayPalAsync(APMDetails apmDetails, int timeoutMs)
    {
        ILocator payPalHeader = _page.GetByText("PayPal", new PageGetByTextOptions { Exact = true }).First;
        await payPalHeader.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
        await payPalHeader.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });

        ILocator payPalButton = _page.Locator("div[id*='paypal-buttons']").First;
        await payPalButton.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        IPage payPalPage = await _page.RunAndWaitForPopupAsync(
            async () => await payPalButton.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs }),
            new PageRunAndWaitForPopupOptions { Timeout = timeoutMs });

        await payPalPage.Locator("input#email").FillAsync(apmDetails.Email);
        await payPalPage.Locator("button[value='Next']").ClickAsync();
        await payPalPage.Locator("input#password").FillAsync(apmDetails.Password);
        await payPalPage.Locator("button[value='Login']").ClickAsync();

        ILocator completePurchase = payPalPage.Locator("button[data-id='payment-submit-btn']").First;
        await completePurchase.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        IRequest capturedRequest = await WaitForCapturedRedirectAsync(
            payPalPage,
            async () => await completePurchase.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs }),
            timeoutMs);

        return ExtractPaymentReferenceId(capturedRequest.Url);
    }

    private static string ExtractPaymentReferenceId(string redirectUrl)
    {
        if (!Uri.TryCreate(redirectUrl, UriKind.Absolute, out Uri? redirectUri))
        {
            throw new InvalidOperationException("PayPal completed without a valid captured redirect URL.");
        }

        foreach (string queryPart in redirectUri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] keyValue = queryPart.Split('=', 2);
            if (keyValue.Length == 2
                && keyValue[0].Is(PaymentReferenceIdQueryParameter)
                && !keyValue[1].IsNullOrWhiteSpace())
            {
                return Uri.UnescapeDataString(keyValue[1].Replace('+', ' '));
            }
        }

        throw new InvalidOperationException("PayPal captured redirect did not contain a child payment reference.");
    }

    private static async Task<IRequest> WaitForCapturedRedirectAsync(
        IPage page,
        Func<Task> action,
        int timeoutMs)
    {
        return await page.RunAndWaitForRequestAsync(
            async () =>
            {
                try
                {
                    await action();
                }
                catch (PlaywrightException exception) when (
                    exception.Message.Contains("ERR_NAME_NOT_RESOLVED", StringComparison.Ordinal)
                    || exception.Message.Contains("Target page, context or browser has been closed", StringComparison.Ordinal))
                {
                    // SIT uses a non-resolving marker URL. Observing the captured request is the
                    // payment-completion contract also used by the Selenium implementation.
                }
            },
            request => Uri.TryCreate(request.Url, UriKind.Absolute, out Uri? redirectUri)
                && redirectUri.AbsolutePath.Contains("/payment/captured", StringComparison.OrdinalIgnoreCase),
            new PageRunAndWaitForRequestOptions { Timeout = timeoutMs });
    }

    private async Task FillAsync(string selector, string value)
    {
        if (value.IsNullOrWhiteSpace())
        {
            return;
        }

        await _page.Locator(selector).First.FillAsync(value);
    }
}
