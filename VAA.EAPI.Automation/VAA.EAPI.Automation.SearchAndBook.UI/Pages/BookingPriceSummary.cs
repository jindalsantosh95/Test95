using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Pages;

public sealed class BookingPriceSummary : BasePage
{
    public BookingPriceSummary(IPage page)
        : base(page)
    {
    }

    public async Task<PriceSummarySnapshot> CaptureAsync(int timeoutMs)
    {
        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        PriceSummarySnapshot snapshot = new();
        do
        {
            snapshot = await CaptureVisibleSummaryAsync();
            if (!snapshot.Text.IsNullOrWhiteSpace() && !snapshot.Total.IsNullOrWhiteSpace())
            {
                return snapshot;
            }

            await Task.Delay(500);
        }
        while (DateTime.UtcNow < deadline);

        throw new TimeoutException(
            $"A visible booking price summary was not available within {timeoutMs}ms. " +
            $"URL: {Page.Url}; last summary: {snapshot.Text}");
    }

    public async Task<PriceSummarySnapshot> WaitForChangeAsync(
        PriceSummarySnapshot previous,
        int timeoutMs)
    {
        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        PriceSummarySnapshot snapshot = new();
        do
        {
            snapshot = await CaptureVisibleSummaryAsync();
            bool hasPrice = !snapshot.Text.IsNullOrWhiteSpace() && !snapshot.Total.IsNullOrWhiteSpace();
            bool changed = snapshot.Total != previous.Total || snapshot.Points != previous.Points;
            if (hasPrice && changed)
            {
                return snapshot;
            }

            await Task.Delay(500);
        }
        while (DateTime.UtcNow < deadline);

        throw new TimeoutException(
            $"The visible price summary did not change within {timeoutMs}ms. " +
            $"Before: {previous.Text}; last summary: {snapshot.Text}; URL: {Page.Url}");
    }

    private async Task<PriceSummarySnapshot> CaptureVisibleSummaryAsync()
    {
        return await Page.EvaluateAsync<PriceSummarySnapshot>(
            @"() => {
                const isVisible = (element) => {
                    const rect = element.getBoundingClientRect();
                    const style = window.getComputedStyle(element);
                    return rect.width > 0 && rect.height > 0 &&
                        style.display !== 'none' && style.visibility !== 'hidden';
                };
                const parsePoints = (text) => {
                    const labelled = text.match(/([0-9][0-9,]*)\s*(?:Virgin\s*)?Points/i);
                    const compact = text.match(/\+\s*([0-9][0-9,]*)\b/);
                    return (labelled?.[1] || compact?.[1] || '').replace(/,/g, '');
                };
                const totalComponent = [...document.querySelectorAll('[data-testid=""breakdown-total-component""]')]
                    .find(isVisible);
                const totalText = (totalComponent?.innerText || '').replace(/\s+/g, ' ').trim();
                const rewardMoney = totalText.match(/([A-Z]{2,3}\$|[\u00A3$\u20AC])\s*([0-9][0-9,]*)\s*(?:\.\s*([0-9]{2}))?/i);
                const rewardPoints = parsePoints(totalText);
                if (rewardPoints && rewardMoney) {
                    return {
                        Text: totalText.slice(0, 1600),
                        Currency: rewardMoney[1] || '',
                        Total: `${rewardMoney[2]}.${rewardMoney[3] || '00'}`,
                        Points: rewardPoints
                    };
                }
                // The application renders the whole amount and decimal part in separate spans, e.g. ""US$394"" + "".50"".
                const splitTotal = totalText.match(/Total to pay\s*([A-Z]{2,3}\$|[\u00A3$\u20AC])?\s*([0-9][0-9,]*)\s*(?:\.\s*([0-9]{2}))?/i);
                if (splitTotal) {
                    return {
                        Text: totalText.slice(0, 1600),
                        Currency: splitTotal[1] || '',
                        Total: `${splitTotal[2]}.${splitTotal[3] || '00'}`,
                        Points: parsePoints(totalText)
                    };
                }
                const basketButton = [...document.querySelectorAll('[data-testid=""bookflow-basket-button""]')]
                    .find(isVisible);
                const basketText = (basketButton?.innerText || '').replace(/\s+/g, ' ').trim();
                const basketTotal = basketText.match(/([A-Z]{2,3}\$|[\u00A3$\u20AC])\s*([0-9][0-9,]*)\s*(?:\.\s*([0-9]{2}))?/i);
                if (basketTotal) {
                    return {
                        Text: `Basket total ${basketText}`,
                        Currency: basketTotal[1] || '',
                        Total: `${basketTotal[2]}.${basketTotal[3] || '00'}`,
                        Points: parsePoints(basketText)
                    };
                }
                const candidates = [...document.querySelectorAll('aside, section, div')]
                    .filter(isVisible)
                    .map((element) => ({
                        text: (element.innerText || '').replace(/\s+/g, ' ').trim(),
                        depth: (() => { let count = 0; let current = element; while (current.parentElement) { count++; current = current.parentElement; } return count; })()
                    }))
                    .filter((candidate) => /Price summary|Total to pay|Total price/i.test(candidate.text) &&
                        /(?:Total to pay|Total price)\s*(?:[A-Z]{2,3}\$|[£$€])?\s*[0-9][0-9,]*(?:\s*\.\s*\d{2})?/i.test(candidate.text))
                    .sort((left, right) => {
                        const leftScore = (/Price summary/i.test(left.text) ? 0 : 10) + left.text.length;
                        const rightScore = (/Price summary/i.test(right.text) ? 0 : 10) + right.text.length;
                        return leftScore - rightScore || right.depth - left.depth;
                    });
                const text = candidates[0]?.text || '';
                const totalMatch = text.match(/(?:Total to pay|Total price)\s*([A-Z]{2,3}\$|[£$€])?\s*([0-9][0-9,]*)\s*(?:\.\s*([0-9]{2}))?/i);
                return {
                    Text: text.slice(0, 1600),
                    Currency: totalMatch?.[1] || '',
                    Total: totalMatch ? `${totalMatch[2]}.${totalMatch[3] || '00'}` : '',
                    Points: parsePoints(text)
                };
            }");
    }

}

public sealed class PriceSummarySnapshot
{
    public string Text { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public string Total { get; set; } = string.Empty;

    public string Points { get; set; } = string.Empty;

    public override string ToString()
    {
        return Points.IsNullOrWhiteSpace()
            ? $"{Currency}{Total}"
            : $"{Points} points + {Currency}{Total}";
    }
}
