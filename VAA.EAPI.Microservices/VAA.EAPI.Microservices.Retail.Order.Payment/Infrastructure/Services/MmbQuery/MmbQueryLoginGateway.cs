using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.MmbQuery;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.MmbQuery
{
    public sealed class MmbQueryLoginGateway : IMmbQueryLoginGateway
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<MmbQueryLoginGateway> _logger;

        public MmbQueryLoginGateway(IHttpConnector httpConnector, ILogger<MmbQueryLoginGateway> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MmbQueryLoginResult> LoginAsync(MmbQueryLoginRequest request, CancellationToken cancellationToken)
        {
            var url = ConfigReader.Get(Constants.Environment.MmbQueryLoginUrl);

            var payload = new JObject
            {
                ["bookingReference"] = request.BookingReference,
                ["surname"] = request.Surname,
                ["dateOfBirth"] = request.DateOfBirth
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(payload.ToString(), System.Text.Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            var response = await _httpConnector.SendAsync(httpRequest, cancellationToken);

            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var body = TryParseResponse(rawBody);
            var cookieHeader = BuildCookieHeader(response);

            _logger.LogInformation("MMB query login call completed with status {StatusCode}.", (int)response.StatusCode);

            return new MmbQueryLoginResult(response.StatusCode, body, cookieHeader);
        }

        private static MmbQueryLoginResponse? TryParseResponse(string rawBody)
        {
            if (rawBody.IsNullOrWhiteSpace())
                return null;

            try
            {
                return Json.Deserialize<MmbQueryLoginResponse>(rawBody);
            }
            catch
            {
                return null;
            }
        }

        private static string? BuildCookieHeader(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
                return null;

            var cookiePairs = setCookieValues
                .Select(v => v?.Split(';', 2)[0])
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToArray();

            return cookiePairs.Length == 0 ? null : string.Join("; ", cookiePairs);
        }
    }
}
