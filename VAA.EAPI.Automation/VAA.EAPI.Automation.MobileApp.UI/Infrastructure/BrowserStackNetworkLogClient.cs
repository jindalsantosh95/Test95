using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

internal sealed class BrowserStackNetworkLogClient
{
    private static readonly Uri ApiBaseUri = new("https://api-cloud.browserstack.com/app-automate/");
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public BrowserStackNetworkLogClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<IReadOnlyList<string>> GetSafeFailureSummariesAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        string userName = _config["BrowserStack:Username"] ?? string.Empty;
        string accessKey = _config["BrowserStack:AccessKey"] ?? string.Empty;
        if (userName.IsNullOrWhiteSpace() || accessKey.IsNullOrWhiteSpace())
        {
            return [];
        }

        using HttpRequestMessage sessionRequest = CreateRequest(
            $"sessions/{Uri.EscapeDataString(sessionId)}.json", userName, accessKey);
        using HttpResponseMessage sessionResponse = await _httpClient.SendAsync(sessionRequest, cancellationToken);
        if (!sessionResponse.IsSuccessStatusCode)
        {
            return [];
        }

        string sessionJson = await sessionResponse.Content.ReadAsStringAsync(cancellationToken);
        string? buildId = ReadBuildId(sessionJson);
        if (buildId.IsNullOrWhiteSpace())
        {
            return [];
        }

        string logsPath =
            $"builds/{Uri.EscapeDataString(buildId!)}/sessions/{Uri.EscapeDataString(sessionId)}/networklogs";
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            using HttpRequestMessage logsRequest = CreateRequest(logsPath, userName, accessKey);
            using HttpResponseMessage logsResponse = await _httpClient.SendAsync(logsRequest, cancellationToken);
            if (logsResponse.IsSuccessStatusCode)
            {
                string harJson = await logsResponse.Content.ReadAsStringAsync(cancellationToken);
                IReadOnlyList<string> summaries;
                try
                {
                    summaries = BrowserStackNetworkLogSanitiser.Summarise(harJson);
                }
                catch (JsonException)
                {
                    return [];
                }

                if (summaries.Count > 0 || attempt == 3)
                {
                    return summaries;
                }
            }

            if (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
            }
        }

        return [];
    }

    private static HttpRequestMessage CreateRequest(string relativePath, string userName, string accessKey)
    {
        HttpRequestMessage request = new(HttpMethod.Get, new Uri(ApiBaseUri, relativePath));
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{accessKey}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        return request;
    }

    private static string? ReadBuildId(string sessionJson)
    {
        using JsonDocument document = JsonDocument.Parse(sessionJson);
        JsonElement root = document.RootElement;
        JsonElement session = root.TryGetProperty("automation_session", out JsonElement nested)
            ? nested
            : root;
        return session.TryGetProperty("build_hashed_id", out JsonElement buildId) &&
               buildId.ValueKind == JsonValueKind.String
            ? buildId.GetString()
            : null;
    }
}
