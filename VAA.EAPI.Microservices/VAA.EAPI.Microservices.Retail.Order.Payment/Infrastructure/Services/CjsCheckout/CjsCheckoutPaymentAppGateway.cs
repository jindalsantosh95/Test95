using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CjsCheckout;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CjsCheckout
{
    public sealed class CjsCheckoutPaymentAppGateway : ICjsCheckoutPaymentAppGateway
    {   
        // Cookies we are willing to surface back to upstream callers (as Set-Cookie).
        private static readonly string[] AllowedDownstreamCookieNames =
        [
            "mmb-session",
            "com.virginholidays.payment.v2"
        ];

        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<CjsCheckoutPaymentAppGateway> _logger;

        public CjsCheckoutPaymentAppGateway(IHttpConnector httpConnector, ILogger<CjsCheckoutPaymentAppGateway> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CjsCheckoutPaymentAppResult> StartPaymentSessionAsync(
            CjsCheckoutPaymentAppPayload request,            
            string? cookieHeader,
            CancellationToken cancellationToken)
        {
            var url = ConfigReader.Get(Constants.Environment.CjsCheckoutPaymentAppUrl);
            var appVersion = ConfigReader.Try(Constants.Environment.CjsCheckoutPaymentAppVersion, "1.0.0");

            var (result, _) = await SendFollowingRedirectsAsync(
                initialUrl: url,
                initialMethod: HttpMethod.Post,
                initialContent: Json.Serialize(request),
                cookieHeader: FilterCookieHeader(cookieHeader),
                appVersion: appVersion,
                cancellationToken);

            return result;
        }

        public async Task<CjsCheckoutPaymentAppResult> SetPaymentAmountAsync(
            decimal amount,
            IEnumerable<string> cookies,
            CancellationToken cancellationToken)
        {
            var url = ConfigReader.Get(Constants.Environment.CjsCheckoutPaymentAmountUrl);
            var appVersion = ConfigReader.Try(Constants.Environment.CjsCheckoutPaymentAppVersion, "1.0.0");

            var (result, rawStatusCode) = await SendFollowingRedirectsAsync(
                initialUrl: url,
                initialMethod: HttpMethod.Post,
                initialContent: Json.Serialize(new CjsCheckoutPaymentAmountPayload(amount)),
                cookieHeader: BuildCookieHeader(cookies),
                appVersion: appVersion,
                cancellationToken);

            return result;
        }

        private async Task<(CjsCheckoutPaymentAppResult Result, HttpStatusCode RawStatusCode)> SendFollowingRedirectsAsync(
            string initialUrl,
            HttpMethod initialMethod,
            string initialContent,
            string? cookieHeader,
            string? appVersion,
            CancellationToken cancellationToken)
        {
            var redirectCount = 0;
            var currentUrl = initialUrl;
            var currentMethod = initialMethod;
            var currentContent = initialContent;

            HttpResponseMessage response;
            var effectiveCookieHeader = cookieHeader;
            while (true)
            {
                var headers = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(effectiveCookieHeader))
                    headers["Cookie"] = effectiveCookieHeader;
                if (!string.IsNullOrWhiteSpace(appVersion))
                    headers["app-version"] = appVersion;

                response = await _httpConnector.SendAsync(
                    uri: currentUrl,
                    headers: headers,
                    bearerToken: null,
                    method: currentMethod,
                    content: currentContent);

                effectiveCookieHeader = MergeCookieHeader(effectiveCookieHeader, response);

                if (!IsRedirect(response.StatusCode) || response.Headers.Location is null)
                    break;

                redirectCount++;
                if (redirectCount > 5)
                    break;

                currentUrl = response.Headers.Location.IsAbsoluteUri
                    ? response.Headers.Location.ToString()
                    : new Uri(new Uri(currentUrl), response.Headers.Location).ToString();

                currentMethod = response.StatusCode == HttpStatusCode.RedirectMethod ? HttpMethod.Get : currentMethod;
                currentContent = response.StatusCode == HttpStatusCode.RedirectMethod ? string.Empty : currentContent;
            }

            var rawBody = response.Content is null
                ? string.Empty
                : await response.Content.ReadAsStringAsync(cancellationToken);

            var body = TryParseResponse(rawBody);

            var effectiveStatus = IsRedirect(response.StatusCode) && body is not null
                ? HttpStatusCode.OK
                : response.StatusCode;

            return (
                new CjsCheckoutPaymentAppResult(
                effectiveStatus,
                body,
                SetCookieHeaders: effectiveCookieHeader),
                response.StatusCode);
        }

        private static CjsCheckoutPaymentAppResponse? TryParseResponse(string rawBody)
        {
            if (rawBody.IsNullOrWhiteSpace())
                return null;

            try
            {
                return Json.Deserialize<CjsCheckoutPaymentAppResponse>(rawBody);
            }
            catch
            {
                return null;
            }
        }

        private static bool IsRedirect(HttpStatusCode statusCode) =>
            statusCode == HttpStatusCode.RedirectMethod || statusCode == HttpStatusCode.TemporaryRedirect;

        private static string? MergeCookieHeader(string? existingCookieHeader, HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
                return FilterCookieHeader(existingCookieHeader);

            var cookies = ParseCookieHeader(existingCookieHeader);

            foreach (var setCookie in setCookieHeaders ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(setCookie))
                    continue;

                var firstSegment = setCookie.Split(';')[0].Trim();
                if (string.IsNullOrWhiteSpace(firstSegment))
                    continue;

                var idx = firstSegment.IndexOf('=', StringComparison.Ordinal);
                if (idx <= 0)
                    continue;

                var name = firstSegment[..idx].Trim();
                var value = firstSegment[(idx + 1)..].Trim();

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                cookies[name] = value;
            }

            return FormatAllowedCookieHeader(cookies);
        }

       

        private static Dictionary<string, string> ParseCookieHeader(string? cookieHeader)
        {
            var cookies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(cookieHeader))
                return cookies;

            foreach (var segment in cookieHeader.Split(';'))
            {
                var trimmed = segment.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                    continue;

                var idx = trimmed.IndexOf('=', StringComparison.Ordinal);
                if (idx <= 0)
                    continue;

                var name = trimmed[..idx].Trim();
                var value = trimmed[(idx + 1)..].Trim();

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                cookies[name] = value;
            }

            return cookies;
        }

        private static string? FilterCookieHeader(string? cookieHeader)
        {
            var cookies = ParseCookieHeader(cookieHeader);
            return FormatAllowedCookieHeader(cookies);
        }

        private static string? BuildCookieHeader(IEnumerable<string> cookies)
        {
            if (cookies is null)
                return null;

            var cookieHeader = string.Join("; ", cookies.Where(c => !string.IsNullOrWhiteSpace(c)));
            return FilterCookieHeader(cookieHeader);
        }

        private static string? FormatAllowedCookieHeader(Dictionary<string, string> cookies)
        {
            if (cookies is null || cookies.Count == 0)
                return null;

            var parts = new List<string>(AllowedDownstreamCookieNames.Length);

            foreach (var name in AllowedDownstreamCookieNames)
            {
                if (cookies.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value))
                    parts.Add($"{name}={value}");
            }

            return parts.Count == 0 ? null : string.Join("; ", parts);
        }
      
    }
}
