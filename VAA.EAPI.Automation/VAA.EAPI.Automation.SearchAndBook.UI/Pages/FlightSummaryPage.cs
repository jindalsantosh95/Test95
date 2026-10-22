using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Pages;

public sealed class FlightSummaryPage : BasePage
{
    private const string ContinueMarkerSelector = "[data-eapi-playwright-target=\"summary-continue\"]";
    public FlightSummaryPage(IPage page)
        : base(page)
    {
    }

    public async Task WaitUntilReadyAsync(int timeoutMs)
    {
        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        DateTime recoveryAt = DateTime.UtcNow.AddMilliseconds(timeoutMs / 2d);
        bool reloadedIncompleteSummary = false;
        do
        {
            SummaryReadinessState state = await ReadReadinessStateAsync();
            if (state.Ready)
            {
                return;
            }

            if (!reloadedIncompleteSummary && DateTime.UtcNow >= recoveryAt && state.IncompleteSummaryShell)
            {
                reloadedIncompleteSummary = true;
                await Page.ReloadAsync(new PageReloadOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = Math.Min(timeoutMs / 2, 45_000)
                });
                await VirginAtlanticConsent.DismissPrivacyBannerIfPresentAsync(Page);
            }

            await Task.Delay(750);
        }
        while (DateTime.UtcNow < deadline);

