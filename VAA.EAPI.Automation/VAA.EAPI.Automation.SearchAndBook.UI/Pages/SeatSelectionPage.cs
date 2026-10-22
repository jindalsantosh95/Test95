using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Extensions;
namespace VAA.EAPI.Automation.SearchAndBook.UI.Pages;

public sealed record SeatSelectionResult(
    IReadOnlyList<string> SelectedSeats,
    IReadOnlyList<string> SkippedReasons);

public sealed class SeatSelectionPage : BasePage
{
    private const string LoaderSelector = "[data-testid=\"search-loader\"], [data-testid=\"tailfin-loader-component\"], [data-testid=\"interstitial-component\"]";
    private const string AdvanceMarkerSelector = "[data-eapi-playwright-target=\"seat-advance\"]";
    public SeatSelectionPage(IPage page)
        : base(page)
    {
    }

    public async Task SkipSeatSelectionIfShownAsync(int timeoutMs)
    {
        for (int leg = 0; leg < 2; leg++)
        {
            try
            {
                await WaitForSeatOrPaymentStateAsync(timeoutMs);
            }
            catch (TimeoutException exception)
            {
                string summary = await GetPageSummaryAsync();
                throw new InvalidOperationException($"Seat selection did not become actionable before timeout. {summary}", exception);
            }
            catch (PlaywrightException exception) when (exception.Message.Contains("Timeout", StringComparison.OrdinalIgnoreCase))
            {
                string summary = await GetPageSummaryAsync();
                throw new InvalidOperationException($"Seat selection did not become actionable before timeout. {summary}", exception);
            }

            if (await IsPaymentPageAsync())
            {
                return;
            }

            await AdvanceToNextFlightOrPaymentAsync(timeoutMs, expectPayment: leg == 1);
        }

        await WaitForPaymentPageAsync(timeoutMs);
    }

    public async Task<SeatSelectionResult> SelectSeatsAndContinueToPaymentAsync(bool requirePaidSeat, int timeoutMs)
    {
        List<string> selectedSeats = [];
        List<string> skippedReasons = [];
        for (int leg = 0; leg < 2; leg++)
        {
            await WaitForSeatOrPaymentStateAsync(timeoutMs);
            if (await IsPaymentPageAsync())
            {
                return new SeatSelectionResult(selectedSeats, skippedReasons);
            }

            if (await IsSeatMapUnavailableAsync())
            {
                skippedReasons.Add($"Flight {leg + 1}: seat map unavailable");
                await AdvanceToNextFlightOrPaymentAsync(timeoutMs, expectPayment: leg == 1);
                continue;
            }

            // Keep the requested seat type consistent across rendered legs. If that inventory
            // is unavailable, record the skipped leg and continue the booking as required.
            bool paidSeatRequiredForCurrentLeg = requirePaidSeat;
            string? selectedSeat = await SelectFirstAvailableSeatAsync(paidSeatRequiredForCurrentLeg);
            if (selectedSeat is not null)
            {
                await AcceptExitRowSelectionIfPresentAsync();
                selectedSeats.Add(selectedSeat);
            }
            else
            {
                skippedReasons.Add(
                    paidSeatRequiredForCurrentLeg
                        ? $"Flight {leg + 1}: no paid seat available"
                        : $"Flight {leg + 1}: no selectable seat available");
            }

            await AdvanceToNextFlightOrPaymentAsync(timeoutMs, expectPayment: leg == 1);
        }

        await WaitForPaymentPageAsync(timeoutMs);
        return new SeatSelectionResult(selectedSeats, skippedReasons);
    }

