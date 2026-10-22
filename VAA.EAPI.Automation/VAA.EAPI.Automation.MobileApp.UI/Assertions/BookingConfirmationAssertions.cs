using System.Globalization;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.MobileApp.UI.Assertions;

internal static class BookingConfirmationAssertions
{
    internal static void AssertTravellers(
        IEnumerable<string?> observedValues, IReadOnlyList<string> expectedNames)
    {
        Assert.True(expectedNames.Count > 0 && expectedNames.All(name => !name.IsNullOrWhiteSpace()),
            "Confirmation requires nonempty expected traveller identities.");
        string expected = $"({expectedNames.Count.ToString(CultureInfo.InvariantCulture)}) " +
            string.Join(", ", expectedNames.Select(Normalize));
        string[] values = observedValues.Where(value => !value.IsNullOrWhiteSpace())
            .Select(value => Normalize(value ?? string.Empty)).ToArray();
        Assert.True(values.Length > 0, "Confirmation traveller identity evidence is missing.");
        // Compare complete values, not substrings; never print leased member names on failure.
        Assert.True(values.All(value => value.Is(expected)),
            "Confirmation traveller count/names do not match the submitted travellers, or native values conflict.");
    }

    private static string Normalize(string value)
        => string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
