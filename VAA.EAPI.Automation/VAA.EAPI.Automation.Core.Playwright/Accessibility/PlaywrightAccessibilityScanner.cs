using System.Text.Json;
using global::Deque.AxeCore.Commons;
using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.Playwright.Accessibility;

public sealed class PlaywrightAccessibilityScanner
{
    private readonly IAxeScriptProvider _scriptProvider;

    public PlaywrightAccessibilityScanner() : this(new BundledAxeScriptProvider())
    {
    }

    internal PlaywrightAccessibilityScanner(IAxeScriptProvider scriptProvider)
    {
        _scriptProvider = scriptProvider;
    }

    public async Task<AccessibilityScanResult> ScanAsync(IPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        await page.AddScriptTagAsync(new PageAddScriptTagOptions { Content = _scriptProvider.GetScript() });

        JsonElement axeResult = await page.EvaluateAsync<JsonElement>(
            "async () => await axe.run(document, { runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'] } })");

        List<AccessibilityViolation> violations = [];
        if (axeResult.TryGetProperty("violations", out JsonElement violationElements))
        {
            foreach (JsonElement violationElement in violationElements.EnumerateArray())
            {
                violations.Add(ParseViolation(violationElement));
            }
        }

        return new AccessibilityScanResult(page.Url, violations);
    }

    private static AccessibilityViolation ParseViolation(JsonElement violation)
    {
        List<string> targets = [];
        if (violation.TryGetProperty("nodes", out JsonElement nodes))
        {
            foreach (JsonElement node in nodes.EnumerateArray())
            {
                if (!node.TryGetProperty("target", out JsonElement target))
                {
                    continue;
                }

                foreach (JsonElement selector in target.EnumerateArray())
                {
                    string? value = selector.GetString();
                    if (value is not null && !value.IsNullOrWhiteSpace())
                    {
                        targets.Add(value);
                    }
                }
            }
        }

        return new AccessibilityViolation(
            ReadString(violation, "id"),
            ReadString(violation, "impact"),
            ReadString(violation, "description"),
            ReadString(violation, "help"),
            ReadString(violation, "helpUrl"),
            targets);
    }

    private static string ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out JsonElement property)
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }
}

public sealed record AccessibilityScanResult(string Url, IReadOnlyList<AccessibilityViolation> Violations)
{
    public IReadOnlyList<AccessibilityViolation> SeriousOrCriticalViolations =>
        Violations
            .Where(violation =>
                violation.Impact.Is("serious") || violation.Impact.Is("critical"))
            .ToArray();
}

public sealed record AccessibilityViolation(
    string RuleId,
    string Impact,
    string Description,
    string Help,
    string HelpUrl,
    IReadOnlyList<string> Targets);
