using System.Text.RegularExpressions;
using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Text;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Pages;

/// <summary>
/// Owns interaction with the Reward Flight Finder and the observable cabin-price results.
/// </summary>
public sealed class RewardFlightFinderPage : BasePage
{
    private const string OriginSelector = "#origin";
    private const string DestinationSelector = "#destination";
    private const string MonthSelector = "#date";
    private const string AvailabilityCardSelector = "article[data-cy='availability-card']";
    private const string EconomySelector = "[data-cy='economy']";
    private const string PremiumSelector = "[data-cy='premium']";
    private const string UpperClassSelector = "[data-cy='upper-class']";
    private const string NoAvailabilitySelector = "[data-cy='no-availability-content']";
    private const string ResultMonthHeadingSelector = "[data-cy='date-subheading']";
    private const string NoAvailabilityMessage = "No flight on this day, or no seats left";
    private const string PointsPattern = @"\b(?<points>\d{1,3}(?:,\d{3})*)\s*(?:points?|pts)\b";

    public RewardFlightFinderPage(IPage page)
        : base(page)
    {
    }

    public async Task SearchAsync(
        string origin,
        string destination,
        string? month,
        int timeoutMs)
    {
        await Page.Locator(OriginSelector).WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
        await SelectNativeOptionWithUserInputAsync(OriginSelector, origin, timeoutMs);
        await Page.WaitForFunctionAsync(
            "([selector, value]) => Array.from(document.querySelector(selector)?.options ?? []).some(option => option.value === value)",
            new object[] { DestinationSelector, destination },
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
        await SelectNativeOptionWithUserInputAsync(DestinationSelector, destination, timeoutMs);

        if (month is not null && !month.IsNullOrWhiteSpace())
        {
            await SelectNativeOptionWithUserInputAsync(MonthSelector, month, timeoutMs);
        }

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
        {
            Name = "Find reward seats",
            Exact = true
        }).ClickAsync(new LocatorClickOptions
        {
            Timeout = timeoutMs
        });
        await WaitForResultsOrFailureAsync(timeoutMs);
    }

