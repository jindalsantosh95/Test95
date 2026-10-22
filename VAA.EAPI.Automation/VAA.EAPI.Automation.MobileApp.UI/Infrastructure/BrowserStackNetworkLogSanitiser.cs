using System.Text.Json;

using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

internal static class BrowserStackNetworkLogSanitiser
{
    private static readonly string[] CorrelationHeaderNames =
    [
        "x-correlation-id",
        "x-request-id",
        "traceparent",
        "api-correlation-id",
        "api-operation-id",
        "api-session-id",
    ];

    public static IReadOnlyList<string> Summarise(string harJson)
    {
        List<string> summaries = [];
        using JsonDocument document = JsonDocument.Parse(harJson);
        if (!TryGetEntries(document.RootElement, out JsonElement entries))
        {
            return summaries;
        }

        foreach (JsonElement entry in entries.EnumerateArray())
        {
            if (!entry.TryGetProperty("request", out JsonElement request) ||
                !entry.TryGetProperty("response", out JsonElement response))
            {
                continue;
            }

            string method = ReadSafeString(request, "method") ?? "unknown";
            string? urlValue = ReadString(request, "url");
            int status = response.TryGetProperty("status", out JsonElement statusElement) &&
                         statusElement.TryGetInt32(out int statusValue)
                ? statusValue
                : 0;
            string operation = ReadGraphQlOperation(request);
            IReadOnlyList<string> backendCodes = ReadGraphQlErrorCodes(response);
            if (!Uri.TryCreate(urlValue, UriKind.Absolute, out Uri? uri) ||
                (status < 400 && backendCodes.Count == 0))
            {
                continue;
            }

            string correlation = ReadCorrelation(request);
            string responseCorrelation = ReadCorrelation(response);
            string path = uri.AbsolutePath.Contains("graphql", StringComparison.OrdinalIgnoreCase)
                ? SafePath(uri.AbsolutePath)
                : "redacted";
            summaries.Add(
                $"method={method}; host={ReadSafeValue(uri.Host) ?? "unavailable"}; path={path}; " +
                $"status={status}; operation={operation}; backendCodes={FormatCodes(backendCodes)}; " +
                $"request-correlation={correlation}; response-correlation={responseCorrelation}");
        }

        return summaries;
    }

    private static bool TryGetEntries(JsonElement root, out JsonElement entries)
    {
        entries = default;
        return root.TryGetProperty("log", out JsonElement log) &&
               log.TryGetProperty("entries", out entries) &&
               entries.ValueKind == JsonValueKind.Array;
    }

    private static string ReadCorrelation(JsonElement request)
    {
        if (!request.TryGetProperty("headers", out JsonElement headers) ||
            headers.ValueKind != JsonValueKind.Array)
        {
            return "unavailable";
        }

        List<string> identifiers = [];
        foreach (JsonElement header in headers.EnumerateArray())
        {
            string? name = ReadString(header, "name");
            if (name is null || !CorrelationHeaderNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            string? value = ReadSafeString(header, "value");
            if (name.StartsWith("api-", StringComparison.OrdinalIgnoreCase) &&
                !Guid.TryParseExact(value, "D", out _))
            {
                continue;
            }
            if (value is not null)
            {
                identifiers.Add($"{name.ToLowerInvariant()}:{value}");
            }
        }

        return identifiers.Count == 0 ? "unavailable" : string.Join(',', identifiers.Distinct().Take(10));
    }

    private static string ReadGraphQlOperation(JsonElement request)
    {
        string? body = ReadNestedText(request, "postData");
        if (body is null)
        {
            return "unavailable";
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            return ReadSafeString(document.RootElement, "operationName") ?? "unavailable";
        }
        catch (JsonException)
        {
            return "unavailable";
        }
    }

    private static IReadOnlyList<string> ReadGraphQlErrorCodes(JsonElement response)
    {
        string? body = ReadNestedText(response, "content");
        if (body is null)
        {
            return [];
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty("errors", out JsonElement errors) ||
                errors.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            List<string> codes = [];
            foreach (JsonElement error in errors.EnumerateArray())
            {
                if (!error.TryGetProperty("extensions", out JsonElement extensions))
                {
                    continue;
                }

                string? code = ReadSafeString(extensions, "code");
                if (code is not null && !codes.Contains(code, StringComparer.Ordinal))
                {
                    codes.Add(code);
                }
            }

            return codes;
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string? ReadNestedText(JsonElement parent, string propertyName) =>
        parent.TryGetProperty(propertyName, out JsonElement nested)
            ? ReadString(nested, "text")
            : null;

    private static string FormatCodes(IReadOnlyList<string> codes) =>
        codes.Count == 0 ? "unavailable" : string.Join(',', codes.Take(10));

    private static string? ReadSafeString(JsonElement element, string propertyName)
    {
        string? value = ReadString(element, propertyName);
        return value is null ? null : ReadSafeValue(value);
    }

    private static string? ReadString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement property) &&
        property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static string SafePath(string value)
    {
        string safe = new(value
            .Take(256)
            .Where(character => char.IsAsciiLetterOrDigit(character) || character is '/' or '.' or '_' or '-')
            .ToArray());
        return safe.IsNullOrWhiteSpace() ? "unavailable" : safe;
    }

    private static string? ReadSafeValue(string value) =>
        value.Length is > 0 and <= 128 && value.All(character =>
            char.IsAsciiLetterOrDigit(character) || character is '.' or '_' or '-')
            ? value
            : null;
}
