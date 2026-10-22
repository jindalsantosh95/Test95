using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Requests;

internal sealed class BackendSearchClient : IDisposable
{
    private readonly HttpClient _client;
    private readonly Uri _baseUri;
    private readonly string _environment;
    private readonly TimeSpan _timeout;

    internal BackendSearchClient(IConfiguration configuration, HttpMessageHandler? handler = null)
    {
        if (!configuration.GetValue<bool>("BackendValidation:Enabled"))
            throw new InvalidOperationException("Enable BackendValidation explicitly for the dedicated UI/backend test.");
        _environment = configuration["BackendValidation:Environment"] ?? string.Empty;
        if (!new[] { "DEV", "SIT", "UAT" }.Contains(_environment, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("BackendValidation:Environment must be DEV, SIT or UAT.");
        string profile = configuration["IOSLaunch:BuildProfile"] ?? string.Empty;
        if (!profile.StartsWith(_environment + "_", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("BackendValidation environment must match IOSLaunch:BuildProfile before any API request.");
        if (!Uri.TryCreate(configuration["BackendValidation:BaseUrl"], UriKind.Absolute, out Uri? baseUri) ||
            baseUri.Scheme != Uri.UriSchemeHttps || !baseUri.Host.Is("testresources.virginatlantic.com") ||
            !baseUri.IsDefaultPort || baseUri.AbsolutePath != "/" || baseUri.UserInfo.Length != 0 ||
            baseUri.Query.Length != 0 || baseUri.Fragment.Length != 0)
            throw new InvalidOperationException("BackendValidation:BaseUrl must be the HTTPS EAPI testresources origin without a path or credentials.");
        _baseUri = baseUri;
        int seconds = configuration.GetValue("BackendValidation:TimeoutSeconds", 30);
        if (seconds is < 1 or > 60)
            throw new InvalidOperationException("BackendValidation:TimeoutSeconds must be 1–60.");
        _timeout = TimeSpan.FromSeconds(seconds);
        // Request-layer isolation preserves TLS validation, bounded payloads and no redirects/logging.
        _client = new HttpClient(handler ?? new HttpClientHandler { AllowAutoRedirect = false })
        { Timeout = Timeout.InfiniteTimeSpan, MaxResponseContentBufferSize = 2 * 1024 * 1024 };
    }

    internal async Task<BackendFareExpectation> PrepareAsync(FlightSearch search, bool connectingPartner = false)
    {
        string payload = BackendSearchExpectation.SearchPayload(search);
        string searchBody = await SendAsync(HttpMethod.Post, $"{_environment.ToLowerInvariant()}/search/search", payload);
        (Guid basketId, string fareId) = BackendSearchExpectation.SelectEconomyFare(searchBody, connectingPartner);
        // Same BFF route used by Search's WebBFFHelper.BuildSummaryUrl. This prices only
        // the backend-owned basket, before the independent UI search starts.
        string summary = await SendAsync(HttpMethod.Get,
            $"{_environment.ToLowerInvariant()}/search/summary/{basketId:D}/{Uri.EscapeDataString(fareId)}");
        return BackendSearchExpectation.ReadSummary(summary, search, connectingPartner);
    }

    private async Task<string> SendAsync(HttpMethod method, string path, string? payload = null)
    {
        string correlationId = Guid.NewGuid().ToString("D");
        using HttpRequestMessage request = new(method, new Uri(_baseUri, path));
        request.Headers.Add("api-correlation-id", correlationId);
        request.Headers.Add("api-channel", "Monitoring");
        request.Headers.Add("userAgent", "MobileAppAutomation");
        if (payload is not null)
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        using CancellationTokenSource timeout = new(_timeout);
        try
        {
            using HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, timeout.Token);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException($"Backend search preparation returned HTTP {(int)response.StatusCode}; correlation {correlationId}. Check runner connectivity and environment. Body suppressed.");
            return await response.Content.ReadAsStringAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            throw new InvalidOperationException($"Backend preparation exceeded its fixed timeout; no retry. Correlation {correlationId}.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException($"Backend preparation transport failed; check runner VPN, DNS and TLS. Correlation {correlationId}.");
        }
    }

    public void Dispose() => _client.Dispose();
}