    public async Task<IReadOnlyList<string>> GetInvalidRequiredSearchFieldsAsync(int timeoutMs)
    {
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
        {
            Name = "Find reward seats",
            Exact = true
        }).ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });

        return await Page.Locator($"{OriginSelector}:invalid, {DestinationSelector}:invalid, {MonthSelector}:invalid")
            .EvaluateAllAsync<string[]>("elements => elements.map(element => element.id)");
    }

    public async Task<IReadOnlyList<string>> SelectOriginAndGetDestinationOptionsAsync(
        string origin,
        int timeoutMs)
    {
        await Page.Locator(OriginSelector).WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
        await SelectNativeOptionWithUserInputAsync(OriginSelector, origin, timeoutMs);

        return await Page.Locator(DestinationSelector).Locator("option")
            .EvaluateAllAsync<string[]>(
                "options => options.map(option => option.value).filter(Boolean)");
    }

    public async Task<IReadOnlyList<string>> GetMonthOptionValuesAsync(int timeoutMs)
    {
        ILocator month = Page.Locator(MonthSelector);
        await month.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        return await month.Locator("option").EvaluateAllAsync<string[]>(
            "options => options.map(option => option.value).filter(Boolean)");
    }

    public Task<string> GetSelectedMonthValueAsync(int timeoutMs) =>
        Page.Locator(MonthSelector).InputValueAsync(new LocatorInputValueOptions { Timeout = timeoutMs });

    public async Task<string> GetDisplayedResultMonthAsync(int timeoutMs)
    {
        ILocator monthHeading = Page.Locator(ResultMonthHeadingSelector);
        await monthHeading.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        return await monthHeading.InnerTextAsync();
    }

    public async Task<bool> IsMonthNavigationDisabledAsync(string name, int timeoutMs)
    {
        ILocator navigation = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
        {
            Name = name,
            Exact = true
        });
        await navigation.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        return await navigation.IsDisabledAsync();
    }

    public async Task NavigateMonthAsync(string name, int timeoutMs)
    {
        string previousMonth = await GetDisplayedResultMonthAsync(timeoutMs);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
        {
            Name = name,
            Exact = true
        }).ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });
        await Page.WaitForFunctionAsync(
            @"([selector, previousMonth]) => {
                const month = document.querySelector(selector)?.textContent?.trim();
                return Boolean(month) && month !== previousMonth.trim();
            }",
            new object[] { ResultMonthHeadingSelector, previousMonth },
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }

    public async Task<IReadOnlyDictionary<string, string>> GetCabinPointsAsync(int timeoutMs)
    {
        ILocator availabilityCards = Page.Locator(AvailabilityCardSelector)
            .Filter(new LocatorFilterOptions { Has = Page.Locator(EconomySelector) });
        await availabilityCards.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        int availabilityCardCount = await availabilityCards.CountAsync();
        for (int index = 0; index < availabilityCardCount; index++)
        {
            ILocator availabilityCard = availabilityCards.Nth(index);
            string? economyPoints = await TryReadCabinPointsAsync(availabilityCard, EconomySelector);
            string? premiumPoints = await TryReadCabinPointsAsync(availabilityCard, PremiumSelector);
            string? upperClassPoints = await TryReadCabinPointsAsync(availabilityCard, UpperClassSelector);

            if (economyPoints is not null && premiumPoints is not null && upperClassPoints is not null)
            {
                return new Dictionary<string, string>
                {
                    ["Economy"] = economyPoints,
                    ["Premium"] = premiumPoints,
                    ["Upper Class"] = upperClassPoints
                };
            }
        }

        throw new InvalidOperationException(
            "Reward Flight Finder did not show a day with points populated for every cabin. " +
            $"Diagnostics: {await GetDiagnosticsAsync()}");
    }

    public async Task ReverseRouteAsync(string origin, string destination, int timeoutMs)
    {
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
        {
            Name = $"View {destination} to {origin}",
            Exact = true
        }).ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });

        await WaitForRouteResultsAsync(destination, origin, timeoutMs);
    }

    public async Task EditSearchAsync(int timeoutMs)
    {
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
        {
            Name = "Edit Search",
            Exact = true
        }).ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });
        await Page.Locator(OriginSelector).WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
    }

    public async Task<IReadOnlyList<string>> GetNoAvailabilityMessagesAsync(int timeoutMs)
    {
        ILocator noAvailability = Page.Locator(AvailabilityCardSelector).Locator(NoAvailabilitySelector);
        await noAvailability.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        IReadOnlyList<string> messages = await noAvailability.AllInnerTextsAsync();
        if (messages.Count == 0 || messages.Any(message =>
            !message.Contains(NoAvailabilityMessage, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Reward Flight Finder did not render the expected no-availability message. " +
                $"Diagnostics: {await GetDiagnosticsAsync()}");
        }

        return messages;
    }

    public async Task<string> GetDiagnosticsAsync()
    {
        return await Page.EvaluateAsync<string>(
            @"() => JSON.stringify({
                title: document.title,
                url: window.location.href,
                body: String(document.body?.innerText || '').replace(/\s+/g, ' ').trim().slice(0, 1500)
            })");
    }

    private async Task WaitForResultsOrFailureAsync(int timeoutMs)
    {
        try
        {
            await Page.WaitForFunctionAsync(
                @"() => {
                    const body = String(document.body?.innerText || '').replace(/\s+/g, ' ').trim();
                    return document.querySelector(""article[data-cy='availability-card']"") ||
                        /Hmm, something's not working/i.test(body);
                }",
                null,
                new PageWaitForFunctionOptions { Timeout = timeoutMs });
        }
        catch (TimeoutException exception)
        {
            string diagnostics = await GetDiagnosticsAsync();
            throw new InvalidOperationException(
                $"Reward Flight Finder did not render a results or application-failure state. {diagnostics}",
                exception);
        }

        string body = await Page.Locator("body").InnerTextAsync();
        if (body.Contains("Hmm, something's not working", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Reward Flight Finder returned an application failure. {await GetDiagnosticsAsync()}");
        }
    }

    private async Task WaitForRouteResultsAsync(string origin, string destination, int timeoutMs)
    {
        try
        {
            await Page.WaitForFunctionAsync(
                @"([origin, destination]) => {
                    const url = new URL(window.location.href);
                    return url.searchParams.get('origin') === origin &&
                        url.searchParams.get('destination') === destination &&
                        document.querySelector(""article[data-cy='availability-card']"");
                }",
                new object[] { origin, destination },
                new PageWaitForFunctionOptions { Timeout = timeoutMs });
        }
        catch (TimeoutException exception)
        {
            string diagnostics = await GetDiagnosticsAsync();
            throw new InvalidOperationException(
                $"Reward Flight Finder did not render the expected {origin} to {destination} route. {diagnostics}",
                exception);
        }
    }

    private static async Task<string?> TryReadCabinPointsAsync(ILocator availabilityCard, string cabinSelector)
    {
        ILocator cabin = availabilityCard.Locator(cabinSelector);
        if (await cabin.CountAsync() == 0)
        {
            return null;
        }

        string cabinText = await cabin.InnerTextAsync();
        Match points = RegexCompiled.Match(cabinText, PointsPattern, RegexOptions.IgnoreCase);
        return points.Success ? points.Groups["points"].Value : null;
    }

    private async Task SelectNativeOptionWithUserInputAsync(string selector, string value, int timeoutMs)
    {
        ILocator select = Page.Locator(selector);
        await select.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        const int maximumSelectionAttempts = 3;
        for (int attempt = 1; attempt <= maximumSelectionAttempts; attempt++)
        {
            IReadOnlyList<string> optionLabels = await select.Locator("option").AllTextContentsAsync();
            int optionIndex = await select.EvaluateAsync<int>(
                "(element, optionValue) => Array.from(element.options).findIndex(option => option.value === optionValue)",
                value);
            if (optionIndex < 0)
            {
                throw new InvalidOperationException(
                    $"The select control '{selector}' does not contain the option '{value}'. " +
                    $"Diagnostics: {await GetDiagnosticsAsync()}");
            }

            string optionPrefix = FindUniqueOptionPrefix(optionLabels, optionLabels[optionIndex]);
            await select.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });
            await Page.Keyboard.TypeAsync(optionPrefix);
            await Page.Keyboard.PressAsync("Enter");

            string selectedValue = await select.InputValueAsync(new LocatorInputValueOptions { Timeout = timeoutMs });
            if (selectedValue.Equals(value, StringComparison.Ordinal))
            {
                return;
            }

            if (attempt < maximumSelectionAttempts)
            {
                await Page.WaitForTimeoutAsync(250);
            }
        }

        string finalSelectedValue = await select.InputValueAsync(new LocatorInputValueOptions { Timeout = timeoutMs });
        throw new InvalidOperationException(
            $"The select control '{selector}' did not select '{value}'. It selected '{finalSelectedValue}'. " +
            $"Diagnostics: {await GetDiagnosticsAsync()}");
    }

    private static string FindUniqueOptionPrefix(IReadOnlyList<string> optionLabels, string optionLabel)
    {
        string normalizedOptionLabel = optionLabel.Trim();
        for (int length = 1; length <= normalizedOptionLabel.Length; length++)
        {
            string prefix = normalizedOptionLabel[..length];
            int matches = optionLabels.Count(label => label.Trim()
                .StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            if (matches == 1)
            {
                return prefix;
            }
        }

        return normalizedOptionLabel;
    }

}
