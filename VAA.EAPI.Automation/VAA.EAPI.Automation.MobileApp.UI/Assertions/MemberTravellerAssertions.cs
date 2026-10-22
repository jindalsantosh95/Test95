using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Assertions;

internal static class MemberTravellerAssertions
{
    private static readonly IReadOnlyDictionary<string, string> Placeholders = new Dictionary<string, string>
    {
        ["firstName"] = "First name",
        ["lastName"] = "Last name",
        ["dateOfBirth"] = "Date of birth",
        ["gender"] = "Select gender",
        ["membership"] = "Membership number",
        ["email"] = "Email address",
        ["telephone"] = "Telephone number",
    };

    internal static string Value(IReadOnlyDictionary<string, string> fields, string key)
    {
        if (!fields.TryGetValue(key, out string? value) || value is null)
        {
            throw new InvalidOperationException($"Passenger evidence is missing '{key}'.");
        }
        return value.Trim().Is(Placeholders[key]) ? string.Empty : value.Trim();
    }

    internal static void MatchesMember(IReadOnlyDictionary<string, string> fields, LoyaltyAccount account)
    {
        // Boolean assertions deliberately avoid writing leased account values into reports.
        Assert.True(Value(fields, "firstName").Is(account.FirstName), "Lead traveller first name does not match the leased account.");
        Assert.True(Value(fields, "lastName").Is(account.LastName), "Lead traveller surname does not match the leased account.");
        Assert.True(Value(fields, "membership").Is(account.FlyingClubNumber), "Lead traveller membership does not match the leased account.");
        foreach (string key in Placeholders.Keys)
        {
            Assert.False(Value(fields, key).IsNullOrWhiteSpace(), $"Member profile did not populate '{key}'.");
        }
    }

    internal static void Cleared(IReadOnlyDictionary<string, string> fields)
    {
        foreach (string key in Placeholders.Keys)
        {
            Assert.True(Value(fields, key).IsNullOrWhiteSpace(), $"Profile-owned '{key}' was not cleared.");
        }
    }

    internal static void Retained(IReadOnlyDictionary<string, string> expected, IReadOnlyDictionary<string, string> actual)
    {
        foreach (string key in Placeholders.Keys)
        {
            Assert.True(Value(expected, key).Is(Value(actual, key)), $"Replacement '{key}' did not survive save/reopen.");
        }
        Assert.True(Value(actual, "membership").IsNullOrWhiteSpace(), "Member number leaked into the replacement traveller.");
        foreach (string key in Placeholders.Keys.Where(key => key != "membership"))
        {
            Assert.False(Value(actual, key).IsNullOrWhiteSpace(), $"Replacement '{key}' is empty.");
        }
    }
}
