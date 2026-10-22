using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Text;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

/// <summary>Reads the local driver's existing log buffer; never persists raw logs.</summary>
internal static class IosAppLogEvidence
{
    private const int MaximumResponseBytes = 4 * 1024 * 1024;
    private static readonly HttpClient Client = new(new HttpClientHandler
    {
        AllowAutoRedirect = false,
        UseProxy = false,
    });

    internal static Uri Endpoint(string server, string session)
    {
        Uri uri = new(server, UriKind.Absolute);
        if (!uri.IsLoopback || uri.Scheme != Uri.UriSchemeHttp || uri.UserInfo.Length != 0 ||
            uri.Query.Length != 0 || uri.Fragment.Length != 0 || !Guid.TryParse(session, out Guid id))
        {
            throw new InvalidOperationException("iOS log collection requires a loopback HTTP Appium endpoint and valid session ID.");
        }
        return new Uri($"{uri.AbsoluteUri.TrimEnd('/')}/session/{id:D}/se/log");
    }

    internal static async Task<IReadOnlyList<string>> CollectAsync(
        string server, string session, string processName, DateTimeOffset since,
        HttpClient? client = null)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(8));
        using HttpRequestMessage request = new(HttpMethod.Post, Endpoint(server, session))
        {
            Content = new StringContent("{\"type\":\"syslog\"}", Encoding.UTF8, "application/json"),
        };
        using HttpResponseMessage response = await (client ?? Client).SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
        response.EnsureSuccessStatusCode();
        using Stream stream = await response.Content.ReadAsStreamAsync(timeout.Token);
        using MemoryStream buffer = new();
        byte[] block = new byte[8192];
        int count;
        while ((count = await stream.ReadAsync(block, timeout.Token)) > 0)
        {
            if (buffer.Length + count > MaximumResponseBytes)
            {
                throw new InvalidOperationException("iOS log buffer exceeded the diagnostic size limit.");
            }
            buffer.Write(block, 0, count);
        }
        return Sanitise(Encoding.UTF8.GetString(buffer.ToArray()), processName, since);
    }

    internal static IReadOnlyList<string> Sanitise(string json, string processName, DateTimeOffset since)
    {
        if (!RegexCompiled.IsMatch(processName, @"^[A-Za-z0-9_.-]{1,80}$"))
        {
            throw new InvalidOperationException("A simple iOS executable name is required for log filtering.");
        }
        JArray entries = JObject.Parse(json)["value"] as JArray
            ?? throw new InvalidOperationException("The driver did not return an iOS log array.");
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset lowerBound = since > now.AddMinutes(-5) ? since : now.AddMinutes(-5);
        string processPattern = @"^[^\r\n\[\]]{0,160}(?<![\w.-])" + Regex.Escape(processName)
            + @"(?:\([^\r\n()]{0,100}\))?\[\d+(?::[^\]\r\n]{1,30})?\]";
        List<string> summaries = [];
        foreach (JToken entry in entries.TakeLast(2000))
        {
            string message = entry.Value<string>("message") ?? string.Empty;
            if (message.Length > 16384 || !RegexCompiled.IsMatch(message, processPattern))
            {
                continue;
            }
            if (!long.TryParse(entry["timestamp"]?.ToString(), out long milliseconds) ||
                milliseconds < lowerBound.ToUnixTimeMilliseconds() || milliseconds > now.ToUnixTimeMilliseconds())
            {
                continue;
            }
            List<string> fields = [];
            Match status = RegexCompiled.Match(message, @"\b(?:statusCode|status|HTTP)[\""']?\s*[:= ]\s*(?<value>[45]\d{2})\b", RegexOptions.IgnoreCase);
            if (status.Success)
            {
                fields.Add($"httpStatus={status.Groups["value"].Value}");
            }
            foreach (string key in new[] { "api-correlation-id", "x-correlation-id", "correlationId", "api-operation-id", "api-session-id" })
            {
                Match identifier = RegexCompiled.Match(message, Regex.Escape(key)
                    + @"[\""']?\s*[:=]\s*[\""']?(?<value>[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})(?![\w-])", RegexOptions.IgnoreCase);
                if (identifier.Success)
                {
                    fields.Add($"{key}={identifier.Groups["value"].Value}");
                }
            }
            Match trace = RegexCompiled.Match(message,
                @"\btraceparent[\""']?\s*[:=]\s*[\""']?(?<value>00-[a-f0-9]{32}-[a-f0-9]{16}-[a-f0-9]{2})(?![\w-])", RegexOptions.IgnoreCase);
            if (trace.Success)
            {
                fields.Add($"traceparent={trace.Groups["value"].Value}");
            }
            // Closed vocabulary: arbitrary error messages/codes may themselves contain secrets.
            foreach (string code in new[] { "UNAUTHENTICATED", "FORBIDDEN", "INTERNAL_SERVER_ERROR", "BAD_USER_INPUT", "GRAPHQL_VALIDATION_FAILED" })
            {
                if (RegexCompiled.IsMatch(message, @"\b" + code + @"\b"))
                {
                    fields.Add($"recognisedCode={code}");
                }
            }
            if (fields.Count == 0 && RegexCompiled.IsMatch(message, @"\b(error|errorCode|failed|failure|exception)\b", RegexOptions.IgnoreCase))
            {
                fields.Add("signal=app-error; detail=omitted");
            }
            if (fields.Count > 0)
            {
                string timestamp = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).ToString("O", CultureInfo.InvariantCulture);
                summaries.Add($"{timestamp}; {string.Join("; ", fields)}");
            }
        }
        return summaries.Distinct(StringComparer.Ordinal).TakeLast(40).ToArray();
    }
}
