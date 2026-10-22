using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

internal sealed class BrowserStackSessionStatusClient(HttpClient client, IConfiguration config)
{
    // One metadata-only request after teardown; never replay any application action.
    internal async Task<bool> UpdateAsync(string sessionId, string status, string reason)
    {
        if (sessionId.Length != 40 || sessionId.Any(character => !Uri.IsHexDigit(character)) ||
            status is not ("passed" or "failed"))
            return false;
        string username = config["BrowserStack:Username"] ?? string.Empty;
        string key = config["BrowserStack:AccessKey"] ?? string.Empty;
        if (username.IsNullOrWhiteSpace() || key.IsNullOrWhiteSpace())
            return false;
        using HttpRequestMessage request = new(HttpMethod.Put,
            $"https://api-cloud.browserstack.com/app-automate/sessions/{sessionId}.json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{key}")));
        request.Content = new StringContent(JsonSerializer.Serialize(new
        {
            status,
            reason = reason.Length <= 255 ? reason : reason[..252] + "...",
        }), Encoding.UTF8, "application/json");
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
        try
        {
            using HttpResponseMessage response = await client.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException) { return false; }
        catch (OperationCanceledException) { return false; }
    }
}
