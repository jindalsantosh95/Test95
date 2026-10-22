using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Pages;

public sealed class ConfirmationPage : BasePage
{
    private static readonly Regex PnrFormat = new("^[A-Z0-9]{6}$", RegexOptions.Compiled);

    public ConfirmationPage(IPage page)
        : base(page)
    {
    }

    public async Task<ConfirmationDetails> WaitForConfirmationAsync(int timeoutMs, int expectedFlights = 1)
    {
        if (expectedFlights < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedFlights));
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        ConfirmationDetails latest = new();
        string stableFingerprint = string.Empty;
        int stablePolls = 0;

        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            latest = await ReadDetailsAsync();
            if (!latest.PaymentError.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException($"Payment did not confirm: {latest.PaymentError}");
            }

            bool complete = !latest.IsLoading &&
                            PnrFormat.IsMatch(latest.BookingReference) &&
                            latest.FlightCount >= expectedFlights;
            if (complete)
            {
                string fingerprint = string.Join(
                    "|",
                    latest.BookingReference,
                    latest.Surname,
                    latest.PassengerDetails,
                    latest.PaymentMethod,
                    latest.VoucherBenefit,
                    latest.PointsUsed,
                    latest.FlightCount,
                    latest.FlightSummary);
                stablePolls = fingerprint.Equals(stableFingerprint, StringComparison.Ordinal) ? stablePolls + 1 : 1;
                stableFingerprint = fingerprint;

                if (stablePolls >= 3)
                {
                    return latest;
                }
            }
            else
            {
                stableFingerprint = string.Empty;
                stablePolls = 0;
            }

            await Task.Delay(1_000);
        }

        throw new TimeoutException(
            $"Confirmation did not finish loading within {timeoutMs / 1000} seconds. " +
            $"PNR='{latest.BookingReference}', expectedFlights={expectedFlights}, " +
            $"renderedFlights={latest.FlightCount}, confirmationPending={latest.IsLoading}. {latest.BodySummary}");
    }

    private async Task<ConfirmationDetails> ReadDetailsAsync()
    {
        return await Page.EvaluateAsync<ConfirmationDetails>(
            @"() => {
                const clean = (value) => (value || '').replace(/\s+/g, ' ').trim();
                const isVisible = (element) => {
                    if (!element) return false;
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden' &&
                        Number(style.opacity || 1) > 0;
                };
                const fromInput = (name) => clean(document.querySelector(`form[action*='findPnr'] input[name='${name}']`)?.value);
                const rowValue = (title) => {
                    const titleElement = [...document.querySelectorAll('p, span, div')]
                        .find((element) => new RegExp(`^${title}$`, 'i').test(clean(element.textContent)));
                    return clean(titleElement?.nextElementSibling?.textContent);
                };
                const body = clean(document.body?.innerText);
                const visibleText = [...document.querySelectorAll('main, [role=""main""], [role=""alert""], h1, h2, h3, p, div')]
                    .filter(isVisible)
                    .map((element) => clean(element.innerText || element.textContent))
                    .filter(Boolean);
                const errorText = visibleText
                    .filter((text) => /Uh oh|something.?s not right|unable to book|payment.*failed/i.test(text))
                    .sort((left, right) => left.length - right.length)[0] || '';
                const flightMatches = [...body.matchAll(/Flight\s+(\d+)\s+of\s+(\d+)/gi)];
                // Each direct slice is rendered as Flight 1 of 1, so a return has two
                // separate matches even though the maximum denominator remains one.
                const flightCount = Math.max(
                    flightMatches.length,
                    flightMatches.reduce((maximum, match) => Math.max(maximum, Number(match[2]) || 0), 0));
                const flightHeadings = [...document.querySelectorAll('h1, h2, h3, h4, p, span')]
                    .map((element) => clean(element.textContent))
                    .filter((text) => /^Flight \d+ of \d+\s*[-–—]/i.test(text));
                const loaderSelector = '[data-testid=""search-loader""], [data-testid=""tailfin-loader-component""], ' +
                    '[data-testid=""interstitial-component""]';
                const visibleLoader = [...document.querySelectorAll(loaderSelector)].some(isVisible);
                const visibleInterstitial = visibleText.some((text) =>
                    /Confirming your flight booking|Almost there/i.test(text));
                const visibleConfirmation = visibleText.some((text) =>
                    /Your flight is booked/i.test(text));
                return {
                    BookingReference: fromInput('confirmationNo') || rowValue('Booking Reference'),
                    Surname: fromInput('lastName'),
                    PassengerDetails: rowValue('Who\'s going') || (body.match(/Who'?s going\s+([^]+?)(?=Payment|Flight \d|$)/i)?.[1] || ''),
                    PaymentMethod: rowValue('Payment'),
                    VoucherBenefit: (body.match(/Vouchers? redeemed\s+[\d,]+\s+Virgin Points/i)?.[0] || '') ||
                        rowValue('Voucher benefit') || rowValue('Vouchers redeemed') ||
                        (body.match(/(?:Companion|Upgrade)[^]+?(?:Voucher Points|points covered)[^\n]*/i)?.[0] || ''),
                    PointsUsed: (body.match(/(?:Virgin Points used|Vouchers? redeemed)\s+[\d,]+\s+Virgin Points/i)?.[0] || '') ||
                        rowValue('Virgin Points used') || rowValue('Vouchers redeemed'),
                    FlightSummary: flightHeadings.join(' | '),
                    FlightCount: flightCount,
                    // Safari can retain a stale loader after the completed confirmation has rendered.
                    // The explicit booked heading is authoritative once the PNR and flights are present.
                    IsLoading: !visibleConfirmation && (visibleLoader || visibleInterstitial),
                    PaymentError: errorText.slice(0, 1200),
                    BodySummary: body.slice(0, 6000)
                };
            }");
    }
}

public sealed class ConfirmationDetails
{
    public string BookingReference { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string PassengerDetails { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = string.Empty;

    public string VoucherBenefit { get; set; } = string.Empty;

    public string PointsUsed { get; set; } = string.Empty;

    public string FlightSummary { get; set; } = string.Empty;

    public int FlightCount { get; set; }

    public bool IsLoading { get; set; }

    public string PaymentError { get; set; } = string.Empty;

    public string BodySummary { get; set; } = string.Empty;
}
