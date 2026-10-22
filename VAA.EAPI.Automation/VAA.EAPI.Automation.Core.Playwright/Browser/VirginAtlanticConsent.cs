using Microsoft.Playwright;

namespace VAA.EAPI.Automation.Core.Playwright.Browser;

public static class VirginAtlanticConsent
{
    private const string VirginAtlanticParentDomain = ".virginatlantic.com";

    private static readonly IReadOnlyDictionary<string, string> EnsightenConsentValues =
        new Dictionary<string, string>
        {
            ["VAA_ENSIGHTEN_PRIVACY_AEPFLAG"] = "Updated",
            ["VAA_ENSIGHTEN_PRIVACY_Analytics"] = "1",
            ["VAA_ENSIGHTEN_PRIVACY_BANNER_VIEWED"] = "1",
            ["VAA_ENSIGHTEN_PRIVACY_Functional"] = "1",
            ["VAA_ENSIGHTEN_PRIVACY_Marketing"] = "1"
        };

    public static async Task ApplyEnsightenConsentAsync(IBrowserContext context, Uri baseUrl)
    {
        List<Cookie> cookies = new List<Cookie>();
        bool isVirginAtlanticHost = baseUrl.Host.Equals(
                VirginAtlanticParentDomain.TrimStart('.'),
                StringComparison.OrdinalIgnoreCase) ||
            baseUrl.Host.EndsWith(VirginAtlanticParentDomain, StringComparison.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, string> consentValue in EnsightenConsentValues)
        {
            Cookie cookie = new()
            {
                Name = consentValue.Key,
                Value = consentValue.Value
            };

            if (isVirginAtlanticHost)
            {
                cookie.Domain = VirginAtlanticParentDomain;
                cookie.Path = "/";
            }
            else
            {
                cookie.Url = baseUrl.GetLeftPart(UriPartial.Authority);
            }

            cookies.Add(cookie);
        }

        await context.AddCookiesAsync(cookies);
        await context.AddInitScriptAsync(BuildConsentStorageScript());
    }

    public static async Task PrimeApplicationSessionAsync(IPage page, Uri baseUrl, int navigationTimeoutMs)
    {
        await page.GotoAsync(baseUrl.ToString(), new PageGotoOptions
        {
            WaitUntil = WaitUntilState.Commit,
            Timeout = Math.Min(navigationTimeoutMs, 30_000)
        });

        try
        {
            await page.WaitForLoadStateAsync(
                LoadState.DOMContentLoaded,
                new PageWaitForLoadStateOptions { Timeout = 15_000 });
        }
        catch (TimeoutException)
        {
            // A committed response is sufficient to establish STG edge/session cookies.
        }

        await DismissPrivacyBannerIfPresentAsync(page);
    }

    public static async Task<bool> DismissPrivacyBannerIfPresentAsync(IPage page)
    {
        const string consentActionSelector = "[data-eapi-playwright-consent-action]";
        bool found = await page.EvaluateAsync<bool>(
            @"() => {
                document.querySelectorAll('[data-eapi-playwright-consent-action]')
                    .forEach((element) => element.removeAttribute('data-eapi-playwright-consent-action'));

                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 &&
                        rect.height > 0 &&
                        style.visibility !== 'hidden' &&
                        style.display !== 'none';
                };

                const exactConsentAction = /^(yes,\s*)?(i\s+)?(agree|accept|allow)(\s+all)?(\s+cookies)?$/i;
                const buttons = [...document.querySelectorAll(
                    ""button, a, [role='button'], input[type='button'], input[type='submit']""
                )].filter(isVisible);

                const agreeButton = buttons.find((button) => {
                    const text = ((button.value || button.textContent || '') + '').trim();
                    if (exactConsentAction.test(text)) {
                        return true;
                    }

                    const dialog = button.closest(
                        ""[role='dialog'], [aria-modal='true'], [class*='privacy' i], [id*='privacy' i], [class*='cookie' i], [id*='cookie' i], [class*='consent' i], [id*='consent' i]""
                    );
                    const dialogText = ((dialog && dialog.textContent) || '').trim();
                    return /privacy|cookie|consent|terms|conditions|t&c|tnc/i.test(dialogText) &&
                        /\bagree\b|\baccept\b|\ballow\b|\bsave\b|\bconfirm\b|\bcontinue\b|\bdone\b/i.test(text);
                });

                if (agreeButton) {
                    agreeButton.setAttribute('data-eapi-playwright-consent-action', 'true');
                    return true;
                }

                return false;
            }");

        if (!found)
        {
            return false;
        }

        ILocator action = page.Locator(consentActionSelector).First;
        await action.ClickAsync(new LocatorClickOptions { Timeout = 10_000 });
        await page.WaitForFunctionAsync(
            @"() => {
                const action = document.querySelector('[data-eapi-playwright-consent-action]');
                if (!action) return true;
                const rect = action.getBoundingClientRect();
                const style = window.getComputedStyle(action);
                return rect.width <= 0 || rect.height <= 0 ||
                    style.visibility === 'hidden' || style.display === 'none';
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = 10_000 });

        return true;
    }

    private static string BuildConsentStorageScript()
    {
        string entries = string.Join(
            Environment.NewLine,
            EnsightenConsentValues.Select(consentValue =>
                $"window.localStorage.setItem('{consentValue.Key}', '{consentValue.Value}');"));

        return $@"
            (() => {{
                try {{
                    if (window.__eapiPlaywrightConsentStoragePrimed__) {{
                        return;
                    }}

                    window.__eapiPlaywrightConsentStoragePrimed__ = true;
                    {entries}
                    window.localStorage.setItem(
                        `VAA_ENSIGHTEN_PRIVACY_TLD-${{window.location.hostname}}`,
                        '1'
                    );
                }} catch {{
                    // Consent storage priming is best-effort only.
                }}
            }})();";
    }
}

