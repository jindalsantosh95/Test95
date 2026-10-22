using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Pages;

public sealed class PassengerDetailsPage : BasePage
{
    private const int FormHydrationDelayMs = 5_000;
    private const int ReactInputSettleDelayMs = 750;
    private const string LoaderSelector = "[data-testid=\"search-loader\"], [data-testid=\"tailfin-loader-component\"], [data-testid=\"interstitial-component\"]";
    private const string SubmitMarkerSelector = "[data-eapi-playwright-target=\"pax-submit\"]";
    private const string SeatChoiceModalActionMarkerSelector = "[data-eapi-playwright-target=\"seat-choice-modal-action\"]";
    private int _expectedPassengerCount;

    public PassengerDetailsPage(IPage page)
        : base(page)
    {
    }

    public async Task WaitUntilReadyAsync(int timeoutMs)
    {
        await VirginAtlanticConsent.DismissPrivacyBannerIfPresentAsync(Page);
        await Page.WaitForFunctionAsync(
            @"() => {
                const isPassengerUrl = window.location.href.includes('/passengers');
                const bodyText = (document.body && document.body.innerText || '').trim();
                const hasPassengerContent = /Passenger details|Who's flying|Who is flying|Your details|Lead passenger/i.test(bodyText);
                const hasFirstName = Boolean(document.querySelector('input[name=""firstName""]'));
                return isPassengerUrl && hasPassengerContent && hasFirstName;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
        await VirginAtlanticConsent.DismissPrivacyBannerIfPresentAsync(Page);
        await Page.Locator("input[name=\"firstName\"]:visible").First.WaitForAsync(
            new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
    }

    public async Task FillDefaultAdultGuestAsync(int timeoutMs)
    {
        await WaitUntilReadyAsync(timeoutMs);
        await VirginAtlanticConsent.DismissPrivacyBannerIfPresentAsync(Page);
        await WaitForPassengerFormToSettleAsync(5_000);
        await Task.Delay(FormHydrationDelayMs);

        string? applicationErrorText = await GetApplicationErrorTextAsync();
        if (!applicationErrorText.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(
                $"Passenger details could not be entered because the application displayed an error: {applicationErrorText}");
        }

        string firstName = Environment.GetEnvironmentVariable("SAB_PASSENGER_FIRST_NAME") ?? "Eapi";
        string lastName = Environment.GetEnvironmentVariable("SAB_PASSENGER_LAST_NAME") ?? "Automation";
        await EnterPassengerAsync(0, firstName, lastName, new DateOnly(1985, 6, 15), false);
        await CompletePassengerFormAsync(false, 1, timeoutMs);
    }

    public async Task FillGuestPassengersAsync(
        int adults,
        int teens,
        int children,
        int infants,
        DateOnly departureDate,
        bool acceptInsurance,
        bool preserveLeadPassenger,
        int timeoutMs)
    {
        await WaitUntilReadyAsync(timeoutMs);
        await VirginAtlanticConsent.DismissPrivacyBannerIfPresentAsync(Page);
        await WaitForPassengerFormToSettleAsync(5_000);
        await Task.Delay(FormHydrationDelayMs);

        string? applicationErrorText = await GetApplicationErrorTextAsync();
        if (!applicationErrorText.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(
                $"Passenger details could not be entered because the application displayed an error: {applicationErrorText}");
        }

        List<int> ages = [];
        ages.AddRange(Enumerable.Repeat(30, adults));
        ages.AddRange(Enumerable.Repeat(14, teens));
        ages.AddRange(Enumerable.Repeat(7, children));
        ages.AddRange(Enumerable.Repeat(1, infants));

        for (int index = 0; index < ages.Count; index++)
        {
            bool preserveExisting = preserveLeadPassenger && index == 0;
            string nameSuffix = PassengerNameSuffix(index);
            DateOnly dateOfBirth = new(departureDate.Year - ages[index], 6, 15);
            await EnterPassengerAsync(
                index,
                $"Eapi{nameSuffix}",
                $"Automation{nameSuffix}",
                dateOfBirth,
                preserveExisting);
        }

        await CompletePassengerFormAsync(acceptInsurance, ages.Count, timeoutMs);
    }

    public async Task AssertLeadPassengerMatchesAsync(
        string firstName,
        string lastName,
        string flyingClubNumber)
    {
        await Page.WaitForFunctionAsync(
            @"expected => {
                const firstName = document.querySelector('input[name=""firstName""]')?.value?.trim() || '';
                const lastName = document.querySelector('input[name=""lastName""]')?.value?.trim() || '';
                return firstName.localeCompare(expected.firstName, undefined, { sensitivity: 'accent' }) === 0 &&
                    lastName.localeCompare(expected.lastName, undefined, { sensitivity: 'accent' }) === 0;
            }",
            new { firstName, lastName },
            new PageWaitForFunctionOptions { Timeout = 30_000 });

        string actualFirstName = await Page.Locator("input[name=\"firstName\"]").First.InputValueAsync();
        string actualLastName = await Page.Locator("input[name=\"lastName\"]").First.InputValueAsync();
        string actualFlyingClubNumber = await Page.Locator("#fqtNumber").First.InputValueAsync();

        if (!actualFirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) ||
            !actualLastName.Equals(lastName, StringComparison.OrdinalIgnoreCase) ||
            (!actualFlyingClubNumber.IsNullOrWhiteSpace() &&
             !actualFlyingClubNumber.Equals(flyingClubNumber, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "Logged-in lead passenger was not populated with the selected Flying Club account. " +
                $"Expected '{firstName} {lastName}' and FC '{flyingClubNumber}', " +
                $"actual '{actualFirstName} {actualLastName}' and FC '{actualFlyingClubNumber}'.");
        }
    }

    public async Task<IReadOnlyList<string>> GetDobOptionsAsync(
        int passengerIndex,
        string field)
    {
        await WaitUntilReadyAsync(30_000);
        ILocator select = GetDobSelect(passengerIndex, field);
        await select.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15_000
        });
        return await select.Locator("option").EvaluateAllAsync<string[]>(
            @"options => options
                .map(option => String(option.value || option.textContent || '').trim())
                .filter(value => value && !/^(DD|MM|YYYY|Day|Month|Year)$/i.test(value))");
    }

