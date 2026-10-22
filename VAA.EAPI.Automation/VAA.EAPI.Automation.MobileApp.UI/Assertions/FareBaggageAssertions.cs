using System.Globalization;
using System.Text.RegularExpressions;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Text;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Assertions;

internal static class FareBaggageAssertions
{
    private const string DimensionsPattern = @"\b(?<a>\d+(?:\.\d+)?)\s*[x×]\s*(?<b>\d+(?:\.\d+)?)\s*[x×]\s*(?<c>\d+(?:\.\d+)?)\s*(?<unit>cm|in)\b";
    internal static string AssertMatches(FareBaggageObservation observation)
    {
        Assert.False(observation.FareLabel.IsNullOrWhiteSpace());
        Assert.Equal(observation.FareLabel.Trim(), observation.SummaryFareLabel.Trim(), ignoreCase: true);
        Assert.NotEmpty(observation.BenefitLabels);
        Assert.NotEmpty(observation.BaggageAllowances);
        List<string> baggageBenefits = observation.BenefitLabels.Where(label => Category(label) is not null).ToList();
        Assert.NotEmpty(baggageBenefits);
        foreach (string allowance in observation.BaggageAllowances)
        {
            Assert.NotNull(Category(allowance));
            Assert.True(IsExcluded(allowance) || Count(allowance) > 0,
                $"Baggage allowance has neither a positive quantity nor an explicit exclusion: '{allowance}'.");
        }

        int countChecks = 0;
        int weightChecks = 0;
        int dimensionChecks = 0;
        foreach (string benefit in baggageBenefits)
        {
            Assert.True(benefit.StartsWith("Included,", StringComparison.OrdinalIgnoreCase) ||
                benefit.StartsWith("Not included,", StringComparison.OrdinalIgnoreCase),
                $"Fare benefit has no explicit inclusion state: '{benefit}'.");
            List<string> matches = observation.BaggageAllowances
                .Where(row => Category(row) == Category(benefit)).ToList();
            // Summary lists returned allowances, not every Brand benefit. An explicit
            // fare exclusion may therefore be omitted, but must never become included.
            if (IsExcluded(benefit) && matches.Count == 0)
            {
                continue;
            }
            Assert.True(matches.Count == 1,
                $"Fare '{observation.FareLabel}' benefit '{benefit}' requires exactly one {Category(benefit)} allowance; " +
                $"found {matches.Count}. Summary allowances: [{string.Join("; ", observation.BaggageAllowances)}].");
            string allowance = matches[0];
            Assert.True(IsExcluded(benefit) == IsExcluded(allowance),
                $"Baggage inclusion changed between fare '{benefit}' and Summary '{allowance}'.");
            if (!IsExcluded(benefit))
            {
                if (Count(benefit) is int expectedCount)
                {
                    Assert.Equal(expectedCount, Count(allowance));
                    countChecks++;
                }

                if (Weight(benefit) is string expectedWeight)
                {
                    Assert.Equal(expectedWeight, Weight(allowance));
                    weightChecks++;
                }

                // Compare dimensions only when the selected fare actually advertises
                // them. Never invent carrier limits from a generic baggage template.
                if (Dimensions(benefit) is string expectedDimensions)
                {
                    Assert.Equal(expectedDimensions, Dimensions(allowance));
                    dimensionChecks++;
                }
            }
        }
        int summaryDimensionRows = observation.BaggageAllowances.Count(row => Dimensions(row) is not null);
        return $"Baggage retention checks: inclusion={baggageBenefits.Count}, quantity={countChecks}, " +
            $"weight={weightChecks}, three-axis dimensions={dimensionChecks}. " +
            $"Summary rows displaying three-axis dimensions={summaryDimensionRows}. " +
            (dimensionChecks == 0
                ? "Dimension retention NOT EXERCISED: the selected included fare benefits supplied no three-axis dimensions."
                : "Advertised three-axis dimensions and units matched Summary.");
    }

    private static bool IsExcluded(string text)
        => text.Contains("not included", StringComparison.OrdinalIgnoreCase);

    internal static string? Category(string text)
    {
        if (RegexCompiled.IsMatch(text, @"\b(checked(?:[ -]in)?|hold)\s*(?:bags?|baggage|luggage)\b", RegexOptions.IgnoreCase))
        {
            return "checked";
        }

        return RegexCompiled.IsMatch(text, @"\b(carry[ -]?on|hand|cabin)\s*(?:bags?|baggage|luggage)\b", RegexOptions.IgnoreCase)
            ? "carryOn" : null;
    }

    internal static int? Count(string text)
    {
        // A dimension such as 56 x 36 x 23cm is not a quantity of 56 bags.
        text = RegexCompiled.Replace(text, DimensionsPattern, string.Empty, RegexOptions.IgnoreCase);
        Match match = RegexCompiled.Match(text,
            @"\b(?<count>\d+)\s*(?:[x×]|(?:checked(?:[ -]in)?|hold|carry[ -]?on|hand|cabin)\s*(?:bags?|baggage|luggage)|bags?)\b",
            RegexOptions.IgnoreCase);
        return match.Success ? int.Parse(match.Groups["count"].Value, CultureInfo.InvariantCulture) : null;
    }

    internal static string? Weight(string text)
    {
        Match match = RegexCompiled.Match(text, @"\b(?<weight>\d+(?:\.\d+)?)\s*(?<unit>kg|lbs?)\b", RegexOptions.IgnoreCase);
        return match.Success
            ? decimal.Parse(match.Groups["weight"].Value, CultureInfo.InvariantCulture).ToString("G29", CultureInfo.InvariantCulture) +
                match.Groups["unit"].Value.ToLowerInvariant().TrimEnd('s')
            : null;
    }

    internal static string? Dimensions(string text)
    {
        Match match = RegexCompiled.Match(text, DimensionsPattern, RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        return string.Join("x", new[] { "a", "b", "c" }.Select(axis =>
            decimal.Parse(match.Groups[axis].Value, CultureInfo.InvariantCulture)
                .ToString("G29", CultureInfo.InvariantCulture))) + match.Groups["unit"].Value.ToLowerInvariant();
    }
}