        string pageText = await Page.Locator("body").InnerTextAsync(new LocatorInnerTextOptions { Timeout = 5_000 });
        string normalisedPageText = System.Text.RegularExpressions.Regex.Replace(pageText, @"\s+", " ");
        throw new TimeoutException(
            $"Flight summary was not ready within {timeoutMs}ms (recoveryReload={reloadedIncompleteSummary}). " +
            $"URL: {Page.Url}; page: {normalisedPageText[..Math.Min(normalisedPageText.Length, 1200)]}");
    }

    private async Task<SummaryReadinessState> ReadReadinessStateAsync()
    {
        return await Page.EvaluateAsync<SummaryReadinessState>(
            @"() => {
                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 &&
                        rect.height > 0 &&
                        style.display !== 'none' &&
                        style.visibility !== 'hidden';
                };

                const loadersVisible = [...document.querySelectorAll('[data-testid=""search-loader""], [data-testid=""tailfin-loader-component""], [data-testid=""interstitial-component""]')].some(isVisible);
                const isSummaryUrl = window.location.href.includes('/flights/search/summary');
                const summaryRendered = [...document.querySelectorAll('h1, h2, h3')]
                    .some((element) => isVisible(element) &&
                        /^Flight summary$/i.test((element.innerText || '').trim()));
                const hasContinue = [...document.querySelectorAll('button, a')]
                    .some((element) => isVisible(element) &&
                        /Continue to Passenger Details/i.test((element.innerText || '').trim()));
                const hasAlert = [...document.querySelectorAll('[data-testid=""alert-component""]')].some(isVisible);
                const body = (document.body?.innerText || '').replace(/\s+/g, ' ').trim();
                const incompleteSummaryShell = (isSummaryUrl || summaryRendered) && loadersVisible && hasContinue &&
                    /Creating your flight summary|just a moment/i.test(body);

                return {
                    Ready: (isSummaryUrl || summaryRendered) && !loadersVisible &&
                        (hasAlert || hasContinue),
                    IncompleteSummaryShell: incompleteSummaryShell
                };
            }");
    }

    public async Task AssertJourneySlicesAsync(string origin, string destination, bool isReturn)
    {
        string body = System.Text.RegularExpressions.Regex.Replace(
            await Page.Locator("body").InnerTextAsync(),
            @"\s+",
            " ").Trim();
        if (System.Text.RegularExpressions.Regex.IsMatch(
                body,
                @"We're sorry, there was a problem processing your request|Please go back and try the entry again|Something went wrong",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            throw new InvalidOperationException(
                $"STG returned an application error instead of a flight summary for {origin} to {destination}. " +
                $"URL: {Page.Url}; page: {body[..Math.Min(body.Length, 1200)]}");
        }

        string outboundPattern = $@"\b{System.Text.RegularExpressions.Regex.Escape(origin)}\b.*?\b{System.Text.RegularExpressions.Regex.Escape(destination)}\b";
        if (!System.Text.RegularExpressions.Regex.IsMatch(
                body,
                outboundPattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            throw new InvalidOperationException(
                $"Flight summary did not contain the outbound route {origin} to {destination}.");
        }

        if (!isReturn)
        {
            return;
        }

        string returnPattern = $@"\b{System.Text.RegularExpressions.Regex.Escape(destination)}\b.*?\b{System.Text.RegularExpressions.Regex.Escape(origin)}\b";
        if (!System.Text.RegularExpressions.Regex.IsMatch(
                body,
                returnPattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            throw new InvalidOperationException(
                $"Return flight summary contained the outbound route but not the inbound route {destination} to {origin}. " +
                $"Page: {body[..Math.Min(body.Length, 1600)]}");
        }
    }

    public async Task ContinueToPassengerDetailsAsync(int timeoutMs)
    {
        await WaitUntilReadyAsync(timeoutMs);
        await VirginAtlanticConsent.DismissPrivacyBannerIfPresentAsync(Page);
        await Task.Delay(500);
        string? markerResult = await Page.EvaluateAsync<string?>(
            @"() => {
                document
                    .querySelectorAll('[data-eapi-playwright-target=""summary-continue""]')
                    .forEach((element) => element.removeAttribute('data-eapi-playwright-target'));

                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 &&
                        rect.height > 0 &&
                        style.display !== 'none' &&
                        style.visibility !== 'hidden' &&
                        style.pointerEvents !== 'none';
                };

                const button = [...document.querySelectorAll('button, a')]
                    .find((element) => isVisible(element) &&
                        /Continue to Passenger Details/i.test((element.innerText || '').trim()) &&
                        element.getAttribute('aria-disabled') !== 'true' &&
                        !element.disabled);

                if (!button) {
                    return 'no enabled Continue to Passenger Details button found';
                }

                button.scrollIntoView({ block: 'center', inline: 'center' });
                button.setAttribute('data-eapi-playwright-target', 'summary-continue');
                return null;
            }");

        if (!markerResult.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(markerResult);
        }

        ILocator continueButton = Page.Locator(ContinueMarkerSelector);
        await continueButton.ScrollIntoViewIfNeededAsync();
        await continueButton.ClickAsync(new LocatorClickOptions { Timeout = 15_000 });
        await WaitForPassengerDetailsAsync(timeoutMs);
    }

    public async Task<string> AssertInsufficientPointsBlocksContinueAsync(int timeoutMs, int? expectedBalance = null)
    {
        await WaitUntilReadyAsync(timeoutMs);
        ILocator warning = Page.Locator("[data-testid='alert-component']").Filter(
            new LocatorFilterOptions
            {
                HasTextRegex = new System.Text.RegularExpressions.Regex(
                    "Insufficient.*(?:Virgin )?Points|not enough.*(?:Virgin )?Points",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            }).First;
        await warning.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        string warningText = System.Text.RegularExpressions.Regex.Replace(
            await warning.InnerTextAsync(),
            @"\s+",
            " ").Trim();
        if (expectedBalance.HasValue)
        {
            string expectedBalanceText = expectedBalance.Value.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
            string compactWarningText = warningText.Replace(",", string.Empty, StringComparison.Ordinal);
            if (!warningText.Contains(expectedBalanceText, StringComparison.OrdinalIgnoreCase) &&
                !compactWarningText.Contains(expectedBalance.Value.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"The insufficient-points warning did not show the member's current balance of {expectedBalanceText}. Warning: {warningText}");
            }
        }

        ILocator buyPoints = Page.GetByRole(
            AriaRole.Link,
            new PageGetByRoleOptions
            {
                NameRegex = new System.Text.RegularExpressions.Regex(
                    "Buy Virgin Points",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            }).First;
        if (await buyPoints.CountAsync() == 0)
        {
            buyPoints = Page.GetByRole(
                AriaRole.Button,
                new PageGetByRoleOptions
                {
                    NameRegex = new System.Text.RegularExpressions.Regex(
                        "Buy Virgin Points",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                }).First;
        }
        await buyPoints.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
        ILocator continueButton = Page.GetByRole(
            AriaRole.Button,
            new PageGetByRoleOptions
            {
                NameRegex = new System.Text.RegularExpressions.Regex(
                    "Continue to Passenger Details",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            }).First;
        await continueButton.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        bool disabled = await continueButton.IsDisabledAsync() ||
            (await continueButton.GetAttributeAsync("aria-disabled")).Is("true");
        if (!disabled)
        {
            throw new InvalidOperationException(
                "The summary showed an insufficient-points warning, but Continue to Passenger Details remained enabled.");
        }

        return warningText;
    }

    public async Task<string> ApplyAvailableUpgradeAsync(int timeoutMs)
    {
        await WaitUntilReadyAsync(timeoutMs);
        ILocator upgradeButton = Page.GetByRole(
            AriaRole.Button,
            new PageGetByRoleOptions { Name = "Upgrade", Exact = true }).First;
        await upgradeButton.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        string offerText = await upgradeButton.EvaluateAsync<string>(
            @"button => {
                for (let element = button; element && element !== document.body; element = element.parentElement) {
                    const text = (element.innerText || '').replace(/\s+/g, ' ').trim();
                    if (/Upgrade to/i.test(text) && /\+\s*(?:[A-Z]{1,3}\$|[£$€])\s*\d/.test(text)) {
                        return text;
                    }
                }
                return (button.parentElement?.innerText || button.innerText || '').replace(/\s+/g, ' ').trim();
            }");
        System.Text.RegularExpressions.Match targetMatch =
            System.Text.RegularExpressions.Regex.Match(
                offerText,
                @"Upgrade to\s+(Economy Light|Economy Classic|Economy Delight|Premium|Upper Class)\b",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        string targetCabin = targetMatch.Success ? targetMatch.Groups[1].Value.Trim() : "upgraded cabin";

        await upgradeButton.ClickAsync(new LocatorClickOptions { Timeout = 15_000 });
        string upgradeAppliedScript = @"() => {
                const targetCabin = __TARGET_CABIN__;
                if (!location.href.includes('upgrade=')) return false;
                const normalise = value => String(value || '').replace(/\s+/g, ' ').trim().toLowerCase();
                const alerts = [...document.querySelectorAll('[data-testid=""alert-component""]')];
                return alerts.some(alert => {
                    const text = normalise(alert.textContent);
                    const className = String(alert.className || '');
                    return /status--success/.test(className) &&
                        (targetCabin === 'upgraded cabin' || text.includes(normalise(targetCabin)));
                });
            }".Replace(
                "__TARGET_CABIN__",
                System.Text.Json.JsonSerializer.Serialize(targetCabin),
                StringComparison.Ordinal);
        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        do
        {
            if (await Page.EvaluateAsync<bool>(upgradeAppliedScript))
            {
                return targetCabin;
            }

            await Task.Delay(500);
        }
        while (DateTime.UtcNow < deadline);

        throw new TimeoutException(
            $"The {targetCabin} upgrade was not confirmed within {timeoutMs}ms. URL: {Page.Url}");
    }

    public async Task WaitForPassengerDetailsAsync(int timeoutMs)
    {
        await Page.WaitForFunctionAsync(
            @"() => {
                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 &&
                        rect.height > 0 &&
                        style.display !== 'none' &&
                        style.visibility !== 'hidden';
                };

                const isPassengerUrl = window.location.href.includes('/passengers');
                const bodyText = (document.body && document.body.innerText || '').trim();
                const hasPassengerContent = /Passenger details|Who's flying|Who is flying|Your details|Lead passenger/i.test(bodyText);

                return isPassengerUrl && hasPassengerContent;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }
}

internal sealed class SummaryReadinessState
{
    public bool Ready { get; set; }

    public bool IncompleteSummaryShell { get; set; }
}