    public async Task SetDobFieldAsync(int passengerIndex, string field, string value)
    {
        ILocator select = GetDobSelect(passengerIndex, field);
        await select.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15_000
        });
        await select.SelectOptionAsync(value);
    }

    public async Task<bool> IsDobSelectableAsync(int passengerIndex, DateOnly date)
    {
        ILocator year = GetDobSelect(passengerIndex, "year");
        ILocator month = GetDobSelect(passengerIndex, "month");
        ILocator day = GetDobSelect(passengerIndex, "day");

        try
        {
            if (!await ContainsOptionAsync(year, date.Year.ToString()))
            {
                return false;
            }
            await year.SelectOptionAsync(date.Year.ToString());

            string monthValue = date.Month.ToString("00");
            if (!await ContainsOptionAsync(month, monthValue))
            {
                return false;
            }
            await month.SelectOptionAsync(monthValue);

            string dayValue = date.Day.ToString("00");
            return await ContainsOptionAsync(day, dayValue);
        }
        finally
        {
            await Page.EvaluateAsync(
                @"idx => {
                    for (const name of ['day', 'month', 'year']) {
                        const selector = 'select[name=""' + name + '""]';
                        const select = [...document.querySelectorAll(selector)][idx];
                        if (!select) continue;
                        select.selectedIndex = 0;
                        select.dispatchEvent(new Event('input', { bubbles: true }));
                        select.dispatchEvent(new Event('change', { bubbles: true }));
                    }
                }",
                passengerIndex);
        }
    }

    public async Task<bool> IsSubmitEnabledAsync()
    {
        return await Page.EvaluateAsync<bool>(
            @"() => {
                const visible = element => {
                    const rect = element.getBoundingClientRect();
                    const style = getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                return [...document.querySelectorAll('button')].some(button => {
                    const text = (button.innerText || '').replace(/\s+/g, ' ').trim();
                    return visible(button) &&
                        /Continue to secure payment|Continue|Select seats|Choose seats|Choose your seat/i.test(text) &&
                        !button.disabled && button.getAttribute('aria-disabled') !== 'true';
                });
            }");
    }

    public async Task<bool> IsEmptyFormSubmissionPreventedAsync(int timeoutMs = 15_000)
    {
        if (!await IsSubmitEnabledAsync())
        {
            return true;
        }

        string beforeUrl = Page.Url;
        await MarkSubmitButtonAsync();
        await Page.Locator(SubmitMarkerSelector).ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });

        try
        {
            await Page.WaitForFunctionAsync(
                @"previousUrl => {
                    const visible = element => {
                        const rect = element.getBoundingClientRect();
                        const style = getComputedStyle(element);
                        return rect.width > 0 && rect.height > 0 &&
                            style.display !== 'none' && style.visibility !== 'hidden';
                    };
                    const hasInvalidField = [...document.querySelectorAll(
                        '[aria-invalid=""true""], input:invalid, select:invalid, textarea:invalid')]
                        .some(visible);
                    const hasValidationMessage = [...document.querySelectorAll(
                        '[role=""alert""], .form-error, [class*=""error-message""], [class*=""field-error""], [data-testid*=""error""]')]
                        .filter(visible)
                        .some(element => /Please fill|required|invalid|must be/i.test(element.innerText || ''));
                    return location.href !== previousUrl || hasInvalidField || hasValidationMessage;
                }",
                beforeUrl,
                new PageWaitForFunctionOptions { Timeout = timeoutMs });
        }
        catch (TimeoutException)
        {
            return false;
        }

        return Page.Url.Contains("/passengers", StringComparison.OrdinalIgnoreCase) &&
            !HasAlreadyProgressedBeyondPassengerPage(beforeUrl);
    }

    public async Task AddLoyaltyAccountAsync(string airline, string accountNumber)
    {
        await WaitUntilReadyAsync(30_000);
        ILocator programme = Page.Locator("#fqtProgramme");
        ILocator number = Page.Locator("#fqtNumber");
        await programme.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15_000
        });
        string? programmeValue = await programme.Locator("option").EvaluateAllAsync<string?>(
            @"(options, expected) => {
                const match = options.find(option =>
                    String(option.textContent || '').toLowerCase().includes(String(expected).toLowerCase()));
                return match ? match.value : null;
            }",
            airline);
        if (programmeValue.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(
                $"No loyalty programme option contained '{airline}'.");
        }
        await programme.SelectOptionAsync(programmeValue!);
        await number.FillAsync(accountNumber);
        await number.BlurAsync();

        string retainedNumber = await number.InputValueAsync();
        string selectedAirline = await programme.Locator("option:checked").InnerTextAsync();
        if (retainedNumber != accountNumber ||
            !selectedAirline.Contains(airline, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Loyalty account was not retained. Airline '{selectedAirline}', number '{retainedNumber}'.");
        }
    }

    public async Task SubmitAndWaitForProgressAsync(int timeoutMs)
    {
        await VirginAtlanticConsent.DismissPrivacyBannerIfPresentAsync(Page);
        await WaitForPassengerFormToSettleAsync(5_000);
        int newlyTicked = await TickConsentCheckboxesAsync();
        if (newlyTicked > 0)
        {
            await WaitForPassengerFormToSettleAsync(5_000);
        }
        if (_expectedPassengerCount > 0)
        {
            await AssertRequiredPassengerFieldsRetainedAsync(_expectedPassengerCount);
        }
        string beforeUrl = Page.Url;
        List<string> attempts = [];
        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        for (int attempt = 1; attempt <= 3 && DateTime.UtcNow < deadline; attempt++)
        {
            if (HasAlreadyProgressedBeyondPassengerPage(beforeUrl))
            {
                return;
            }

            await EnsureConsentCheckboxesSelectedAsync(
                Math.Min(10_000, Math.Max(1_000, (int)(deadline - DateTime.UtcNow).TotalMilliseconds)));
            string chosenText;
            try
            {
                chosenText = await MarkSubmitButtonAsync();
            }
            catch (InvalidOperationException) when (HasAlreadyProgressedBeyondPassengerPage(beforeUrl))
            {
                return;
            }
            attempts.Add(chosenText);
            ILocator submitButton = Page.Locator(SubmitMarkerSelector);
            await submitButton.ScrollIntoViewIfNeededAsync();
            float apiTimeoutMs = Math.Max(1_000, (float)(deadline - DateTime.UtcNow).TotalMilliseconds);
            Task<IResponse> termsResponseTask = WaitForCheckoutResponseAsync("/book/terms", apiTimeoutMs);
            Task<IResponse> passengerResponseTask = WaitForCheckoutResponseAsync("/book/passenger/", apiTimeoutMs);
            if (attempt == 1)
            {
                await submitButton.ClickAsync(new LocatorClickOptions { Timeout = 15_000 });
            }
            else
            {
                await submitButton.EvaluateAsync("button => button.click()");
            }

            try
            {
                IResponse[] checkoutResponses = await Task.WhenAll(termsResponseTask, passengerResponseTask);
                foreach (IResponse response in checkoutResponses)
                {
                    await EnsureSuccessfulCheckoutResponseAsync(response);
                }
            }
            catch (TimeoutException)
            {
                if (await IsPassengerSubmitProcessingAsync())
                {
                    if (await HasProgressedBeyondPassengerPageAsync(beforeUrl))
                    {
                        return;
                    }

                    throw new InvalidOperationException(
                        $"Passenger submit was accepted but the checkout APIs did not complete within {timeoutMs}ms. " +
                        "Awaited PUT responses for /book/terms and /book/passenger/0. " +
                        $"{await GetPageSummaryAsync()}");
                }

                throw;
            }

            try
            {
                await Page.WaitForFunctionAsync(
                    @"previousUrl => {
                        const isVisible = element => {
                            const rect = element.getBoundingClientRect();
                            const style = getComputedStyle(element);
                            return rect.width > 0 && rect.height > 0 &&
                                style.display !== 'none' && style.visibility !== 'hidden';
                        };
                        const url = location.href;
                        const bodyText = (document.body?.innerText || '').trim();
                        const dialogs = [...document.querySelectorAll('[role=""dialog""], [aria-modal=""true""]')]
                            .filter(isVisible);
                        const seatModal = dialogs.some(dialog => {
                            const text = dialog.innerText || '';
                            return /Your seat, your choice/i.test(text) ||
                                (/Select seats?/i.test(text) && /Skip to payment/i.test(text));
                        });
                        const applicationError = dialogs.some(dialog =>
                            /Uh oh|something.?s not right|unable to book|encountered an error/i.test(dialog.innerText || ''));
                        const validationError = [...document.querySelectorAll(
                            '[role=""alert""], .form-error, [class*=""error-message""], [class*=""field-error""], [data-testid*=""error""]')]
                            .filter(isVisible)
                            .some(element => /Please fill|required|invalid|must be/i.test(element.innerText || ''));
                        const invalidControl = [...document.querySelectorAll('[aria-invalid=""true""]')]
                            .some(isVisible);
                        return !url.includes('/passengers') || url !== previousUrl || seatModal ||
                            url.includes('/payment') || /How would you like to pay/i.test(bodyText) ||
                            applicationError || validationError || invalidControl;
                    }",
                    beforeUrl,
                    new PageWaitForFunctionOptions
                    {
                        Timeout = Math.Min(25_000, Math.Max(1_000, (float)(deadline - DateTime.UtcNow).TotalMilliseconds))
                    });
            }
            catch (TimeoutException)
            {
                if (await IsPassengerSubmitProcessingAsync())
                {
                    try
                    {
                        await Page.WaitForFunctionAsync(
                            @"previousUrl => {
                                const url = location.href;
                                const bodyText = document.body?.innerText || '';
                                return !url.includes('/passengers') || url !== previousUrl ||
                                    url.includes('/payment') || /How would you like to pay/i.test(bodyText);
                            }",
                            beforeUrl,
                            new PageWaitForFunctionOptions
                            {
                                Timeout = Math.Max(1_000, (float)(deadline - DateTime.UtcNow).TotalMilliseconds)
                            });
                    }
                    catch (TimeoutException)
                    {
                        // The dedicated processing failure below retains the relevant page state.
                    }
                }
                // Retry only when the click produced neither progress nor a processing signal.
            }

            string? applicationErrorText = await GetApplicationErrorTextAsync();
            if (!applicationErrorText.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException(
                    $"Passenger details page showed an application error after clicking '{chosenText}': {applicationErrorText}");
            }

            if (HasAlreadyProgressedBeyondPassengerPage(beforeUrl))
            {
                return;
            }

            string validationErrors = await GetPassengerValidationErrorsAsync();
            if (!validationErrors.Equals("<none>", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Passenger submit click '{chosenText}' was rejected. Visible validation errors: {validationErrors}.");
            }

            if (await IsPassengerSubmitProcessingAsync())
            {
                if (await HasProgressedBeyondPassengerPageAsync(beforeUrl))
                {
                    return;
                }

                throw new InvalidOperationException(
                    $"Passenger submit was accepted but backend processing did not complete within {timeoutMs}ms. " +
                    $"The page remained on /passengers with the payment interstitial visible. {await GetPageSummaryAsync()}");
            }
        }

        if (HasAlreadyProgressedBeyondPassengerPage(beforeUrl))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Passenger submit did not progress after {attempts.Count} attempts " +
            $"({string.Join(" | ", attempts)}). {await GetPageSummaryAsync()}");
    }

    private bool HasAlreadyProgressedBeyondPassengerPage(string beforeUrl)
    {
        return !Page.Url.Contains("/passengers", StringComparison.OrdinalIgnoreCase) ||
            !Page.Url.Equals(beforeUrl, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<bool> HasProgressedBeyondPassengerPageAsync(string beforeUrl)
    {
        if (HasAlreadyProgressedBeyondPassengerPage(beforeUrl))
        {
            return true;
        }

        await Task.Delay(750);
        return HasAlreadyProgressedBeyondPassengerPage(beforeUrl);
    }

    public async Task SubmitForSeatSelectionAsync(int timeoutMs)
    {
        await VirginAtlanticConsent.DismissPrivacyBannerIfPresentAsync(Page);
        await WaitForPassengerFormToSettleAsync(Math.Min(timeoutMs, 5_000));
        await TickConsentCheckboxesAsync();

        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        string beforeUrl = Page.Url;
        List<string> attempts = [];
        for (int attempt = 1; attempt <= 3 && DateTime.UtcNow < deadline; attempt++)
        {
            if (HasAlreadyProgressedBeyondPassengerPage(beforeUrl))
            {
                return;
            }

            await EnsureConsentCheckboxesSelectedAsync(
                Math.Min(10_000, Math.Max(1_000, (int)(deadline - DateTime.UtcNow).TotalMilliseconds)));
            await WaitForSeatSelectionCandidateAsync(
                Math.Min(30_000, Math.Max(1_000, (int)(deadline - DateTime.UtcNow).TotalMilliseconds)));
            await EnsureConsentCheckboxesSelectedAsync(
                Math.Min(10_000, Math.Max(1_000, (int)(deadline - DateTime.UtcNow).TotalMilliseconds)));
            string chosenText = await MarkSeatSelectionButtonAsync();
            attempts.Add(chosenText);
            ILocator submitButton = Page.Locator(SubmitMarkerSelector);
            await submitButton.ScrollIntoViewIfNeededAsync();
            if (attempt == 1)
            {
                await submitButton.ClickAsync(new LocatorClickOptions { Timeout = 15_000 });
            }
            else
            {
                await submitButton.EvaluateAsync("button => button.click()");
            }

            try
            {
                await Page.WaitForFunctionAsync(
                    @"([previousUrl]) => {
                        const currentUrl = window.location.href;
                        const isVisible = element => {
                            const rect = element.getBoundingClientRect();
                            const style = getComputedStyle(element);
                            return rect.width > 0 && rect.height > 0 &&
                                style.display !== 'none' && style.visibility !== 'hidden';
                        };
                        const seatModalOpen = [...document.querySelectorAll('[role=""dialog""], [aria-modal=""true""]')]
                            .filter(isVisible)
                            .some(dialog => {
                                const text = dialog.innerText || '';
                                return /Your seat, your choice/i.test(text) ||
                                    (/Select seats?/i.test(text) && /Skip to payment/i.test(text));
                            });
                        const applicationError = [...document.querySelectorAll('[role=""dialog""], [aria-modal=""true""]')]
                            .filter(isVisible)
                            .some(dialog => /Uh oh|something.?s not right|unable to book|encountered an error/i
                                .test(dialog.innerText || ''));
                        return !currentUrl.includes('/passengers') ||
                            currentUrl !== previousUrl ||
                            seatModalOpen ||
                            applicationError;
                    }",
                    new[] { beforeUrl },
                    new PageWaitForFunctionOptions
                    {
                        Timeout = Math.Min(20_000, Math.Max(1_000, (float)(deadline - DateTime.UtcNow).TotalMilliseconds))
                    });
            }
            catch (TimeoutException)
            {
                // Re-resolve and retry only while the passenger page remains unchanged.
            }

            if (Page.Url.Contains("/passengers", StringComparison.OrdinalIgnoreCase) &&
                await HandleSeatChoiceModalIfPresentAsync(preferSelectSeats: true))
            {
                try
                {
                    await Page.WaitForFunctionAsync(
                        "() => !window.location.href.includes('/passengers')",
                        null,
                        new PageWaitForFunctionOptions
                        {
                            Timeout = Math.Min(20_000, Math.Max(1_000, (float)(deadline - DateTime.UtcNow).TotalMilliseconds))
                        });
                }
                catch (TimeoutException)
                {
                    // A remounted modal action is safe to retry while navigation has not started.
                }
            }

            string? applicationErrorText = await GetApplicationErrorTextAsync();
            if (!applicationErrorText.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException(
                    $"Passenger details page showed an application error after clicking '{chosenText}': {applicationErrorText}");
            }

            if (HasAlreadyProgressedBeyondPassengerPage(beforeUrl))
            {
                return;
            }

            string validationErrors = await GetPassengerValidationErrorsAsync();
            if (!validationErrors.Equals("<none>", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Passenger seat-selection click '{chosenText}' was rejected. " +
                    $"Visible validation errors: {validationErrors}. {await GetPageSummaryAsync()}");
            }

            if (await IsPassengerSubmitProcessingAsync())
            {
                if (await HasProgressedBeyondPassengerPageAsync(beforeUrl))
                {
                    return;
                }

                throw new InvalidOperationException(
                    $"Passenger seat-selection submit '{chosenText}' was accepted but backend processing did not complete " +
                    $"within {timeoutMs}ms. The page remained on /passengers with the payment interstitial visible. " +
                    await GetPageSummaryAsync());
            }

            await Page.WaitForFunctionAsync(
                @"() => {
                    const button = document.querySelector('[data-eapi-playwright-target=""pax-submit""]');
                    return !location.href.includes('/passengers') ||
                        (button && !button.disabled && button.getAttribute('aria-disabled') !== 'true');
                }",
                null,
                new PageWaitForFunctionOptions
                {
                    Timeout = Math.Min(5_000, Math.Max(1_000, (float)(deadline - DateTime.UtcNow).TotalMilliseconds))
                });
        }

        throw new InvalidOperationException(
            $"Passenger seat-selection did not leave the passenger page after {attempts.Count} bounded attempts " +
            $"({string.Join(" | ", attempts)}). Visible validation errors: {await GetPassengerValidationErrorsAsync()}. " +
            await GetPageSummaryAsync());
    }

    private async Task WaitForSeatSelectionCandidateAsync(int timeoutMs)
    {
        await Page.WaitForFunctionAsync(
            @"() => {
                const isVisible = element => {
                    const rect = element.getBoundingClientRect();
                    const style = getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                const seatAction = [...document.querySelectorAll('button')].some(button => {
                    const text = (button.innerText || '').replace(/\s+/g, ' ').trim();
                    return isVisible(button) &&
                        /Select seats|Choose seats|Choose your seat/i.test(text) &&
                        !button.disabled && button.getAttribute('aria-disabled') !== 'true';
                });
                const applicationError = [...document.querySelectorAll('[role=""dialog""], [aria-modal=""true""]')]
                    .some(dialog => isVisible(dialog) &&
                        /Uh oh|something.?s not right|unable to book|encountered an error/i.test(dialog.innerText || ''));
                return seatAction || applicationError;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });

        string? applicationErrorText = await GetApplicationErrorTextAsync();
        if (!applicationErrorText.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(
                $"Passenger details page showed an application error before seat selection became available: {applicationErrorText}");
        }
    }

    private async Task EnsureConsentCheckboxesSelectedAsync(int timeoutMs)
    {
        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        do
        {
            await TickConsentCheckboxesAsync();
            await Task.Delay(500);
            bool selected = await Page.EvaluateAsync<bool>(
                @"() => {
                    const checkboxes = [...document.querySelectorAll('label')]
                        .filter(label => /I confirm|important information/i.test((label.innerText || '').trim()))
                        .map(label => {
                            const forId = label.getAttribute('for') || '';
                            return forId
                                ? document.getElementById(forId)
                                : label.querySelector('input[type=""checkbox""]');
                        })
                        .filter(Boolean);
                    return checkboxes.length > 0 && checkboxes.every(checkbox => checkbox.checked);
                }");
            if (selected)
            {
                return;
            }
        }
        while (DateTime.UtcNow < deadline);

        throw new InvalidOperationException(
            "Passenger consent was cleared by a form remount and could not be retained before seat selection.");
    }

    private async Task<bool> HandleSeatChoiceModalIfPresentAsync(bool preferSelectSeats)
    {
        bool marked = await Page.EvaluateAsync<bool>(
            @"(selectSeats) => {
                document.querySelectorAll('[data-eapi-playwright-target=""seat-choice-modal-action""]')
                    .forEach(element => element.removeAttribute('data-eapi-playwright-target'));
                const isVisible = element => {
                    const rect = element.getBoundingClientRect();
                    const style = getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                for (const dialog of document.querySelectorAll('[role=""dialog""], [aria-modal=""true""]')) {
                    if (!isVisible(dialog)) continue;
                    const text = dialog.innerText || '';
                    const isSeatChoice = /Your seat, your choice/i.test(text) ||
                        (/Select seats?/i.test(text) && /Skip to payment/i.test(text));
                    if (!isSeatChoice) continue;
                    const expected = selectSeats ? /Select seats?/i : /Skip to payment/i;
                    const button = [...dialog.querySelectorAll('button, [role=""button""]')]
                        .find(candidate => isVisible(candidate) && expected.test(candidate.innerText || candidate.textContent || ''));
                    if (!button) return false;
                    button.setAttribute('data-eapi-playwright-target', 'seat-choice-modal-action');
                    return true;
                }
                return false;
            }",
            preferSelectSeats);

        if (!marked)
        {
            return false;
        }

        ILocator action = Page.Locator(SeatChoiceModalActionMarkerSelector);
        await action.ScrollIntoViewIfNeededAsync();
        await action.ClickAsync(new LocatorClickOptions { Timeout = 15_000 });
        return true;
    }

    public async Task<string> GetPageSummaryAsync()
    {
        return await Page.EvaluateAsync<string>(
            @"() => {
                const visible = element => {
                    const rect = element.getBoundingClientRect();
                    const style = getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                const visibleCount = selector => [...document.querySelectorAll(selector)].filter(visible).length;
                return `URL: ${location.origin}${location.pathname}; ` +
                    `visiblePassengerInputs: ${visibleCount('input[name=""firstName""]')}; ` +
                    `visibleValidationErrors: ${visibleCount('[role=""alert""], [aria-invalid=""true""]')}; ` +
                    `visibleSubmitActions: ${visibleCount('[data-eapi-playwright-target=""pax-submit""]')}; ` +
                    `visibleLoaders: ${visibleCount('[data-testid=""search-loader""], [data-testid=""tailfin-loader-component""], [data-testid=""interstitial-component""]')}`;
            }");
    }

    private async Task FillContactInformationAsync(int timeoutMs)
    {
        ContactValidationResult? validationResult = null;
        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        do
        {
            await FillIfEmptyAsync("input[name=\"email\"]", "eapi.automation@example.com");
            await SelectAsync("select[name=\"countryCode\"]", "44");
            await FillIfEmptyAsync("input[name=\"telephone\"]", "7700900123");
            await Task.Delay(250);

            validationResult = await GetContactValidationResultAsync();
            if (validationResult.Valid)
            {
                return;
            }

            await Task.Delay(500);
        }
        while (DateTime.UtcNow < deadline);

        throw new InvalidOperationException(
            "Passenger contact information did not become valid. " +
            $"Email present={!validationResult?.Email.IsNullOrWhiteSpace()}; " +
            $"country code present={!validationResult?.CountryCode.IsNullOrWhiteSpace()}; " +
            $"telephone present={!validationResult?.Telephone.IsNullOrWhiteSpace()}; " +
            $"validation error count={validationResult?.Errors.Count() ?? 0}.");
    }

    private async Task EnterPassengerAsync(
        int index,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        bool preserveExisting)
    {
        await FillIfEmptyAsync("input[name=\"firstName\"]", firstName, index, preserveExisting);
        await FillIfEmptyAsync("input[name=\"lastName\"]", lastName, index, preserveExisting);
        await SelectAsync("select[name=\"day\"]", dateOfBirth.Day.ToString("00"), index, preserveExisting);
        await SelectAsync("select[name=\"month\"]", dateOfBirth.Month.ToString("00"), index, preserveExisting);
        await SelectAsync("select[name=\"year\"]", dateOfBirth.Year.ToString(), index, preserveExisting);
        await SelectAsync("select[name=\"gender\"]", "Male", index, preserveExisting);
    }

    private async Task CompletePassengerFormAsync(
        bool acceptInsurance,
        int passengerCount,
        int timeoutMs)
    {
        await FillContactInformationAsync(timeoutMs);
        PassengerFieldSnapshot[] passengerSnapshot = await CapturePassengerFieldsAsync(passengerCount);
        await ClickIfPresentAsync(acceptInsurance ? "#protectTrip" : "#noProtectTrip");
        await WaitForConsentControlsReadyAsync(Math.Min(timeoutMs, 15_000));
        await TickConsentCheckboxesAsync();
        await WaitForPassengerFormToSettleAsync(5_000);

        // Insurance and consent updates can remount the React form and clear passenger values.
        // Restore only fields that became empty, retaining account-populated and user-entered data.
        await RestoreMissingPassengerFieldsAsync(passengerSnapshot);
        await WaitForPassengerFormToSettleAsync(5_000);

        _expectedPassengerCount = passengerCount;
        await AssertRequiredPassengerFieldsRetainedAsync(passengerCount);
        await AssertContactInformationRetainedAsync();
        await WaitForSubmitCandidateAsync(timeoutMs);
    }

    private async Task WaitForConsentControlsReadyAsync(int timeoutMs)
    {
        await Page.WaitForFunctionAsync(
            @"() => [...document.querySelectorAll('label')].some((label) => {
                const text = (label.innerText || '').trim();
                if (!/I confirm|important information/i.test(text)) {
                    return false;
                }

                const forId = label.getAttribute('for') || '';
                const checkbox = forId
                    ? document.getElementById(forId)
                    : label.querySelector('input[type=""checkbox""]');
                return checkbox && !checkbox.disabled && checkbox.getAttribute('aria-disabled') !== 'true';
            })",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }

    private async Task<PassengerFieldSnapshot[]> CapturePassengerFieldsAsync(int passengerCount)
    {
        return await Page.EvaluateAsync<PassengerFieldSnapshot[]>(
            @"count => {
                const visible = selector => [...document.querySelectorAll(selector)]
                    .filter(element => {
                        const rect = element.getBoundingClientRect();
                        const style = getComputedStyle(element);
                        return rect.width > 0 && rect.height > 0 &&
                            style.display !== 'none' && style.visibility !== 'hidden';
                    });
                const fields = {
                    FirstName: visible('input[name=""firstName""]'),
                    LastName: visible('input[name=""lastName""]'),
                    Day: visible('select[name=""day""]'),
                    Month: visible('select[name=""month""]'),
                    Year: visible('select[name=""year""]'),
                    Gender: visible('select[name=""gender""]')
                };
                return Array.from({ length: count }, (_, index) =>
                    Object.fromEntries(Object.entries(fields).map(([name, elements]) =>
                        [name, String(elements[index]?.value || '').trim()])));
            }",
            passengerCount);
    }

    private async Task RestoreMissingPassengerFieldsAsync(PassengerFieldSnapshot[] snapshots)
    {
        for (int index = 0; index < snapshots.Length; index++)
        {
            PassengerFieldSnapshot snapshot = snapshots[index];
            await FillIfEmptyAsync("input[name=\"firstName\"]", snapshot.FirstName, index, preserveExisting: true);
            await FillIfEmptyAsync("input[name=\"lastName\"]", snapshot.LastName, index, preserveExisting: true);
            await SelectAsync("select[name=\"day\"]", snapshot.Day, index, preserveExisting: true);
            await SelectAsync("select[name=\"month\"]", snapshot.Month, index, preserveExisting: true);
            await SelectAsync("select[name=\"year\"]", snapshot.Year, index, preserveExisting: true);
            await SelectAsync("select[name=\"gender\"]", snapshot.Gender, index, preserveExisting: true);
        }
    }

    private sealed class PassengerFieldSnapshot
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Day { get; set; } = string.Empty;

        public string Month { get; set; } = string.Empty;

        public string Year { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;
    }

    private async Task AssertContactInformationRetainedAsync()
    {
        ContactValidationResult result = await GetContactValidationResultAsync();
        if (!result.Valid)
        {
            // React occasionally remounts the contact section after consent and clears all
            // three values. Repair only after the read-only check proves that happened.
            await FillContactInformationAsync(15_000);
            await WaitForPassengerFormToSettleAsync(2_000);
            result = await GetContactValidationResultAsync();
        }

        if (!result.Valid)
        {
            throw new InvalidOperationException(
                "Passenger contact information was not retained after consent was selected. " +
                $"Email present={!result.Email.IsNullOrWhiteSpace()}; " +
                $"country code present={!result.CountryCode.IsNullOrWhiteSpace()}; " +
                $"telephone present={!result.Telephone.IsNullOrWhiteSpace()}; " +
                $"validation error count={result.Errors.Count()}.");
        }
    }

    private static string PassengerNameSuffix(int index)
    {
        string[] suffixes =
        [
            "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine"
        ];
        if (index < 0 || index >= suffixes.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index),
                index,
                $"Passenger name generation supports up to {suffixes.Length} passengers.");
        }

        return suffixes[index];
    }

    private async Task AssertRequiredPassengerFieldsRetainedAsync(int passengerCount)
    {
        string[] missing = await Page.EvaluateAsync<string[]>(
            @"count => {
                const missing = [];
                const isVisible = element => {
                    const rect = element.getBoundingClientRect();
                    const style = getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                const values = selector => [...document.querySelectorAll(selector)]
                    .filter(isVisible)
                    .slice(0, count)
                    .map(element => String(element.value || '').trim());
                for (const [name, selector] of Object.entries({
                    firstName: 'input[name=""firstName""]',
                    lastName: 'input[name=""lastName""]',
                    day: 'select[name=""day""]',
                    month: 'select[name=""month""]',
                    year: 'select[name=""year""]',
                    gender: 'select[name=""gender""]'
                })) {
                    values(selector).forEach((value, index) => {
                        if (!value) missing.push('passenger ' + (index + 1) + ' ' + name);
                    });
                }
                return missing;
            }",
            passengerCount);

        if (missing.Length > 0)
        {
            throw new InvalidOperationException(
                $"Passenger data was not retained after the form settled: {string.Join(", ", missing)}.");
        }
    }

    private async Task<string> GetPassengerValidationErrorsAsync()
    {
        string[] errors = await Page.EvaluateAsync<string[]>(
            @"() => {
                const visible = element => {
                    const rect = element.getBoundingClientRect();
                    const style = getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                const messages = [...document.querySelectorAll(
                    '[role=""alert""], .form-error, [class*=""error-message""], [class*=""field-error""], [data-testid*=""error""]')]
                    .filter(visible)
                    .map(element => (element.innerText || element.textContent || '').replace(/\s+/g, ' ').trim())
                    .filter(text => text && /Please fill|required|invalid|must be/i.test(text));
                const invalidControls = [...document.querySelectorAll('[aria-invalid=""true""]')]
                    .filter(visible)
                    .map(element => {
                        const label = element.id ? document.querySelector(`label[for=""${element.id}""]`) : null;
                        return (label?.innerText || element.name || element.id || 'invalid field').trim();
                    });
                return [...new Set([...messages, ...invalidControls])].slice(0, 20);
            }");
        return errors.Length == 0 ? "<none>" : string.Join(" | ", errors);
    }

    private ILocator GetDobSelect(int passengerIndex, string field)
    {
        if (field is not ("day" or "month" or "year"))
        {
            throw new ArgumentOutOfRangeException(nameof(field), field, "DOB field must be day, month or year.");
        }

        return Page.Locator($"select[name='{field}']:visible").Nth(passengerIndex);
    }

    private static async Task<bool> ContainsOptionAsync(ILocator select, string value)
    {
        return await select.Locator("option").EvaluateAllAsync<bool>(
            @"(options, expected) => options.some(option =>
                String(option.value || option.textContent || '').trim() === expected)",
            value);
    }

    private async Task WaitForPassengerFormToSettleAsync(int timeoutMs)
    {
        try
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

                    const visibleBusy = [...document.querySelectorAll('[data-testid=""search-loader""], [data-testid=""tailfin-loader-component""], [data-testid=""interstitial-component""], [aria-busy=""true""]')]
                        .some(isVisible);
                    return !visibleBusy;
                }",
                null,
                new PageWaitForFunctionOptions { Timeout = timeoutMs });
        }
        catch (TimeoutException)
        {
            // The passenger page can retain stale loading copy; do not turn a stability pause into a hard failure.
        }
        catch (PlaywrightException exception) when (exception.Message.Contains("Timeout", StringComparison.OrdinalIgnoreCase))
        {
            // The subsequent validation/submit checks provide the real failure signal.
        }

        await Task.Delay(1_000);
    }
    private async Task<ContactValidationResult> GetContactValidationResultAsync()
    {
        return await Page.EvaluateAsync<ContactValidationResult>(
            @"() => {
                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 &&
                        rect.height > 0 &&
                        style.display !== 'none' &&
                        style.visibility !== 'hidden';
                };

                const valueOfLastVisible = (selector) => {
                    const elements = [...document.querySelectorAll(selector)].filter(isVisible);
                    const element = elements[elements.length - 1];
                    return element ? (element.value || '').trim() : '';
                };

                const contactSections = [...document.querySelectorAll('section, div, form')]
                    .filter((element) => isVisible(element) && /Contact information/i.test(element.innerText || ''));
                const contactSection = contactSections[0] || document;
                const errors = [...contactSection.querySelectorAll('[class*=""error""], [role=""alert""], .form-error')]
                    .filter(isVisible)
                    .map((element) => (element.innerText || element.textContent || '').replace(/\s+/g, ' ').trim())
                    .filter(Boolean);

                const email = valueOfLastVisible('input[name=""email""]');
                const countryCode = valueOfLastVisible('select[name=""countryCode""]');
                const telephone = valueOfLastVisible('input[name=""telephone""]');
                const valid = /@/.test(email) && countryCode.length > 0 && telephone.length >= 7;

                return { Email: email, CountryCode: countryCode, Telephone: telephone, Errors: errors, Valid: valid };
            }");
    }
    private async Task FillIfEmptyAsync(
        string selector,
        string value,
        int index = 0,
        bool preserveExisting = false)
    {
        ILocator locator = Page.Locator($"{selector}:visible").Nth(index);
        await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 30_000 });
        string currentValue = await locator.InputValueAsync();
        if (preserveExisting && !currentValue.IsNullOrWhiteSpace())
        {
            return;
        }

        if (currentValue == value)
        {
            return;
        }

        await locator.ScrollIntoViewIfNeededAsync();
        await locator.ClickAsync(new LocatorClickOptions { Timeout = 10_000 });
        await locator.PressAsync("Control+A");
        await locator.PressAsync("Backspace");
        await locator.PressSequentiallyAsync(value, new LocatorPressSequentiallyOptions { Delay = 35 });
        await Page.WaitForFunctionAsync(
            @"([selector, expected, index]) => {
                const visible = [...document.querySelectorAll(selector)].filter((element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 && style.display !== 'none' && style.visibility !== 'hidden';
                });
                const element = visible[index];
                return element && element.value === expected;
            }",
            new object[] { selector, value, index },
            new PageWaitForFunctionOptions { Timeout = 10_000 });
        await Task.Delay(ReactInputSettleDelayMs);
    }

    private async Task SelectAsync(
        string selector,
        string value,
        int index = 0,
        bool preserveExisting = false)
    {
        ILocator locator = Page.Locator($"{selector}:visible").Nth(index);
        await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 30_000 });
        if (preserveExisting && !(await locator.InputValueAsync()).IsNullOrWhiteSpace())
        {
            return;
        }

        try
        {
            IReadOnlyList<string> selectedValues = await locator.SelectOptionAsync(
                value,
                new LocatorSelectOptionOptions { Timeout = 10_000 });
            if (selectedValues.Count > 0 &&
                await WaitForOptionValueAsync(locator, value, TimeSpan.FromSeconds(3)))
            {
                await Task.Delay(ReactInputSettleDelayMs);
                return;
            }
        }
        catch (PlaywrightException)
        {
            // Some controls expose a label that differs from its value; match a concrete option below.
        }

        // BrowserStack's real-iOS Playwright bridge can report a successful SelectOption call
        // without dispatching the events React needs to retain the value. Commit the same
        // existing option in-page; this is only reached when the native interaction did not stick.
        string commitSelectionScript =
            @"() => {
                const selector = __SELECTOR__;
                const wantedIndex = __INDEX__;
                const expected = __EXPECTED__;
                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                const select = [...document.querySelectorAll(selector)].filter(isVisible)[wantedIndex];
                if (!select) return;

                const normalise = (candidate) => (candidate || '').toString().trim().toLowerCase();
                const wanted = normalise(expected);
                const wantedNumber = /^\d+$/.test(wanted) ? Number(wanted) : null;
                const options = [...select.options];
                const match = options.find((option) => normalise(option.value) === wanted) ||
                    options.find((option) => wantedNumber !== null && /^\d+$/.test(normalise(option.value)) &&
                        Number(option.value) === wantedNumber) ||
                    options.find((option) => normalise(option.textContent) === wanted) ||
                    options.find((option) => normalise(option.value).includes(wanted) ||
                        normalise(option.textContent).includes(wanted));
                if (!match) return;

                select.selectedIndex = options.indexOf(match);
                select.value = match.value;
                select.dispatchEvent(new Event('input', { bubbles: true, composed: true }));
                select.dispatchEvent(new Event('change', { bubbles: true, composed: true }));
            }"
            .Replace(
                "__SELECTOR__",
                System.Text.Json.JsonSerializer.Serialize(selector),
                StringComparison.Ordinal)
            .Replace("__INDEX__", index.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace(
                "__EXPECTED__",
                System.Text.Json.JsonSerializer.Serialize(value),
                StringComparison.Ordinal);

        for (int attempt = 0; attempt < 3; attempt++)
        {
            await Page.EvaluateAsync(commitSelectionScript);

            if (await WaitForOptionValueAsync(locator, value, TimeSpan.FromSeconds(3)))
            {
                await Task.Delay(ReactInputSettleDelayMs);
                return;
            }
        }

        IReadOnlyList<string> optionTexts = await locator.Locator("option").AllTextContentsAsync();
        string retainedValue = await locator.InputValueAsync();

        throw new InvalidOperationException(
            $"No option matching '{value}' could be retained for '{selector}'. " +
            $"Retained value: '{retainedValue}'. Available option labels: {string.Join(", ", optionTexts.Take(40))}");
    }

    private static async Task<bool> WaitForOptionValueAsync(
        ILocator locator,
        string expected,
        TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow.Add(timeout);
        do
        {
            try
            {
                if (OptionValueMatches(await locator.InputValueAsync(), expected))
                {
                    return true;
                }
            }
            catch (PlaywrightException)
            {
                // React can replace the select while committing its state; locators re-resolve on the next poll.
            }

            await Task.Delay(250);
        }
        while (DateTime.UtcNow < deadline);

        return false;
    }

    private static bool OptionValueMatches(string actual, string expected)
    {
        if (actual.Equals(expected, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return int.TryParse(actual, out int actualNumber) &&
            int.TryParse(expected, out int expectedNumber) &&
            actualNumber == expectedNumber;
    }

    private sealed class ContactValidationResult
    {
        public string Email { get; set; } = string.Empty;

        public string CountryCode { get; set; } = string.Empty;

        public string Telephone { get; set; } = string.Empty;

        public string[] Errors { get; set; } = Array.Empty<string>();

        public bool Valid { get; set; }
    }

    private async Task ClickIfPresentAsync(string selector)
    {
        await Page.EvaluateAsync(
            @"(selector) => {
                const element = document.querySelector(selector);
                if (!element) {
                    return;
                }

                const isChoice = element.matches('input[type=""radio""], input[type=""checkbox""]');
                if (isChoice && element.checked) {
                    return;
                }

                element.scrollIntoView({ block: 'center', inline: 'center' });
                const label = element.id ? document.querySelector(`label[for=""${element.id}""]`) : null;
                const target = label || element;
                target.click();

                if (isChoice && !element.checked) {
                    element.checked = true;
                    element.dispatchEvent(new Event('input', { bubbles: true }));
                    element.dispatchEvent(new Event('change', { bubbles: true }));
                }
            }",
            selector);

        await Task.Delay(150);
    }

    private async Task<int> TickConsentCheckboxesAsync()
    {
        return await Page.EvaluateAsync<int>(
            @"() => {
                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 &&
                        rect.height > 0 &&
                        style.visibility !== 'hidden' &&
                        style.display !== 'none';
                };

                let ticked = 0;
                for (const label of [...document.querySelectorAll('label')]) {
                    const text = (label.innerText || '').trim();
                    if (!/I confirm|important information/i.test(text)) {
                        continue;
                    }

                    const forId = label.getAttribute('for') || '';
                    const checkbox = forId ? document.getElementById(forId) : label.querySelector('input[type=""checkbox""]');
                    if (!checkbox || checkbox.checked) {
                        continue;
                    }

                    const target = isVisible(label) ? label : checkbox;
                    target.scrollIntoView({ block: 'center', inline: 'center' });
                    target.click();
                    ticked += 1;
                }

                return ticked;
            }");
    }

    private async Task<string?> GetApplicationErrorTextAsync()
    {
        return await Page.EvaluateAsync<string?>(
            @"() => {
                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 &&
                        rect.height > 0 &&
                        style.display !== 'none' &&
                        style.visibility !== 'hidden';
                };

                const dialogs = [...document.querySelectorAll('[role=""dialog""], [aria-modal=""true""]')]
                    .filter(isVisible);
                const errorDialog = dialogs.find((dialog) =>
                    /Uh oh|something.?s not right|unable to book|encountered an error/i.test(dialog.innerText || ''));
                if (!errorDialog) {
                    return null;
                }

                return (errorDialog.innerText || '').replace(/\s+/g, ' ').trim();
            }");
    }

    private async Task<bool> IsPassengerSubmitProcessingAsync()
    {
        return await Page.EvaluateAsync<bool>(
            @"() => {
                const visible = element => {
                    const rect = element.getBoundingClientRect();
                    const style = getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                const bodyText = document.body?.innerText || '';
                const visibleBusy = [...document.querySelectorAll(
                    '[data-testid=""search-loader""], [data-testid=""tailfin-loader-component""], [data-testid=""interstitial-component""], [aria-busy=""true""]')]
                    .some(visible);
                return visibleBusy || /Next,? payment options\s+One moment please/i.test(bodyText);
            }");
    }

    private Task<IResponse> WaitForCheckoutResponseAsync(string pathFragment, float timeoutMs)
    {
        return Page.WaitForResponseAsync(
            response => response.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase) &&
                response.Url.Contains("/flight-only/api/checkout/", StringComparison.OrdinalIgnoreCase) &&
                response.Url.Contains(pathFragment, StringComparison.OrdinalIgnoreCase) &&
                (response.Status < 300 || response.Status >= 400),
            new PageWaitForResponseOptions { Timeout = timeoutMs });
    }

    private static async Task EnsureSuccessfulCheckoutResponseAsync(IResponse response)
    {
        if (response.Ok)
        {
            return;
        }

        string responseBody;
        try
        {
            responseBody = await response.TextAsync();
        }
        catch (PlaywrightException exception)
        {
            responseBody = $"<response body unavailable: {exception.Message}>";
        }

        if (responseBody.Length > 1_000)
        {
            responseBody = responseBody[..1_000];
        }

        throw new InvalidOperationException(
            $"Passenger checkout API failed: {response.Request.Method} {response.Url} returned " +
            $"HTTP {response.Status} {response.StatusText}. Body: {responseBody}");
    }

    private async Task WaitForSubmitCandidateAsync(int timeoutMs)
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

                const hasSubmitCandidate = [...document.querySelectorAll('button')].some((button) => {
                    const text = (button.innerText || '').replace(/\s+/g, ' ').trim();
                    return isVisible(button) &&
                        /Continue to secure payment|Continue|Select seats|Choose seats|Choose your seat|Skip to payment/i.test(text) &&
                        !button.disabled &&
                        button.getAttribute('aria-disabled') !== 'true';
                });

                const hasApplicationError = [...document.querySelectorAll('[role=""dialog""], [aria-modal=""true""]')]
                    .some((dialog) => isVisible(dialog) &&
                        /Uh oh|something.?s not right|unable to book|encountered an error/i.test(dialog.innerText || ''));

                return hasSubmitCandidate || hasApplicationError;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });

        string? applicationErrorText = await GetApplicationErrorTextAsync();
        if (!applicationErrorText.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException($"Passenger details page showed an application error before submit became available: {applicationErrorText}");
        }
    }

    private async Task<string> MarkSubmitButtonAsync()
    {
        string? chosenText = await Page.EvaluateAsync<string?>(
            @"() => {
                document
                    .querySelectorAll('[data-eapi-playwright-target=""pax-submit""]')
                    .forEach((element) => element.removeAttribute('data-eapi-playwright-target'));

                const normalise = (value) => (value || '').replace(/\s+/g, ' ').trim();
                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 &&
                        rect.height > 0 &&
                        style.display !== 'none' &&
                        style.visibility !== 'hidden' &&
                        style.pointerEvents !== 'none';
                };

                const candidates = [...document.querySelectorAll('button')]
                    .map((button, index) => ({ button, index, text: normalise(button.innerText || '') }))
                    .filter((candidate) => isVisible(candidate.button) &&
                        /Continue to secure payment|Continue|Select seats|Choose seats|Choose your seat|Skip to payment/i.test(candidate.text) &&
                        !candidate.button.disabled &&
                        candidate.button.getAttribute('aria-disabled') !== 'true')
                    .map((candidate) => {
                        let score = 0;
                        if (candidate.button.closest('article, aside')) score += 10;
                        if (candidate.button.closest('header')) score += 5;
                        if (candidate.button.closest('form')) score -= 5;
                        if (candidate.button.closest('footer')) score -= 2;
                        if (/Continue to secure payment/i.test(candidate.text)) score -= 5;
                        return { ...candidate, score };
                    })
                    .sort((left, right) => left.score - right.score || right.index - left.index);

                const picked = candidates[0];
                if (!picked) {
                    return null;
                }

                picked.button.scrollIntoView({ block: 'center', inline: 'center' });
                picked.button.setAttribute('data-eapi-playwright-target', 'pax-submit');
                return picked.text;
            }");

        if (chosenText.IsNullOrWhiteSpace())
        {
            string summary = await GetPageSummaryAsync();
            throw new InvalidOperationException($"No enabled passenger submit button could be targeted. {summary}");
        }

        return chosenText;
    }

    private async Task<string> MarkSeatSelectionButtonAsync()
    {
        string? chosenText = await Page.EvaluateAsync<string?>(
            @"() => {
                document
                    .querySelectorAll('[data-eapi-playwright-target=""pax-submit""]')
                    .forEach((element) => element.removeAttribute('data-eapi-playwright-target'));

                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden' &&
                        style.pointerEvents !== 'none';
                };
                const picked = [...document.querySelectorAll('button')]
                    .map((button, index) => ({ button, index, text: (button.innerText || '').replace(/\s+/g, ' ').trim() }))
                    .filter((candidate) => isVisible(candidate.button) &&
                        /Select seats|Choose seats|Choose your seat/i.test(candidate.text) &&
                        !candidate.button.disabled && candidate.button.getAttribute('aria-disabled') !== 'true')
                    .sort((left, right) => right.index - left.index)[0];

                if (!picked) return null;
                picked.button.scrollIntoView({ block: 'center', inline: 'center' });
                picked.button.setAttribute('data-eapi-playwright-target', 'pax-submit');
                return picked.text;
            }");

        if (chosenText.IsNullOrWhiteSpace())
        {
            string summary = await GetPageSummaryAsync();
            throw new InvalidOperationException($"No enabled passenger Select seats button could be targeted. {summary}");
        }

        return chosenText;
    }
}





