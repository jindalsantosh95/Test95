using System.Globalization;
using System.Text.RegularExpressions;
using VAA.EAPI.Automation.Core.Text;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Assertions;

internal static class BackendBaggageAssertions
{
    internal static string AssertMatches(IReadOnlyList<BackendBaggageAllowance> expected, IReadOnlyList<string> actual)
    {
        Assert.NotEmpty(expected);
        Assert.Equal(expected.Count, actual.Count);
        int dimensionsChecked = 0;
        foreach (BackendBaggageAllowance allowance in expected)
        {
            string row = Assert.Single(actual, value => FareBaggageAssertions.Category(value) == allowance.Category);
            if (allowance.Count == 0)
                Assert.Contains("not included", row, StringComparison.OrdinalIgnoreCase);
            else
            {
                Assert.DoesNotContain("not included", row, StringComparison.OrdinalIgnoreCase);
                Assert.Equal(allowance.Count, FareBaggageAssertions.Count(row));
                Assert.Equal(allowance.Weight, FareBaggageAssertions.Weight(row));
            }
            string? dimensions = FareBaggageAssertions.Dimensions(row);
            if (dimensions is null)
            {
                Match linear = RegexCompiled.Match(row, @"\b(?<value>\d+(?:\.\d+)?)\s*(?<unit>cm|in)\b", RegexOptions.IgnoreCase);
                if (linear.Success)
                    dimensions = decimal.Parse(linear.Groups["value"].Value, CultureInfo.InvariantCulture).ToString("G29", CultureInfo.InvariantCulture) + linear.Groups["unit"].Value.ToLowerInvariant();
            }
            Assert.Equal(allowance.Dimensions, dimensions);
            if (allowance.Dimensions is not null)
                dimensionsChecked++;
        }
        Assert.True(dimensionsChecked > 0, "The selected backend fare supplied no dimensions; dimension accuracy was not exercised.");
        return $"Backend-first matched-fare comparison: {expected.Count} allowance rows matched; {dimensionsChecked} dimension comparisons passed. No checkout.";
    }
}
