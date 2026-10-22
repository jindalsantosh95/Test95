using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Pages;

public sealed class VoucherRedemptionPage : BasePage
{
    public VoucherRedemptionPage(IPage page)
        : base(page)
    {
    }

    public async Task AssertFlightSelectionGuidanceAndAcceptAsync(int timeoutMs)
    {
        ILocator modal = Page.Locator("[role=\"dialog\"], [aria-modal=\"true\"]")
            .Filter(new LocatorFilterOptions { HasTextString = "Choose the flight and cabin you want" })
            .First;
        await modal.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        string modalText = await modal.InnerTextAsync();
        string[] expectedMessages =
        [
            "Choose the flight and cabin you want",
            "We'll apply the option that gives you the best value",
            "Redeem your voucher(s) at checkout"
        ];
        string[] missingMessages = expectedMessages
            .Where(message => !modalText.Contains(message, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (missingMessages.Length > 0)
        {
            throw new InvalidOperationException(
                $"Voucher flight-selection guidance was incomplete. Missing: {string.Join(" | ", missingMessages)}. " +
                $"Modal: {modalText}");
        }

        await modal.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions
        {
            Name = "Got it thanks",
            Exact = true
        }).ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });
        await modal.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = timeoutMs
        });
    }

    public async Task AcceptFlightSelectionGuidanceIfShownAsync(int timeoutMs = 2_000)
    {
        ILocator modal = Page.Locator("[role=\"dialog\"], [aria-modal=\"true\"]")
            .Filter(new LocatorFilterOptions { HasTextString = "Choose the flight and cabin you want" })
            .First;
        try
        {
            await modal.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
        }
        catch (TimeoutException)
        {
            return;
        }

        await modal.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions
        {
            Name = "Got it thanks",
            Exact = true
        }).ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });
        await modal.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = timeoutMs
        });
    }

    public async Task<VoucherUiSnapshot> WaitForOptionsAsync(int timeoutMs)
    {
        await Page.WaitForFunctionAsync(
            @"() => {
                const visible = element => {
                    const rect = element.getBoundingClientRect();
                    const style = getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                const controls = [...document.querySelectorAll('button, label, summary, [role=radio], [role=option]')]
                    .map(element => element.innerText || element.textContent || '');
                const summaryCards = [...document.querySelectorAll(
                    'article[data-testid^=""voucher-price-summary__voucher-saving__option-""]')]
                    .filter(visible)
                    .map(card => card.innerText || card.textContent || '');
                return [...controls, ...summaryCards].some(text =>
                    /voucher|best value|virgin points/i.test(text) &&
                    /upgrade|companion|use|apply/i.test(text));
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });

        VoucherUiSnapshot snapshot = await CaptureAsync();
        if (snapshot.OptionTexts.Length == 0)
        {
            throw new InvalidOperationException(
                $"Voucher redemption controls were detected but no option text could be captured. Page: {snapshot.PageText}");
        }

        return snapshot;
    }

    public async Task AssertSummaryGuidanceAndAcceptAsync(int timeoutMs)
    {
        ILocator modal = Page.Locator("[role=\"dialog\"], [aria-modal=\"true\"]")
            .Filter(new LocatorFilterOptions { HasTextString = "Your voucher saving is already factored in" })
            .First;
        await modal.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        string modalText = await modal.InnerTextAsync();
        string[] expectedMessages =
        [
            "Your voucher saving is already factored in the price summary",
            "We've applied the option that gives you the best value",
            "You can redeem or remove vouchers at checkout"
        ];
        string[] missingMessages = expectedMessages
            .Where(message => !modalText.Contains(message, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (missingMessages.Length > 0)
        {
            throw new InvalidOperationException(
                $"Voucher Summary guidance was incomplete. Missing: {string.Join(" | ", missingMessages)}. " +
                $"Modal: {modalText}");
        }

        await modal.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions
        {
            Name = "Got it thanks",
            Exact = true
        }).ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });
        await modal.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = timeoutMs
        });
    }

    public async Task AssertNoOptionsAsync(int settleMs = 3_000)
    {
        await Task.Delay(settleMs);
        VoucherUiSnapshot snapshot = await CaptureAsync();
        if (snapshot.OptionTexts.Length > 0)
        {
            throw new InvalidOperationException(
                $"Voucher redemption remained available when it should have been excluded. Options: {string.Join(" | ", snapshot.OptionTexts)}");
        }
    }

    public async Task<VoucherTieSnapshot> AssertUpgradePreferredWhenAtLeastAsValuableAsync(int timeoutMs)
    {
        VoucherUiSnapshot snapshot = await WaitForOptionsAsync(timeoutMs);
        string upgrade = FindOption(snapshot, "upgrade");
        string companion = FindOption(snapshot, "companion");
        decimal? upgradePointsToPay = ExtractPointsToPay(upgrade);
        decimal? companionPointsToPay = ExtractPointsToPay(companion);
        if (!upgradePointsToPay.HasValue ||
            !companionPointsToPay.HasValue ||
            upgradePointsToPay.Value > companionPointsToPay.Value)
        {
            throw new InvalidOperationException(
                $"Upgrade was not at least as valuable as Companion. Upgrade: '{upgrade}'; Companion: '{companion}'.");
        }

        if (!Regex.IsMatch(upgrade, "best|recommended|selected|default", RegexOptions.IgnoreCase))
        {
            throw new InvalidOperationException(
                $"Upgrade was at least as valuable as Companion but was not marked as preferred. Upgrade: '{upgrade}'.");
        }

        return new VoucherTieSnapshot(upgrade, companion, [upgradePointsToPay.Value]);
    }

    public async Task AssertUpgradePreferredAtPaymentAsync(int timeoutMs)
    {
        await Page.WaitForURLAsync(
            url => url.Contains("/payment", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = timeoutMs, WaitUntil = WaitUntilState.Commit });

        ILocator voucherSection = Page.Locator("section.payment-vouchers").First;
        await voucherSection.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        string appliedText = await voucherSection.InnerTextAsync();
        bool upgradeApplied = Regex.IsMatch(
            appliedText,
            @"covered your cabin upgrade|cabin upgrade.{0,160}(?:applied|selected)|(?:applied|selected).{0,160}cabin upgrade",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (!upgradeApplied)
        {
            throw new InvalidOperationException(
                $"Payment did not show the applied cabin-upgrade voucher. Voucher section: {appliedText}");
        }

        ILocator explanationToggle = Page.Locator("button")
            .Filter(new LocatorFilterOptions
            {
                HasTextRegex = new Regex("How did we work out your saving", RegexOptions.IgnoreCase)
            })
            .First;
        if (await explanationToggle.CountAsync() > 0 &&
            await explanationToggle.GetAttributeAsync("aria-expanded") != "true")
        {
            await explanationToggle.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });
        }

        ILocator bestValue = Page.Locator(
                "article[data-testid^=\"voucher-price-summary__voucher-saving__option-\"], article")
            .Filter(new LocatorFilterOptions
            {
                HasTextRegex = new Regex("Cabin upgrade", RegexOptions.IgnoreCase)
            })
            .First;
        await bestValue.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
        string bestValueText = await bestValue.InnerTextAsync();
        if (!Regex.IsMatch(bestValueText, "best value", RegexOptions.IgnoreCase))
        {
            throw new InvalidOperationException(
                $"Payment did not mark Cabin Upgrade as best value. Price-summary option: {bestValueText}");
        }
    }

    public async Task<VoucherPaymentSnapshot> WaitForPaymentOptionsAsync(int timeoutMs)
    {
        await Page.WaitForURLAsync(
            url => url.Contains("/payment", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = timeoutMs, WaitUntil = WaitUntilState.Commit });

        ILocator voucherSection = Page.Locator("section.payment-vouchers").First;
        await voucherSection.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        VoucherPaymentSnapshot snapshot = await CapturePaymentAsync();
        if (snapshot.OptionTexts.Length == 0)
        {
            throw new InvalidOperationException(
                $"The Payment voucher section was visible but exposed no redemption options. Section: {snapshot.SectionText}");
        }

        AssertNoTechnicalError(snapshot);
        return snapshot;
    }

    public async Task<VoucherPaymentSnapshot> SelectPaymentOptionAsync(string redemptionType, int timeoutMs)
    {
        VoucherPaymentSnapshot before = await WaitForPaymentOptionsAsync(timeoutMs);
        bool alreadySelected = before.SelectedOption.Contains(redemptionType, StringComparison.OrdinalIgnoreCase) ||
            (redemptionType.Is("Companion") &&
                before.SectionText.Contains("covered your companion seat", StringComparison.OrdinalIgnoreCase)) ||
            (redemptionType.Is("Upgrade") &&
                before.SectionText.Contains("covered your cabin upgrade", StringComparison.OrdinalIgnoreCase));
        if (alreadySelected)
        {
            return before;
        }

        ILocator voucherSection = Page.Locator("section.payment-vouchers").First;
        Regex optionName = new(redemptionType, RegexOptions.IgnoreCase);
        bool requiresRedeem = false;
        ILocator optionControl = voucherSection.GetByRole(
            AriaRole.Radio,
            new LocatorGetByRoleOptions { NameRegex = optionName }).First;
        if (await optionControl.CountAsync() == 0)
        {
            optionControl = voucherSection.GetByRole(
                AriaRole.Button,
                new LocatorGetByRoleOptions { NameRegex = optionName }).First;
        }

        if (await optionControl.CountAsync() == 0)
        {
            optionControl = voucherSection.Locator("input[type=checkbox]").First;
            requiresRedeem = await optionControl.CountAsync() > 0;
        }

        if (await optionControl.CountAsync() == 0)
        {
            throw new InvalidOperationException(
                $"Payment did not expose a selectable {redemptionType} option. Options: {string.Join(" | ", before.OptionTexts)}");
        }

        await optionControl.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });
        if (requiresRedeem)
        {
            await voucherSection.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions
            {
                Name = "Redeem",
                Exact = true
            }).ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });
        }
        await Page.WaitForFunctionAsync(
            @"([type]) => {
                const section = document.querySelector('section.payment-vouchers');
                if (!section) return false;
                const normalise = value => String(value || '').replace(/\s+/g, ' ').trim();
                const sectionText = normalise(section.innerText);
                const appliedText = type.toLowerCase() === 'companion'
                    ? /covered your companion seat/i.test(sectionText)
                    : /covered your cabin upgrade/i.test(sectionText);
                if (appliedText) return true;
                const controls = [...section.querySelectorAll('input[type=radio], [role=radio], button')];
                return controls.some(control => {
                    const text = normalise(control.innerText || control.textContent ||
                        control.getAttribute('aria-label') || control.closest('label, article')?.innerText);
                    const selected = control.checked || control.getAttribute('aria-checked') === 'true' ||
                        control.getAttribute('aria-pressed') === 'true' ||
                        /selected|applied/i.test(normalise(control.closest('label, article')?.innerText));
                    return new RegExp(type, 'i').test(text) && selected;
                });
            }",
            new[] { redemptionType },
            new PageWaitForFunctionOptions { Timeout = timeoutMs });

        VoucherPaymentSnapshot after = await CapturePaymentAsync();
        AssertNoTechnicalError(after);
        return after;
    }

    public async Task<VoucherPaymentSnapshot> RemoveSelectedVoucherAsync(int timeoutMs)
    {
        VoucherPaymentSnapshot before = await WaitForPaymentOptionsAsync(timeoutMs);
        ILocator voucherSection = Page.Locator("section.payment-vouchers").First;
        ILocator remove = voucherSection.GetByRole(
            AriaRole.Button,
            new LocatorGetByRoleOptions
            {
                NameRegex = new Regex("remove", RegexOptions.IgnoreCase)
            }).First;
        await remove.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
        await remove.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });

        await Page.WaitForFunctionAsync(
            @"([previous]) => {
                const section = document.querySelector('section.payment-vouchers');
                const text = String(section?.innerText || '').replace(/\s+/g, ' ').trim();
                return text !== previous && /removed|select|apply|redeem|available|request failed/i.test(text);
            }",
            new[] { before.SectionText },
            new PageWaitForFunctionOptions { Timeout = timeoutMs });

        VoucherPaymentSnapshot after = await CapturePaymentAsync();
        AssertNoTechnicalError(after);
        return after;
    }

    public async Task<string> AssertUnavailableVoucherShowsPointsOptionAsync(int timeoutMs)
    {
        try
        {
            await Page.WaitForFunctionAsync(
                @"() => {
                const text = String(document.body?.innerText || '').replace(/\s+/g, ' ');
                const unavailable = /voucher[^.]{0,160}(?:unavailable|not available|can(?:not|'t) be used|not eligible)/i.test(text) ||
                    /(?:unavailable|not available|not eligible)[^.]{0,160}voucher/i.test(text);
                const pointsOption = [...document.querySelectorAll(
                    'button, a, label, summary, [role=button], [role=radio], [role=option]')]
                    .some(element => {
                        const rect = element.getBoundingClientRect();
                        const style = getComputedStyle(element);
                        const visible = rect.width > 0 && rect.height > 0 &&
                            style.display !== 'none' && style.visibility !== 'hidden';
                        return visible && /(?:Virgin )?Points/i.test(element.innerText || element.textContent || '');
                    });
                return unavailable && pointsOption;
            }",
                null,
                new PageWaitForFunctionOptions { Timeout = timeoutMs });
        }
        catch (TimeoutException exception)
        {
            throw new InvalidOperationException(
                $"The Summary page did not show both the voucher-unavailable message and a visible Points option within {timeoutMs} ms.",
                exception);
        }

        return "Voucher unavailable state and points option were displayed.";
    }

    private static void AssertNoTechnicalError(VoucherPaymentSnapshot snapshot)
    {
        if (!snapshot.ErrorText.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(
                $"Payment displayed a technical voucher error: {snapshot.ErrorText}. Section: {snapshot.SectionText}");
        }
    }

    private async Task<VoucherPaymentSnapshot> CapturePaymentAsync()
    {
        return await Page.EvaluateAsync<VoucherPaymentSnapshot>(
            @"() => {
                const section = document.querySelector('section.payment-vouchers');
                const normalise = value => String(value || '').replace(/\s+/g, ' ').trim();
                const isVisible = element => {
                    if (!element) return false;
                    const rect = element.getBoundingClientRect();
                    const style = getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                const optionElements = [...(section?.querySelectorAll(
                    'article, label, [role=radio], input[type=radio]') || [])]
                    .filter(isVisible)
                    .map(element => normalise(element.innerText || element.textContent ||
                        element.getAttribute('aria-label') || element.closest('label, article')?.innerText))
                    .filter(text => /upgrade|companion/i.test(text));
                const sectionText = normalise(section?.innerText);
                if (optionElements.length === 0 &&
                    /voucher(?:s|\(s\))? covered your (?:companion seat|cabin upgrade)/i.test(sectionText)) {
                    optionElements.push(sectionText);
                }
                const dateMatches = sectionText.match(
                    /(?:\d{1,2}[\/-]\d{1,2}[\/-]\d{2,4}|\d{1,2}\s+[A-Za-z]{3,9}\s+\d{4}|\d{4}-\d{2}-\d{2})/g) || [];
                const selected = [...(section?.querySelectorAll(
                    'input[type=radio]:checked, [role=radio][aria-checked=true], [aria-pressed=true]') || [])]
                    .map(element => normalise(element.getAttribute('aria-label') ||
                        element.closest('label, article')?.innerText || element.textContent))[0] || '';
                const error = [...document.querySelectorAll('[role=alert], .error, [class*=error]')]
                    .filter(isVisible)
                    .map(element => normalise(element.innerText || element.textContent))
                    .find(text => /request failed|technical error|something.?s not right|field .?id.? does not exist|unable to (?:apply|remove|redeem)/i.test(text)) || '';
                return {
                    OptionTexts: [...new Set(optionElements)],
                    ExpiryDates: [...new Set(dateMatches)],
                    SelectedOption: selected,
                    ErrorText: error,
                    SectionText: sectionText.slice(0, 4000)
                };
            }");
    }

    private async Task<VoucherUiSnapshot> CaptureAsync()
    {
        return await Page.EvaluateAsync<VoucherUiSnapshot>(
            @"() => {
                const visible = element => {
                    const rect = element.getBoundingClientRect();
                    const style = getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                const normalise = value => String(value || '').replace(/\s+/g, ' ').trim();
                const controls = [...document.querySelectorAll('button, label, summary, [role=radio], [role=option]')]
                    .filter(visible)
                    .map(element => normalise(element.innerText || element.textContent))
                    .filter(text => /voucher/i.test(text) && /upgrade|companion|use|apply/i.test(text));
                const summaryCards = [...document.querySelectorAll(
                    'article[data-testid^=""voucher-price-summary__voucher-saving__option-""]')]
                    .filter(visible)
                    .map(card => normalise(card.innerText || card.textContent))
                    .filter(text => /voucher|best value|virgin points/i.test(text) && /upgrade|companion/i.test(text));
                return {
                    OptionTexts: [...new Set([...controls, ...summaryCards])],
                    PageText: normalise(document.body?.innerText).slice(0, 2000)
                };
            }");
    }

    private static string FindOption(VoucherUiSnapshot snapshot, string type)
    {
        return snapshot.OptionTexts.FirstOrDefault(option =>
                   option.Contains(type, StringComparison.OrdinalIgnoreCase))
               ?? throw new InvalidOperationException(
                   $"No {type} voucher option was shown. Options: {string.Join(" | ", snapshot.OptionTexts)}");
    }

    private static IReadOnlyList<decimal> ExtractAmounts(string option)
    {
        return Regex.Matches(option, @"\d[\d,]*(?:\.\d+)?")
            .Select(match => decimal.Parse(
                match.Value.Replace(",", string.Empty, StringComparison.Ordinal),
                CultureInfo.InvariantCulture))
            .ToArray();
    }

    private static decimal? ExtractPointsToPay(string option)
    {
        Match match = Regex.Match(
            option,
            @"Virgin\s+Points\s+to\s+pay\s+([\d,]+(?:\.\d+)?)",
            RegexOptions.IgnoreCase);
        return match.Success
            ? decimal.Parse(
                match.Groups[1].Value.Replace(",", string.Empty, StringComparison.Ordinal),
                CultureInfo.InvariantCulture)
            : null;
    }
}

public sealed class VoucherUiSnapshot
{
    public string[] OptionTexts { get; set; } = [];

    public string PageText { get; set; } = string.Empty;
}

public sealed record VoucherTieSnapshot(
    string UpgradeOption,
    string CompanionOption,
    IReadOnlyList<decimal> Amounts);

public sealed class VoucherPaymentSnapshot
{
    public string[] OptionTexts { get; set; } = [];

    public string[] ExpiryDates { get; set; } = [];

    public string SelectedOption { get; set; } = string.Empty;

    public string ErrorText { get; set; } = string.Empty;

    public string SectionText { get; set; } = string.Empty;
}
