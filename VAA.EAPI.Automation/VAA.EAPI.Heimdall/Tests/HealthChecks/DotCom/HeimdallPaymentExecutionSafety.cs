using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.DotCom;

internal static class HeimdallPaymentExecutionSafety
{
    private static readonly HashSet<string> SupportedNonProductionEnvironments =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "dev",
            "sit",
            "si",
            "uat",
            "stg",
            "stage"
        };

    internal static bool RequiresHppValidationOnly(string? environment)
    {
        if (environment.Is("prod"))
        {
            return true;
        }

        if (!environment.IsNullOrWhiteSpace() &&
            SupportedNonProductionEnvironments.Contains(environment!))
        {
            return false;
        }

        throw new InvalidOperationException(
            "Heimdall payment execution requires env=prod or an explicitly supported non-production environment.");
    }
}