    public async Task WaitForPaymentPageAsync(int timeoutMs)
    {
        await Page.WaitForFunctionAsync(
            @"() => {
                const bodyText = (document.body && document.body.innerText || '').trim();
                return window.location.href.includes('/payment') ||
                    /How would you like to pay|Payment options/i.test(bodyText);
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }

    public async Task<string> GetPageSummaryAsync()
    {
        return await Page.EvaluateAsync<string>(
            @"() => {
                const seatMaps = document.querySelectorAll('[data-testid*=""seat-map"" i], [class*=""seat-map"" i]').length;
                const advanceActions = document.querySelectorAll('[data-eapi-playwright-target=""seat-advance""]').length;
                const paymentFrames = document.querySelectorAll(""iframe[name='paymentFrame']"").length;
                return `URL: ${location.origin}${location.pathname}; seatMaps: ${seatMaps}; ` +
                    `advanceActions: ${advanceActions}; paymentFrames: ${paymentFrames}`;
            }");
    }

    private async Task WaitForSeatOrPaymentStateAsync(int timeoutMs)
    {
        await Page.WaitForFunctionAsync(
            @"([loaderSelector]) => {
                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 &&
                        rect.height > 0 &&
                        style.display !== 'none' &&
                        style.visibility !== 'hidden';
                };

                const url = window.location.href;
                const bodyText = (document.body && document.body.innerText || '').trim();
                const onPayment = url.includes('/payment') || /How would you like to pay|Payment options/i.test(bodyText);
                const onSeatPage = url.includes('/seat') || /seat selection|choose your seat|select seats|skip to payment|seat map for this flight isn['’]t available/i.test(bodyText);
                const hasVisibleSeatAction = [...document.querySelectorAll('button, a')]
                    .some((element) => {
                        const text = (element.innerText || element.textContent || '').replace(/\s+/g, ' ').trim();
                        return isVisible(element) &&
                            /Skip to payment|Skip seats?|Continue to payment|Confirm and pay|Next flight|Continue/i.test(text) &&
                            !element.disabled &&
                            element.getAttribute('aria-disabled') !== 'true';
                    });
                const stillLoadingSeats = /Loading seats/i.test(bodyText);
                const loadersVisible = [...document.querySelectorAll(loaderSelector)].some(isVisible);
                return onPayment || (onSeatPage && hasVisibleSeatAction && !stillLoadingSeats && !loadersVisible);
            }",
            new[] { LoaderSelector },
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }

    private async Task<bool> IsSeatMapUnavailableAsync()
    {
        return await Page.EvaluateAsync<bool>(
            @"() => /seat map for this flight isn['’]t available|choose your seat once you['’]ve finished/i
                .test((document.body && document.body.innerText || '').trim())");
    }

    private async Task AdvanceToNextFlightOrPaymentAsync(int timeoutMs, bool expectPayment)
    {
        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        int RemainingTimeoutMs() => Math.Max(0, (int)(deadline - DateTime.UtcNow).TotalMilliseconds);

        await DismissSeatInformationOverlayAsync();

        string beforeUrl = Page.Url;
        string beforeBody = await Page.Locator("body").InnerTextAsync();
        const int attempts = 15;
        for (int attempt = 1; attempt <= attempts; attempt++)
        {
            int remainingTimeoutMs = RemainingTimeoutMs();
            if (remainingTimeoutMs <= 0)
            {
                break;
            }

            string? actionText = await MarkAdvanceButtonAsync();
            if (actionText.IsNullOrWhiteSpace())
            {
                if (attempt < attempts)
                {
                    // Safari can briefly remount the seat-map shell after readiness was observed.
                    // Reacquire the action rather than failing against that transient DOM.
                    await Task.Delay(Math.Min(1_000, remainingTimeoutMs));
                    continue;
                }

                throw new InvalidOperationException($"Seat selection page loaded, but no advance action could be targeted after {attempts} attempts. {await GetPageSummaryAsync()}");
            }

            ILocator action = Page.Locator(AdvanceMarkerSelector);
            try
            {
                await action.ScrollIntoViewIfNeededAsync();
                await action.ClickAsync(new LocatorClickOptions
                {
                    Timeout = Math.Min(15_000, remainingTimeoutMs)
                });
            }
            catch (Exception exception) when (exception is PlaywrightException or TimeoutException)
            {
                bool invokedCurrentAction = await Page.EvaluateAsync<bool>(
                    @"selector => {
                        const action = document.querySelector(selector);
                        if (!action || action.disabled || action.getAttribute('aria-disabled') === 'true') {
                            return false;
                        }
                        action.click();
                        return true;
                    }",
                    AdvanceMarkerSelector);
                if (!invokedCurrentAction)
                {
                    if (attempt < attempts)
                    {
                        await Task.Delay(500);
                        continue;
                    }
                    throw;
                }
            }
            try
            {
                int stateWaitTimeoutMs = RemainingTimeoutMs();
                if (stateWaitTimeoutMs <= 0)
                {
                    break;
                }

                await Page.WaitForFunctionAsync(
                    expectPayment
                        ? @"() => {
                            const bodyText = (document.body && document.body.innerText || '').trim();
                            return window.location.href.includes('/payment') ||
                                /How would you like to pay|Payment options/i.test(bodyText);
                        }"
                        : @"([previousUrl, previousBody]) => {
                            const bodyText = (document.body && document.body.innerText || '').trim();
                            return window.location.href !== previousUrl || bodyText !== previousBody;
                        }",
                    expectPayment ? null : new[] { beforeUrl, beforeBody },
                    new PageWaitForFunctionOptions
                    {
                        Timeout = Math.Min(stateWaitTimeoutMs, 30_000)
                    });
                return;
            }
            catch (TimeoutException) when (attempt < attempts)
            {
                int retryDelayMs = Math.Min(1_000, RemainingTimeoutMs());
                if (retryDelayMs > 0)
                {
                    await Task.Delay(retryDelayMs);
                }
            }
        }

