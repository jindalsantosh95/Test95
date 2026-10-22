using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Extensions;
namespace VAA.EAPI.Automation.SearchAndBook.UI.Pages;

public sealed class FlightResultsPage : BasePage
{
    private const string FlightCardSelector = "[data-testid=\"flight-card\"]";
    private const string LoaderSelector = "[data-testid=\"search-loader\"], [data-testid=\"tailfin-loader-component\"], [data-testid=\"interstitial-component\"]";
    private const string DrawerSelector = "[data-testid=\"drawer\"]";
    private const string FarePickMarkerSelector = "[data-eapi-playwright-target=\"fare-pick\"]";
    public FlightResultsPage(IPage page)
        : base(page)
    {
    }

    public async Task WaitUntilLoadedAsync(int timeoutMs)
    {
        await Page.WaitForURLAsync(
            url => url.Contains("/flights/search", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions
            {
                Timeout = timeoutMs,
                WaitUntil = WaitUntilState.Commit
            });

        try
        {
            await Page.EvaluateAsync(
                @"() => {
                    delete window.__eapiResultsReadyFingerprint;
                    delete window.__eapiResultsReadySince;
                }");
            await Page.WaitForFunctionAsync(
                @"() => {
                const isVisible = element => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden' &&
                        Number(style.opacity || 1) > 0;
                };
                const cards = [...document.querySelectorAll('[data-testid=""flight-card""]')].filter(isVisible);
                const visibleLoaders = [...document.querySelectorAll('[data-testid=""search-loader""], [data-testid=""tailfin-loader-component""], [data-testid=""interstitial-component""]')]
                    .filter(isVisible);

                if (cards.length === 0 || visibleLoaders.length > 0) {
                    delete window.__eapiResultsReadyFingerprint;
                    delete window.__eapiResultsReadySince;
                    return false;
                }

                const normalise = value => String(value || '').replace(/\s+/g, ' ').trim();
                const fingerprint = JSON.stringify(cards.slice(0, 8)
                    .map(card => normalise(card.innerText || card.textContent || '').slice(0, 800)));
                if (window.__eapiResultsReadyFingerprint !== fingerprint) {
                    window.__eapiResultsReadyFingerprint = fingerprint;
                    window.__eapiResultsReadySince = performance.now();
                    return false;
                }

                return performance.now() - window.__eapiResultsReadySince >= 1000;
            }",
                null,
                new PageWaitForFunctionOptions { Timeout = timeoutMs });
        }
        catch (TimeoutException exception)
        {
            string diagnostics = await GetLoadDiagnosticsAsync();
            throw new InvalidOperationException($"Flight results did not become ready. {diagnostics}", exception);
        }

    }

    public async Task<int> GetFlightCardCountAsync()
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            int count = await Page.Locator(FlightCardSelector).CountAsync();
            if (count > 0)
            {
                return count;
            }

            await Task.Delay(500);
        }

        return 0;
    }

    public async Task<string> GetSearchSummaryAsync()
    {
        string title = await Page.TitleAsync();
        string url = Page.Url;
        int flightCardCount = await GetFlightCardCountAsync();

        return $"title=\"{title}\" url=\"{url}\" flightCards={flightCardCount}";
    }

    private async Task<string> GetLoadDiagnosticsAsync()
    {
        string state = await Page.EvaluateAsync<string>(
            @"() => {
                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                const cards = document.querySelectorAll('[data-testid=""flight-card""]').length;
                const loaders = [...document.querySelectorAll('[data-testid=""search-loader""], [data-testid=""tailfin-loader-component""], [data-testid=""interstitial-component""]')].filter(isVisible).length;
                const body = (document.body?.innerText || '').replace(/\s+/g, ' ').trim().slice(0, 800);
                return JSON.stringify({ cards, visibleLoaders: loaders, body });
            }");

        return $"title=\"{await Page.TitleAsync()}\" url=\"{Page.Url.Split('#')[0]}\" state={state}";
    }

    public async Task<string> SelectAnyAvailableFareAndWaitForSummaryAsync(int timeoutMs, int expectedSlices = 1)
    {
        List<string> selectedSlices = [];
        List<string> transitions = [];
        for (int slice = 0; slice < expectedSlices; slice++)
        {
            if (await IsFlightSummaryRenderedAsync())
            {
                return string.Join(" / ", selectedSlices);
            }

            string previousUrl = Page.Url;
            await WaitUntilLoadedAsync(timeoutMs);
            string previousResultsFingerprint = await GetResultsFingerprintAsync();
            string pickedFare = await ClickFirstEnabledPricedFareAsync(null);
            await WaitForFareDrawerAsync(timeoutMs);
            await Task.Delay(1_500);
            string selectedDrawerFare = await ClickFirstEnabledDrawerSelectAsync(
                Environment.GetEnvironmentVariable("SAB_PREFERRED_FARE") ?? "Economy Classic");
            selectedSlices.Add($"{pickedFare} / {selectedDrawerFare}");
            await Task.Delay(800);
            await DismissOptionalModalButtonAsync("Continue with standard", 5_000);
            await DismissOptionalModalButtonAsync("Select cabin", 5_000, 3_000);
            await WaitForSelectionProgressAsync(previousUrl, previousResultsFingerprint, timeoutMs);
            transitions.Add($"slice {slice + 1}: {previousUrl} -> {Page.Url}");
        }

        await WaitForSummaryReadyAsync(timeoutMs);

        return string.Join(" / ", selectedSlices);
    }

    public async Task<string> SelectAnyAvailableFareOnCarrierAndWaitForSummaryAsync(
        string carrierCode,
        int timeoutMs,
        int expectedSlices = 1)
    {
        List<string> selectedSlices = [];
        List<string> transitions = [];
        for (int slice = 0; slice < expectedSlices; slice++)
        {
            if (await IsFlightSummaryRenderedAsync())
            {
                return string.Join(" / ", selectedSlices);
            }

            string previousUrl = Page.Url;
            await WaitUntilLoadedAsync(timeoutMs);
            string previousResultsFingerprint = await GetResultsFingerprintAsync();
            string pickedFare = await ClickFirstEnabledPricedFareAsync(carrierCode);
            await WaitForFareDrawerAsync(timeoutMs);
            string selectedDrawerFare = await ClickFirstEnabledDrawerSelectAsync(string.Empty);
            selectedSlices.Add($"{pickedFare} / {selectedDrawerFare}");

            await DismissOptionalModalButtonAsync("Continue with standard", 5_000);
            await DismissOptionalModalButtonAsync("Select cabin", 5_000, 3_000);
            await WaitForSelectionProgressAsync(previousUrl, previousResultsFingerprint, timeoutMs);
            transitions.Add($"slice {slice + 1}: {previousUrl} -> {Page.Url}");
        }

        await WaitForSummaryReadyAsync(timeoutMs);
        return string.Join(" / ", selectedSlices);
    }

    public async Task<string> SelectAnyAvailablePartnerFareAndWaitForSummaryAsync(int timeoutMs)
    {
        string previousUrl = Page.Url;
        await WaitUntilLoadedAsync(timeoutMs);
        string previousResultsFingerprint = await GetResultsFingerprintAsync();
        string pickedFare = await ClickFirstEnabledPartnerFareAsync();
        await WaitForFareDrawerAsync(timeoutMs);
        string selectedDrawerFare = await ClickFirstEnabledDrawerSelectAsync(string.Empty);

        await DismissOptionalModalButtonAsync("Continue with standard", 5_000);
        await DismissOptionalModalButtonAsync("Select cabin", 5_000, 3_000);
        await WaitForSelectionProgressAsync(previousUrl, previousResultsFingerprint, timeoutMs);
        await WaitForSummaryReadyAsync(timeoutMs);
        return $"{pickedFare} / {selectedDrawerFare}";
    }

    public async Task<string> SelectVirginAtlanticFareAndWaitForSummaryAsync(
        string fareName,
        int timeoutMs,
        int expectedSlices = 1)
    {
        const string virginAtlanticCarrierCode = "vs";
        List<string> selectedSlices = new();
        List<string> transitions = new();

        for (int slice = 0; slice < expectedSlices; slice++)
        {
            if (await IsFlightSummaryRenderedAsync())
            {
                return string.Join(" / ", selectedSlices);
            }

            string urlBefore = Page.Url;
            await WaitUntilLoadedAsync(timeoutMs);
            string previousResultsFingerprint = await GetResultsFingerprintAsync();
            string? fallbackCabinName = fareName.StartsWith("Economy ", StringComparison.OrdinalIgnoreCase)
                ? "Economy"
                : null;
            string selectedFlightFare = await ClickRequestedFareAsync(
                fareName,
                virginAtlanticCarrierCode,
                fallbackCabinName);
            await WaitForFareDrawerAsync(timeoutMs);
            bool requestedFareDetailsLoaded = await WaitForRequestedFareDrawerAsync(
                fareName,
                Math.Min(timeoutMs, 10_000));
            string selectedDrawerFare = await ClickFirstEnabledDrawerSelectAsync(
                fareName,
                requireExactFare: requestedFareDetailsLoaded);
            selectedSlices.Add($"{selectedFlightFare} / {selectedDrawerFare}");

            await DismissOptionalModalButtonAsync("Continue with standard", 5_000);
            await DismissOptionalModalButtonAsync("Select cabin", 5_000, 3_000);

            await WaitForSelectionProgressAsync(urlBefore, previousResultsFingerprint, timeoutMs);
            transitions.Add($"slice {slice + 1}: {urlBefore} -> {Page.Url}");
        }

        await WaitForSummaryReadyAsync(timeoutMs);
        return string.Join(" / ", selectedSlices);
    }

    public async Task<string> SelectVirginAtlanticPremiumWithLargestUpgradeDeltaAndWaitForSummaryAsync(
        int timeoutMs,
        int expectedSlices = 1)
    {
        List<string> selectedSlices = [];
        for (int slice = 0; slice < expectedSlices; slice++)
        {
            if (await IsFlightSummaryRenderedAsync())
            {
                return string.Join(" / ", selectedSlices);
            }

            string urlBefore = Page.Url;
            await WaitUntilLoadedAsync(timeoutMs);
            string previousResultsFingerprint = await GetResultsFingerprintAsync();
            string selectedFlightFare = await ClickPremiumFareWithLargestUpgradeDeltaAsync();
            await WaitForRequestedFareDrawerAsync("Premium", timeoutMs);
            string selectedDrawerFare = await ClickFirstEnabledDrawerSelectAsync("Premium", requireExactFare: true);
            selectedSlices.Add($"{selectedFlightFare} / {selectedDrawerFare}");

            await DismissOptionalModalButtonAsync("Continue with standard", 5_000);
            await DismissOptionalModalButtonAsync("Select cabin", 5_000, 3_000);
            await WaitForSelectionProgressAsync(urlBefore, previousResultsFingerprint, timeoutMs);
        }

        await WaitForSummaryReadyAsync(timeoutMs);
        return string.Join(" / ", selectedSlices);
    }

    private async Task<bool> IsFlightSummaryRenderedAsync()
    {
        return await Page.EvaluateAsync<bool>(
            @"() => {
                const isVisible = element => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                return [...document.querySelectorAll('h1, h2, h3')]
                    .some(element => isVisible(element) &&
                        /^Flight summary$/i.test((element.innerText || '').trim()));
            }");
    }

    public async Task WaitForSummaryReadyAsync(int timeoutMs)
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

                const loadersVisible = [...document.querySelectorAll('[data-testid=""search-loader""], [data-testid=""tailfin-loader-component""], [data-testid=""interstitial-component""]')].some(isVisible);
                const isSummaryUrl = window.location.href.includes('/flights/search/summary');
                const summaryRendered = [...document.querySelectorAll('h1, h2, h3')]
                    .some(element => isVisible(element) &&
                        /^Flight summary$/i.test((element.innerText || '').trim()));
                const hasContinue = [...document.querySelectorAll('button, a')]
                    .some((element) => isVisible(element) &&
                        /Continue to Passenger Details/i.test((element.innerText || '').trim()));
                const hasAlert = [...document.querySelectorAll('[data-testid=""alert-component""]')].some(isVisible);

                return (isSummaryUrl || summaryRendered) && !loadersVisible && (hasContinue || hasAlert);
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }

    private async Task<string> ClickFirstEnabledPricedFareAsync(string? carrierCode)
    {
        ILocator cards = Page.Locator(FlightCardSelector);
        int cardCount = await cards.CountAsync();
        for (int cardIndex = 0; cardIndex < cardCount; cardIndex++)
        {
            ILocator card = cards.Nth(cardIndex);
            if (!carrierCode.IsNullOrWhiteSpace())
            {
                string carrierSelector = $"[data-testid=\"carrier-logo-component-carriers-icon-{carrierCode!.ToLowerInvariant()}\"]";
                if (await card.Locator(carrierSelector).CountAsync() == 0)
                {
                    continue;
                }
            }

            ILocator buttons = card.Locator("button");
            int buttonCount = await buttons.CountAsync();
            for (int buttonIndex = 0; buttonIndex < buttonCount; buttonIndex++)
            {
                ILocator button = buttons.Nth(buttonIndex);
                string text = (await button.InnerTextAsync()).ReplaceLineEndings(" ").Trim();
                bool hasPrice = text.Any(char.IsDigit) &&
                    (text.Contains('$') || text.Contains('\u00A3') || text.Contains('\u20AC'));
                if (!hasPrice ||
                    !await button.IsVisibleAsync() ||
                    !await button.IsEnabledAsync())
                {
                    continue;
                }

                await button.ScrollIntoViewIfNeededAsync();
                await button.ClickAsync(new LocatorClickOptions { Timeout = 15_000 });
                return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Unknown fare";
            }
        }

        string summary = await GetSearchSummaryAsync();
        string fareDiagnostics = await GetFareButtonDiagnosticsAsync();
        throw new InvalidOperationException(
            $"No enabled priced fare button was found{(carrierCode is null ? string.Empty : $" for carrier '{carrierCode}'")}. " +
            $"{summary} fareButtons={fareDiagnostics}");
    }

    private async Task<string> GetFareButtonDiagnosticsAsync()
    {
        return await Page.EvaluateAsync<string>(
            @"() => JSON.stringify([...document.querySelectorAll('[data-testid=""flight-card""]')]
                .map((card, cardIndex) => ({
                    cardIndex: cardIndex + 1,
                    carriers: [...card.querySelectorAll('[data-testid^=""carrier-logo-component-carriers-icon-""]')]
                        .map(element => element.getAttribute('data-testid')),
                    text: (card.innerText || '').replace(/\s+/g, ' ').trim().slice(0, 500),
                    buttons: [...card.querySelectorAll('button')].map((button) => {
                        const rect = button.getBoundingClientRect();
                        const style = window.getComputedStyle(button);
                        return {
                            innerText: (button.innerText || '').replace(/\s+/g, ' ').trim(),
                            textContent: (button.textContent || '').replace(/\s+/g, ' ').trim(),
                            disabled: button.disabled,
                            ariaDisabled: button.getAttribute('aria-disabled'),
                            pointerEvents: style.pointerEvents,
                            rect: [Math.round(rect.width), Math.round(rect.height)]
                        };
                    })
                })))");
    }

    private async Task<string> ClickFirstEnabledPartnerFareAsync()
    {
        string script = @"() => {
                document.querySelectorAll('[data-eapi-playwright-target=""fare-pick""]')
                    .forEach(element => element.removeAttribute('data-eapi-playwright-target'));
                const visible = element => {
                    const rect = element.getBoundingClientRect();
                    const style = getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden' &&
                        style.pointerEvents !== 'none';
                };
                const partnerName = /delta|air france|klm|aeromexico|korean air|china airlines|middle east airlines|saudia|xiamen|ita airways|westjet/i;
                const cards = [...document.querySelectorAll('[data-testid=""flight-card""]')];
                for (let index = 0; index < cards.length; index++) {
                    const card = cards[index];
                    const carrierIds = [...card.querySelectorAll('[data-testid^=""carrier-logo-component-carriers-icon-""]')]
                        .map(element => (element.getAttribute('data-testid') || '').split('-').pop()?.toLowerCase())
                        .filter(Boolean);
                    const cardText = (card.innerText || card.textContent || '').replace(/\s+/g, ' ').trim();
                    const hasPartnerCarrier = carrierIds.some(code => code !== 'vs') || partnerName.test(cardText);
                    if (!hasPartnerCarrier) continue;

                    const button = [...card.querySelectorAll('button')].find(candidate => {
                        const text = (candidate.innerText || candidate.textContent || '').trim();
                        return visible(candidate) && !candidate.disabled &&
                            candidate.getAttribute('aria-disabled') !== 'true' &&
                            /\d/.test(text) && /[$£€]/.test(text);
                    });
                    if (button) {
                        button.setAttribute('data-eapi-playwright-target', 'fare-pick');
                        return `card ${index + 1}; carriers ${carrierIds.join(',') || 'text-detected'}; ${(button.innerText || '').split('\n')[0].trim()}`;
                    }
                }
                return `ERROR: no partner-operated priced fare. ${JSON.stringify(cards.map((card, index) => ({
                    card: index + 1,
                    carriers: [...card.querySelectorAll('[data-testid^=""carrier-logo-component-carriers-icon-""]')]
                        .map(element => element.getAttribute('data-testid')),
                    text: (card.innerText || '').replace(/\s+/g, ' ').trim().slice(0, 500)
                })))}`;
            }";

        DateTime deadline = DateTime.UtcNow.AddSeconds(60);
        string? result = null;
        HashSet<string> inspectedCards = new(StringComparer.Ordinal);
        do
        {
            result = await Page.EvaluateAsync<string>(script);
            if (!result.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    await Page.Locator(FarePickMarkerSelector)
                        .ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
                    return result;
                }
                catch (PlaywrightException)
                {
                    // Results can remount while partner fare content settles; reacquire the card and fare.
                }
            }

            string batchFingerprint = await Page.EvaluateAsync<string>(
                @"() => JSON.stringify([...document.querySelectorAll('[data-testid=""flight-card""]')]
                    .map(card => (card.innerText || card.textContent || '').replace(/\s+/g, ' ').trim().slice(0, 300)))");
            inspectedCards.Add(batchFingerprint);
            await Page.EvaluateAsync(
                @"() => {
                    const cards = [...document.querySelectorAll('[data-testid=""flight-card""]')];
                    const lastCard = cards[cards.length - 1];
                    if (lastCard) {
                        lastCard.scrollIntoView({ block: 'end', inline: 'nearest' });
                        window.scrollBy(0, Math.max(window.innerHeight * 0.75, 500));
                    } else {
                        window.scrollBy(0, Math.max(window.innerHeight * 0.8, 600));
                    }
                }");
            await Task.Delay(1_000);
        }
        while (DateTime.UtcNow < deadline);

        throw new InvalidOperationException(
            $"{result ?? "No partner-operated fare could be identified."} " +
            $"Inspected {inspectedCards.Count} rendered result batches while scrolling.");
    }

    private async Task<string> ClickRequestedFareAsync(
        string fareName,
        string carrierCode,
        string? fallbackFareName = null)
    {
        string script = @"() => {
                const [flightCardSelector, fare, carrier, fallbackFare] = __ARGS__;
                document.querySelectorAll('[data-eapi-playwright-target=""fare-pick""]')
                    .forEach((element) => element.removeAttribute('data-eapi-playwright-target'));

                const normalise = (value) => (value || '').replace(/\s+/g, ' ').trim().toLowerCase();
                const wantedFare = normalise(fare);
                const wantedFares = [wantedFare, normalise(fallbackFare)].filter(Boolean);
                const carrierTestId = `carrier-logo-component-carriers-icon-${normalise(carrier)}`;
                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden' &&
                        style.pointerEvents !== 'none';
                };

                const cards = [...document.querySelectorAll(flightCardSelector)];
                for (let cardIndex = 0; cardIndex < cards.length; cardIndex++) {
                    const card = cards[cardIndex];
                    if (!card.querySelector(`[data-testid=""${carrierTestId}""]`)) continue;

                    const button = [...card.querySelectorAll('button')].find((candidate) => {
                        const firstLine = normalise((candidate.innerText || '').split('\n')[0]).replace(/ from$/i, '');
                        return wantedFares.includes(firstLine) && isVisible(candidate) &&
                            !candidate.disabled && candidate.getAttribute('aria-disabled') !== 'true';
                    });
                    if (button) {
                        button.setAttribute('data-eapi-playwright-target', 'fare-pick');
                        return `card ${cardIndex + 1}: ${(button.innerText || '').split('\n')[0].trim()}`;
                    }
                }

                const diagnostics = cards.slice(0, 8).map((card, cardIndex) => ({
                    cardIndex: cardIndex + 1,
                    carriers: [...card.querySelectorAll('[data-testid^=""carrier-logo-component-carriers-icon-""]')]
                        .map((element) => element.getAttribute('data-testid')),
                    fares: [...card.querySelectorAll('button')]
                        .map((button) => (button.innerText || '').split('\n')[0].trim())
                        .filter(Boolean)
                }));
                return `ERROR: no ${fare} fare on ${carrier.toUpperCase()} card. ${JSON.stringify(diagnostics)}`;
            }".Replace(
                "__ARGS__",
                System.Text.Json.JsonSerializer.Serialize(new[] { FlightCardSelector, fareName, carrierCode, fallbackFareName ?? string.Empty }),
                StringComparison.Ordinal);
        DateTime deadline = DateTime.UtcNow.AddSeconds(30);
        string? lastResult = null;
        do
        {
            lastResult = await Page.EvaluateAsync<string?>(script);
            if (!lastResult.IsNullOrWhiteSpace() &&
                !lastResult.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    ILocator fareButton = Page.Locator(FarePickMarkerSelector);
                    await fareButton.ScrollIntoViewIfNeededAsync();
                    await fareButton.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
                    return lastResult;
                }
                catch (PlaywrightException)
                {
                    // React can remount the inbound cards between targeting and clicking; reacquire the target.
                }
            }

            if (Page.Url.Contains("/flights/search/summary", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            await Task.Delay(500);
        }
        while (DateTime.UtcNow < deadline);

        string diagnostics = await GetLoadDiagnosticsAsync();
        throw new InvalidOperationException(
            $"{lastResult ?? "Requested carrier fare could not be targeted."} {diagnostics}");
    }

    private async Task<string> ClickPremiumFareWithLargestUpgradeDeltaAsync()
    {
        string script = @"() => {
                document.querySelectorAll('[data-eapi-playwright-target=""fare-pick""]')
                    .forEach(element => element.removeAttribute('data-eapi-playwright-target'));
                const normalise = value => String(value || '').replace(/\s+/g, ' ').trim();
                const points = button => {
                    const value = button?.querySelector('[data-cy=""points-component""]')?.textContent || '';
                    const parsed = Number(value.replace(/[^0-9.]/g, ''));
                    return Number.isFinite(parsed) ? parsed : null;
                };
                const fareButton = (card, fare) => [...card.querySelectorAll('button[data-cy=""flight-fare-component-button""]')]
                    .find(button => normalise(button.querySelector('[data-cy=""fare-label""]')?.textContent)
                        .toLowerCase() === fare.toLowerCase());
                const candidates = [...document.querySelectorAll('[data-testid=""flight-card""]')]
                    .map((card, cardIndex) => {
                        if (!card.querySelector('[data-testid=""carrier-logo-component-carriers-icon-vs""]')) return null;
                        const economy = fareButton(card, 'Economy Classic');
                        const premium = fareButton(card, 'Premium');
                        const economyPoints = points(economy);
                        const premiumPoints = points(premium);
                        if (!economy || !premium || economyPoints === null || premiumPoints === null ||
                            premium.disabled || premium.getAttribute('aria-disabled') === 'true') return null;
                        return { cardIndex, premium, economyPoints, premiumPoints,
                            delta: premiumPoints - economyPoints };
                    })
                    .filter(candidate => candidate && candidate.delta > 0)
                    .sort((left, right) => right.delta - left.delta || left.cardIndex - right.cardIndex);
                const picked = candidates[0];
                if (!picked) {
                    return 'ERROR: no VS card exposed enabled Economy Classic and Premium reward points with a positive upgrade delta';
                }
                picked.premium.setAttribute('data-eapi-playwright-target', 'fare-pick');
                return `card ${picked.cardIndex + 1}: Economy ${picked.economyPoints}; Premium ${picked.premiumPoints}; delta ${picked.delta}`;
            }";

        DateTime deadline = DateTime.UtcNow.AddSeconds(30);
        string? result = null;
        do
        {
            result = await Page.EvaluateAsync<string>(script);
            if (!result.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    ILocator fareButton = Page.Locator(FarePickMarkerSelector);
                    await fareButton.ScrollIntoViewIfNeededAsync();
                    await fareButton.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
                    return result;
                }
                catch (PlaywrightException)
                {
                    // Results can remount while fares settle; recalculate against the current cards.
                }
            }

            await Task.Delay(500);
        }
        while (DateTime.UtcNow < deadline);

        throw new InvalidOperationException(result ?? "No maximum-delta Premium fare could be selected.");
    }

    private async Task WaitForFareDrawerAsync(int timeoutMs)
    {
        await Page.WaitForFunctionAsync(
            @"() => {
                const drawer = document.querySelector('[data-testid=""drawer""]') || document;

                if (drawer !== document) {
                    const drawerRect = drawer.getBoundingClientRect();
                    if (drawerRect.width <= 0 || drawerRect.height <= 0) {
                        return false;
                    }
                }

                const articles = [...drawer.querySelectorAll('article')].filter((article) =>
                    article.closest('[data-testid=""drawer""]') ||
                    article.closest('[data-swiper-slide-index]'));
                if (articles.length === 0) {
                    return false;
                }
                const hasArticle = articles.some((article) => {
                    const rect = article.getBoundingClientRect();
                    return rect.width > 0 && rect.height > 0;
                });

                const hasEnabledSelect = [...drawer.querySelectorAll('button')].some((button) => {
                    const text = `${button.innerText || button.textContent || ''} ${button.getAttribute('aria-label') || ''}`
                        .replace(/\s+/g, ' ').trim().toLowerCase();
                    const style = window.getComputedStyle(button);
                    const rect = button.getBoundingClientRect();
                    return /^select(?:\s|$)/.test(text) &&
                        !button.disabled &&
                        button.getAttribute('aria-disabled') !== 'true' &&
                        style.pointerEvents !== 'none' &&
                        rect.width > 0 &&
                        rect.height > 0;
                });

                return hasArticle && hasEnabledSelect;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }

    private async Task<bool> WaitForRequestedFareDrawerAsync(string fareName, int timeoutMs)
    {
        string script = @"() => {
                const [drawerSelector, fare] = __ARGS__;
                const drawer = document.querySelector(drawerSelector) || document;
                const wanted = (fare || '').replace(/\s+/g, ' ').trim().toLowerCase();
                if (!wanted) return false;
                const cabinByIndex = ['economy classic', 'premium', 'upper class'];
                return [...drawer.querySelectorAll('article')].some((article) => {
                    const imageAlt = (article.querySelector('img[alt$=""cabin photo""]')?.getAttribute('alt') || '')
                        .replace(/\s+/g, ' ').trim().toLowerCase();
                    const heading = [...article.querySelectorAll('h1, h2, h3, h4, h5, h6')]
                        .map((element) => (element.innerText || '').replace(/\s+/g, ' ').trim().toLowerCase())
                        .find(Boolean) || '';
                    const slideIndex = Number(article.closest('[data-swiper-slide-index]')?.getAttribute('data-swiper-slide-index'));
                    const inferredCabin = Number.isInteger(slideIndex) ? cabinByIndex[slideIndex] || '' : '';
                    return imageAlt.startsWith(wanted) || heading === wanted || inferredCabin === wanted;
                });
            }".Replace(
                "__ARGS__",
                System.Text.Json.JsonSerializer.Serialize(new[] { DrawerSelector, fareName }),
                StringComparison.Ordinal);
        try
        {
            await Page.WaitForFunctionAsync(
                script,
                null,
                new PageWaitForFunctionOptions { Timeout = timeoutMs });
            return true;
        }
        catch (TimeoutException)
        {
            // The third-party fare-content response can be absent even though the selected
            // cabin drawer is usable. The caller may safely choose its first enabled option.
            return false;
        }
    }

    private async Task<string> ClickFirstEnabledDrawerSelectAsync(string preferredFare, bool requireExactFare = false)
    {
        string script = @"() => {
                const [drawerSelector, preferredFare, requireExactFare] = __ARGS__;
                const drawer = document.querySelector(drawerSelector) || document;

                drawer
                    .querySelectorAll('[data-eapi-playwright-target=""drawer-select""]')
                    .forEach((element) => element.removeAttribute('data-eapi-playwright-target'));

                const normalise = (value) => (value || '').replace(/\s+/g, ' ').trim().toLowerCase();
                const wanted = normalise(preferredFare);
                const cabinByIndex = ['economy classic', 'premium', 'upper class'];
                const isClickableSelect = (button) => {
                    const text = normalise(`${button.innerText || button.textContent || ''} ${button.getAttribute('aria-label') || ''}`);
                    const style = window.getComputedStyle(button);
                    const rect = button.getBoundingClientRect();
                    return /^select(?:\s|$)/.test(text) &&
                        !button.disabled &&
                        button.getAttribute('aria-disabled') !== 'true' &&
                        style.pointerEvents !== 'none' &&
                        rect.width > 0 &&
                        rect.height > 0;
                };

                const candidates = [...drawer.querySelectorAll('button')]
                    .filter(isClickableSelect)
                    .map((selectButton, index) => {
                        let container = selectButton.parentElement;
                        let distance = 0;
                        let exactContainer = null;
                        let exactLabel = false;
                        let inferredCabin = '';
                        while (container && container !== drawer.parentElement) {
                            const labels = [
                                ...container.querySelectorAll('h1, h2, h3, h4, h5, h6'),
                                ...container.querySelectorAll('img[alt$=""cabin photo""]')
                            ].map((element) => normalise(
                                element.tagName === 'IMG'
                                    ? (element.getAttribute('alt') || '').replace(/ cabin photo$/i, '')
                                    : element.innerText));
                            if (wanted && labels.includes(wanted)) {
                                exactContainer = container;
                                exactLabel = true;
                                break;
                            }
                            const containerText = normalise(container.innerText || '');
                            if (wanted && containerText.includes(wanted)) {
                                // Inline fare tiles use styled divs rather than semantic headings.
                                // The requested tile encounters its own short local container well
                                // before another tile only encounters the shared flight article.
                                exactContainer = container;
                                break;
                            }
                            container = container.parentElement;
                            distance++;
                        }

                        const article = selectButton.closest('article');
                        const slideIndex = Number(selectButton.closest('[data-swiper-slide-index]')?.getAttribute('data-swiper-slide-index'));
                        inferredCabin = Number.isInteger(slideIndex) ? cabinByIndex[slideIndex] || '' : '';
                        const localContainer = exactContainer || article || selectButton.parentElement;
                        const localText = normalise(localContainer?.innerText || '');
                        const fareName = exactContainer ? wanted : inferredCabin || localText;
                        const priority = exactContainer
                            ? distance * 10000 + (exactLabel ? 0 : localText.length)
                            : wanted && inferredCabin === wanted
                                ? 100
                                : wanted && localText.includes(wanted)
                                    ? 200
                                    : selectButton.closest('.swiper-slide-active')
                                        ? 300
                                        : 400;

                        return { article: localContainer, index, priority, fareName, selectButton, exact: Boolean(exactContainer) || inferredCabin === wanted };
                    })
                    .sort((left, right) => left.priority - right.priority || left.index - right.index);

                const picked = candidates[0];
                if (!picked) {
                    return 'ERROR: no enabled Select button in any drawer article';
                }

                if (requireExactFare && !picked.exact) {
                    return `ERROR: requested fare '${preferredFare}' was not available in the drawer; closest target was '${picked.fareName || '<unnamed>'}'`;
                }

                picked.selectButton.setAttribute('data-eapi-playwright-target', 'drawer-select');
                // Activate the same current DOM node that was validated. Safari can remount the
                // fare carousel between returning from this script and a separate locator click.
                picked.selectButton.click();
                return picked.fareName || normalise(picked.article.innerText || '') || 'selected drawer fare';
            }".Replace(
                "__ARGS__",
                System.Text.Json.JsonSerializer.Serialize(new object[] { DrawerSelector, preferredFare, requireExactFare }),
                StringComparison.Ordinal);
        DateTime deadline = DateTime.UtcNow.AddSeconds(30);
        string? markerResult = null;
        do
        {
            markerResult = await Page.EvaluateAsync<string?>(script);
            if (!markerResult.IsNullOrWhiteSpace() &&
                !markerResult.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
            {
                return markerResult;
            }

            await Task.Delay(500);
        }
        while (DateTime.UtcNow < deadline);

        throw new InvalidOperationException(
            $"Fare options opened but no stable Select button could be targeted: {markerResult}.");
    }

    private async Task DismissOptionalModalButtonAsync(string buttonText, int timeoutMs, int settleBeforeClickMs = 0)
    {
        try
        {
            string waitScript = @"() => {
                const text = __TEXT__;
                return [...document.querySelectorAll('[role=""dialog""] button, [aria-modal=""true""] button')]
                    .some((button) => `${button.innerText || ''} ${button.getAttribute('aria-label') || ''}`.trim().includes(text));
            }"
                .Replace("__TEXT__", System.Text.Json.JsonSerializer.Serialize(buttonText), StringComparison.Ordinal);
            await Page.WaitForFunctionAsync(
                waitScript,
                null,
                new PageWaitForFunctionOptions { Timeout = timeoutMs });
        }
        catch (TimeoutException)
        {
            return;
        }

        if (settleBeforeClickMs > 0)
        {
            await Task.Delay(settleBeforeClickMs);
        }

        string clickScript = @"() => {
                const text = __TEXT__;
                const buttons = [...document.querySelectorAll('[role=""dialog""] button, [aria-modal=""true""] button')];
                const button = buttons.find((candidate) =>
                    `${candidate.innerText || ''} ${candidate.getAttribute('aria-label') || ''}`.trim().includes(text));
                if (button) {
                    button.scrollIntoView({ block: 'center', inline: 'center' });
                    button.click();
                }
            }".Replace("__TEXT__", System.Text.Json.JsonSerializer.Serialize(buttonText), StringComparison.Ordinal);
        await Page.EvaluateAsync(clickScript);
    }

    private async Task<string> GetResultsFingerprintAsync()
    {
        return await Page.EvaluateAsync<string>(
            @"() => {
                const normalise = value => String(value || '').replace(/\s+/g, ' ').trim();
                const cards = [...document.querySelectorAll('[data-testid=""flight-card""]')]
                    .slice(0, 8)
                    .map(card => normalise(card.innerText || card.textContent || '').slice(0, 800));
                return JSON.stringify(cards);
            }");
    }

    private async Task WaitForSelectionProgressAsync(
        string previousUrl,
        string previousResultsFingerprint,
        int timeoutMs)
    {
        DateTime deadline = DateTime.UtcNow.AddMilliseconds(Math.Min(timeoutMs, 45_000));
        SelectionProgressState? lastState = null;
        string? replacementCandidate = null;
        int stableReplacementSamples = 0;
        while (DateTime.UtcNow < deadline)
        {
            bool dismissedModal = await DismissBlockingSelectionModalIfPresentAsync();
            if (dismissedModal)
            {
                await Task.Delay(300);
            }

            lastState = await Page.EvaluateAsync<SelectionProgressState>(
                @"() => {
                    const isVisible = element => {
                        const rect = element.getBoundingClientRect();
                        const style = window.getComputedStyle(element);
                        return rect.width > 0 && rect.height > 0 &&
                            style.display !== 'none' && style.visibility !== 'hidden' &&
                            Number(style.opacity || 1) > 0;
                    };
                    const normalise = value => String(value || '').replace(/\s+/g, ' ').trim();
                    const cards = [...document.querySelectorAll('[data-testid=""flight-card""]')]
                        .slice(0, 8)
                        .map(card => normalise(card.innerText || card.textContent || '').slice(0, 800));
                    const loadersVisible = [...document.querySelectorAll(
                        '[data-testid=""search-loader""], [data-testid=""tailfin-loader-component""], [data-testid=""interstitial-component""]'
                    )].some(isVisible);
                    const drawerVisible = [...document.querySelectorAll('[data-testid=""drawer""]')].some(isVisible);
                    return {
                        Url: window.location.href,
                        ResultsFingerprint: JSON.stringify(cards),
                        ReadyResults: cards.length > 0 && !loadersVisible && !drawerVisible,
                        SummaryRendered: [...document.querySelectorAll('h1, h2, h3')]
                            .some(element => isVisible(element) &&
                                /^Flight summary$/i.test((element.innerText || '').trim())),
                        VisibleDrawer: drawerVisible,
                        VisibleLoaders: loadersVisible,
                        BodySample: normalise(document.body?.innerText || '').slice(0, 800)
                    };
                }");

            bool summaryReached = lastState.SummaryRendered || lastState.Url.Contains(
                "/flights/search/summary",
                StringComparison.OrdinalIgnoreCase);
            bool urlChanged = !lastState.Url.Equals(previousUrl, StringComparison.Ordinal);
            bool resultsReplacedInPlace = lastState.ReadyResults &&
                !lastState.ResultsFingerprint.Equals(previousResultsFingerprint, StringComparison.Ordinal);
            if (summaryReached || urlChanged)
            {
                return;
            }

            if (resultsReplacedInPlace)
            {
                if (lastState.ResultsFingerprint.Equals(replacementCandidate, StringComparison.Ordinal))
                {
                    stableReplacementSamples++;
                }
                else
                {
                    replacementCandidate = lastState.ResultsFingerprint;
                    stableReplacementSamples = 1;
                }

                if (stableReplacementSamples >= 3)
                {
                    return;
                }
            }
            else
            {
                replacementCandidate = null;
                stableReplacementSamples = 0;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException(
            $"Flight selection produced no URL or results-card progress from '{previousUrl}'. " +
            $"Current URL: '{lastState?.Url ?? Page.Url}'; drawerVisible={lastState?.VisibleDrawer}; " +
            $"loadersVisible={lastState?.VisibleLoaders}; readyResults={lastState?.ReadyResults}; " +
            $"body={lastState?.BodySample}");
    }

    private async Task<bool> DismissBlockingSelectionModalIfPresentAsync()
    {
        return await Page.EvaluateAsync<bool>(
            @"() => {
                const isVisible = element => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden' &&
                        style.pointerEvents !== 'none';
                };
                const button = [...document.querySelectorAll('[role=""dialog""] button, [aria-modal=""true""] button')]
                    .find(candidate => {
                        const text = `${candidate.innerText || ''} ${candidate.getAttribute('aria-label') || ''}`
                            .replace(/\s+/g, ' ')
                            .trim();
                        return isVisible(candidate) &&
                            /Continue with standard|Select cabin/i.test(text) &&
                            !candidate.disabled && candidate.getAttribute('aria-disabled') !== 'true';
                    });
                if (!button) return false;
                button.scrollIntoView({ block: 'center', inline: 'center' });
                button.click();
                return true;
            }");
    }
}

internal sealed class SelectionProgressState
{
    public string Url { get; set; } = string.Empty;

    public string ResultsFingerprint { get; set; } = string.Empty;

    public bool ReadyResults { get; set; }

    public bool SummaryRendered { get; set; }

    public bool VisibleDrawer { get; set; }

    public bool VisibleLoaders { get; set; }

    public string BodySample { get; set; } = string.Empty;
}
