using Aliencube.AdalWrapper;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<RefreshTokenService> _logger;

        public RefreshTokenService(IHttpConnector httpConnector, ILogger<RefreshTokenService> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> RefreshToken(string refreshToken, IEnumerable<string> scopes, CancellationToken cancellationToken)
        {
            string url = $"https://{ConfigReader.Get(Environment.EntraAPISubDomain)}.ciamlogin.com/{ConfigReader.Get(Environment.EntraTenantId)}/oauth2/v2.0/token";

            var formData = new Dictionary<string, string>
            {
                { "client_id", ConfigReader.Get(Environment.EntraClientId) },
                { "refresh_token", refreshToken },
                { "grant_type", "refresh_token" }
            };

            if (scopes != null && scopes.Any())
            {
                formData.Add("scope", string.Join(" ", scopes));
            }

            var content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            try
            {
                HttpResponseMessage httpResponse = await _httpConnector.PostAsync(url, content);
                var responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!httpResponse.IsSuccessStatusCode || responseMessage.Contains("error", StringComparison.CurrentCultureIgnoreCase))
                {
                    _logger.LogError("Entra token refresh failed. StatusCode: {StatusCode}", (int)httpResponse.StatusCode);
                }

                return responseMessage;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Entra token refresh request failed.");
                return null;
            }
        }
    }
}
