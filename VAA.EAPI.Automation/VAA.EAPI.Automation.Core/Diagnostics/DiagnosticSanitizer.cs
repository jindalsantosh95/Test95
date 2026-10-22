using System.Text.RegularExpressions;

namespace VAA.EAPI.Automation.Core.Diagnostics;

public static class DiagnosticSanitizer
{
    private static readonly Regex UrlPattern = new(
        "https?://[^\\s\\\"'<>]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex AuthorizationPattern = new(
        "(?<keyQuote>[\\\"']?)(?<key>authorization|proxy-authorization)\\k<keyQuote>(?<separator>\\s*[:=]\\s*)(?:(?:basic|bearer|digest|apikey)\\s+)?(?:\\\"[^\\\"]*\\\"|'[^']*'|[^\\s,;}&]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex SensitiveValuePattern = new(
        "(?<keyQuote>[\\\"']?)(?<key>paymentReferenceId|paymentReference|cardNumber|cvv|securityCode|access_token|id_token|refresh_token|api[-_]?key|client[-_]?id|client[-_]?secret|password|token|ocp-apim-subscription-key|subscription-key|x-vaa_digitalfirst|x-delta_enterprisepaymentshpp_test)\\k<keyQuote>(?<separator>\\s*[:=]\\s*)(?:\\\"[^\\\"]*\\\"|'[^']*'|[^\\s,;}&]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex StandaloneAuthorizationPattern = new(
        "\\b(bearer|basic)\\s+[^\\s,;}&]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string Sanitize(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text ?? string.Empty;
        }

        string sanitized = UrlPattern.Replace(text, match => SanitizeUrl(match.Value));
        sanitized = AuthorizationPattern.Replace(
            sanitized,
            "${keyQuote}${key}${keyQuote}${separator}[REDACTED]");
        sanitized = SensitiveValuePattern.Replace(
            sanitized,
            "${keyQuote}${key}${keyQuote}${separator}[REDACTED]");
        return StandaloneAuthorizationPattern.Replace(sanitized, "$1 [REDACTED]");
    }

    public static string SanitizeUrl(string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            return "unavailable";
        }

        UriBuilder sanitizedUri = new(uri)
        {
            UserName = string.Empty,
            Password = string.Empty,
            Query = string.Empty,
            Fragment = string.Empty
        };
        return sanitizedUri.Uri.GetLeftPart(UriPartial.Path);
    }
}