        throw new TimeoutException(
            $"Final seat confirmation did not reach payment within {timeoutMs}ms or {attempts} attempts. " +
            await GetPageSummaryAsync());
    }

    private async Task DismissSeatInformationOverlayAsync()
    {
        const string blanketSelector = "[data-testid=\"blanket-component\"]";
        ILocator visibleBlanket = Page.Locator(blanketSelector).Last;
        if (!await visibleBlanket.IsVisibleAsync())
        {
            return;
        }

        await Page.Keyboard.PressAsync("Escape");
        try
        {
            await visibleBlanket.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = 1_000
            });
            return;
        }
        catch (TimeoutException)
        {
            // Some informational seat dialogs do not support Escape and must be closed explicitly.
        }

        bool closed = await Page.EvaluateAsync<bool>(
            @"blanketSelector => {
                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 && style.display !== 'none' &&
                        style.visibility !== 'hidden' && style.pointerEvents !== 'none';
                };
                const blanket = [...document.querySelectorAll(blanketSelector)].find(isVisible);
                const portal = blanket && blanket.closest('[class*=""portal""]');
                if (!portal) return false;
                const candidates = [...portal.querySelectorAll('button, [role=""button""]')]
                    .filter(isVisible)
                    .map((element) => {
                        const rect = element.getBoundingClientRect();
                        const label = `${element.getAttribute('aria-label') || ''} ${element.getAttribute('title') || ''} ${element.innerText || ''}`.trim();
                        return { element, rect, label };
                    });
                const closeButton = candidates.find(({ label }) => /close|dismiss/i.test(label)) ||
                    candidates
                        .filter(({ rect }) => rect.width <= 64 && rect.height <= 64)
                        .sort((left, right) => right.rect.top - left.rect.top || right.rect.right - left.rect.right)[0];
                if (!closeButton) return false;
                closeButton.element.click();
                return true;
            }",
            blanketSelector);

        if (!closed)
        {
            throw new InvalidOperationException($"A seat-information overlay blocked navigation and could not be dismissed. {await GetPageSummaryAsync()}");
        }

        await visibleBlanket.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = 5_000
        });
    }

    private async Task<bool> IsPaymentPageAsync()
    {
        return await Page.EvaluateAsync<bool>(
            @"() => {
                const bodyText = (document.body && document.body.innerText || '').trim();
                return window.location.href.includes('/payment') || /How would you like to pay|Payment options/i.test(bodyText);
            }");
    }

    private async Task<string?> SelectFirstAvailableSeatAsync(bool requirePaidSeat)
    {
        return await Page.EvaluateAsync<string?>(
            @"(paidOnly) => {
                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 && style.display !== 'none' &&
                        style.visibility !== 'hidden' && style.pointerEvents !== 'none';
                };
                const parsePrice = (value) => {
                    const match = (value || '').replace(/,/g, '').match(/[£$€]\s*(\d+(?:\.\d{1,2})?)/);
                    return match ? Number(match[1]) : 0;
                };
                const candidates = [...document.querySelectorAll('button[aria-label^=""Select seat ""]')]
                    .map((button) => {
                        const label = button.getAttribute('aria-label') || '';
                        return { button, label, price: parsePrice(`${button.innerText || ''} ${label}`) };
                    })
                    .filter((candidate) => isVisible(candidate.button) && !candidate.button.disabled &&
                        candidate.button.getAttribute('aria-disabled') !== 'true');
                // Prefer an included seat unless the journey explicitly requires a paid seat.
                const picked = paidOnly
                    ? candidates.find((candidate) => candidate.price > 0)
                    : candidates.find((candidate) => candidate.price === 0);
                if (!picked) return null;
                const seatMatch = picked.label.match(/Select seat\s+(\d+[A-Z])\b/i);
                picked.button.scrollIntoView({ block: 'center', inline: 'center' });
                picked.button.click();
                return `${seatMatch ? seatMatch[1].toUpperCase() : picked.label} (${picked.price.toFixed(2)})`;
            }",
            requirePaidSeat);
    }

    private async Task AcceptExitRowSelectionIfPresentAsync()
    {
        try
        {
            await Page.WaitForFunctionAsync(
                @"() => [...document.querySelectorAll('button')].some((button) => {
                    const text = (button.innerText || button.textContent || '').replace(/\s+/g, ' ').trim();
                    const rect = button.getBoundingClientRect();
                    return /Accept and select seat/i.test(text) &&
                        rect.width > 0 && rect.height > 0 &&
                        !button.disabled && button.getAttribute('aria-disabled') !== 'true';
                })",
                null,
                new PageWaitForFunctionOptions { Timeout = 3_000 });
        }
        catch (TimeoutException)
        {
            return;
        }

        // Safari can remount the confirmation dialog between locating the action and
        // Playwright clicking it. Locate and click in the same browser evaluation so
        // the selected DOM node cannot become stale between those operations.
        try
        {
            await Page.WaitForFunctionAsync(
                @"() => {
                    const button = [...document.querySelectorAll('button')].find((candidate) => {
                        const text = (candidate.innerText || candidate.textContent || '').replace(/\s+/g, ' ').trim();
                        const rect = candidate.getBoundingClientRect();
                        return /Accept and select seat/i.test(text) &&
                            rect.width > 0 && rect.height > 0 &&
                            !candidate.disabled && candidate.getAttribute('aria-disabled') !== 'true';
                    });
                    if (!button) return false;
                    button.scrollIntoView({ block: 'center', inline: 'center' });
                    button.click();
                    return true;
                }",
                null,
                new PageWaitForFunctionOptions { Timeout = 15_000 });
        }
        catch (Exception exception) when (
            exception is TimeoutException ||
            exception is PlaywrightException playwrightException &&
            playwrightException.Message.Contains("Timeout", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Exit-row seat confirmation appeared but its accept action could not be targeted.",
                exception);
        }

        await Page.WaitForTimeoutAsync(500);
    }

    private async Task<string?> MarkAdvanceButtonAsync()
    {
        return await Page.EvaluateAsync<string?>(
            @"() => {
                document
                    .querySelectorAll('[data-eapi-playwright-target=""seat-advance""]')
                    .forEach((element) => element.removeAttribute('data-eapi-playwright-target'));
                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 && style.display !== 'none' &&
                        style.visibility !== 'hidden' && style.pointerEvents !== 'none';
                };
                const picked = [...document.querySelectorAll('button, a, [role=""button""]')]
                    .map((element, index) => ({ element, index, text: (element.innerText || element.textContent || '').replace(/\s+/g, ' ').trim() }))
                    .filter((candidate) => isVisible(candidate.element) &&
                        /Skip to payment|Skip seats?|Confirm and pay|Continue to next flight|Next flight|Continue/i.test(candidate.text) &&
                        !candidate.element.disabled && candidate.element.getAttribute('aria-disabled') !== 'true')
                    .sort((left, right) => {
                        const leftScore = /Confirm and pay/i.test(left.text) ? -10 : 0;
                        const rightScore = /Confirm and pay/i.test(right.text) ? -10 : 0;
                        return leftScore - rightScore || right.index - left.index;
                    })[0];
                if (!picked) return null;
                picked.element.scrollIntoView({ block: 'start', inline: 'center' });
                window.scrollBy(0, -120);
                picked.element.setAttribute('data-eapi-playwright-target', 'seat-advance');
                return picked.text;
            }");
    }
}



