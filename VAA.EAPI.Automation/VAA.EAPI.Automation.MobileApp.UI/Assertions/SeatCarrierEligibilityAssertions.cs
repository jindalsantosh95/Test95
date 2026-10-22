using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Assertions;

internal static class SeatCarrierEligibilityAssertions
{
    internal static bool HasSeatSupportedLeg(ConnectingFlightDetails details)
        => details.Legs.Any(leg =>
            CarrierMatches(leg.OperatingCarrier, "VS", "Virgin Atlantic") ||
            CarrierMatches(leg.OperatingCarrier, "DL", "Delta Air Lines"));

    private static bool CarrierMatches(string carrier, string code, string name)
    {
        string value = carrier.Trim();
        const string prefix = "Operated by ";
        if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            value = value[prefix.Length..].Trim();
        return value.Is(code) || value.Is(name);
    }

    internal static bool AllLegsAreKnownUnsupported(ConnectingFlightDetails details)
        => details.Legs.Count > 0 && details.Legs.All(leg => IsKnownUnsupported(leg.OperatingCarrier));

    internal static bool IsKnownUnsupported(string? carrier)
    {
        string value = (carrier ?? string.Empty).Trim();
        const string prefix = "Operated by ";
        if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            value = value[prefix.Length..].Trim();
        // Explicit identities only: unknown/missing labels cannot prove exclusion of VS/DL.
        return value.Is("AF") || value.Is("Air France") || value.Is("KL") ||
            value.Is("KLM") || value.Is("KLM Royal Dutch Airlines");
    }
}
