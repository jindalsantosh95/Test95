using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Helper;

public class TelemetryHelper : ITelemetryInitializer
{
    private static readonly HashSet<string> SensitiveQueryKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "key",
        "api_key",
        "apikey",
        "token",
        "access_token"
    };

    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is not DependencyTelemetry dependencyTelemetry)
        {
            return;
        }

        if (!Uri.TryCreate(dependencyTelemetry.Data, UriKind.Absolute, out var uri) || string.IsNullOrWhiteSpace(uri.Query))
        {
            return;
        }

        var sanitizedUri = BuildSanitizedUri(uri);
        dependencyTelemetry.Data = sanitizedUri;
    }

    private static string BuildSanitizedUri(Uri uri)
    {
        var redactedQuery = string.Join('&', uri.Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(RedactSensitiveQueryPart));

        return string.IsNullOrWhiteSpace(redactedQuery)
            ? uri.GetLeftPart(UriPartial.Path)
            : $"{uri.GetLeftPart(UriPartial.Path)}?{redactedQuery}";
    }

    private static string RedactSensitiveQueryPart(string queryPart)
    {
        var equalsIndex = queryPart.IndexOf('=');
        if (equalsIndex <= 0)
        {
            return queryPart;
        }

        var rawKey = queryPart[..equalsIndex];
        var decodedKey = Uri.UnescapeDataString(rawKey);

        if (!SensitiveQueryKeys.Contains(decodedKey))
        {
            return queryPart;
        }

        return $"{rawKey}=REDACTED";
    }
}
