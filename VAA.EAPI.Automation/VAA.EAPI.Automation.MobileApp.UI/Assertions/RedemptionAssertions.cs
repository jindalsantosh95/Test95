using System.Globalization;
using System.Text.RegularExpressions;
using VAA.EAPI.Automation.Core.Text;

namespace VAA.EAPI.Automation.MobileApp.UI.Assertions;

internal static class RedemptionAssertions
{
    internal static int ReadBalance(string text)
    {
        Match match = RegexCompiled.Match(text,
            @"^\s*(?:Virgin\s+)?(?:Points\s*:?\s*)?(?<balance>\d{1,3}(?:,\d{3})+|\d+)\s*(?:Virgin\s+)?(?:Points)?\s*$",
            RegexOptions.IgnoreCase);
        if (!match.Success || !int.TryParse(match.Groups["balance"].Value.Replace(",", string.Empty),
                NumberStyles.None, CultureInfo.InvariantCulture, out int balance))
        {
            throw new InvalidOperationException("The member screen did not expose an unambiguous whole-number points balance.");
        }
        return balance;
    }

    internal static void AssertAffordable(int cost, int balance)
    {
        Assert.True(cost > 0, $"Redemption must cost a positive number of points; observed {cost}.");
        Assert.True(cost <= balance, $"Redemption cost {cost:N0} exceeds live member balance {balance:N0}; do not proceed to payment.");
    }
}
