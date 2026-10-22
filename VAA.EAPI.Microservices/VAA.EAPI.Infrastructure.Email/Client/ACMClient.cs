using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Email.Policies;
using VAA.EAPI.Infrastructure.Interfaces;


namespace VAA.EAPI.Infrastructure.Email.Client
{
    public class ACMClient : IACMClient
    {
        private static readonly string adobeAuthenticationUrl = $"{ConfigReader.Get(Constants.Constants.AdobeAuthenticationUrl)}token/v3";
        private static readonly string adobeTokenCacheKey = $"{ConfigReader.Get(Constants.Constants.AdobeClientId)}/{ConfigReader.Get(Constants.Constants.AdobeClientSecret)}";
        private static readonly Dictionary<string, string> clientCredentials = new()
        {
            { Constants.Application.ClientId, ConfigReader.Get(Constants.Constants.AdobeClientId) },
            { Constants.Application.ClientSecret, ConfigReader.Get(Constants.Constants.AdobeClientSecret) },
            { Constants.Application.GrantType, ConfigReader.Get(Constants.Constants.AdobeGrantType) },
            { Constants.Application.Scope, ConfigReader.Get(Constants.Constants.AdobeScope) }
        };

        private readonly ILogger<ACMClient> _logger;
        private readonly IHttpConnector _httpConnector;
        private readonly ITokenHelper _tokenHelper;

        public ACMClient(ILogger<ACMClient> logger, IHttpConnector httpConnector, ITokenHelper tokenHelper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
        }

        public async Task<bool> SendEmail(string request)
        {
            if (EmailRecipientPolicy.ShouldSuppressAcmRequest(request))
            {
                _logger.LogInformation("Email delivery suppressed by recipient policy");
                return true;
            }

            try
            {
                var authorization = string.Empty;
                var header = new Dictionary<string, string>()
                {
                     { "SOAPAction", "nms:rtEvent#PushEvent" },
                };

                string uri = ConfigReader.Get<bool>(Constants.Constants.EnableAdobeNewEndpoint) ? ConfigReader.Get(Constants.Constants.SoapEndpointUriV2) : ConfigReader.Get(Constants.Constants.SoapEndpointUri);

                if (ConfigReader.Get<bool>(Constants.Environment.EnableEmailMSAuthentication))
                {
                    authorization = await _tokenHelper.GetToken(adobeTokenCacheKey, adobeAuthenticationUrl, new FormUrlEncodedContent(clientCredentials));
                }

                var httpResponse = await _httpConnector.SendAsync(uri, header, authorization, HttpMethod.Post, request);

                return httpResponse.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ACM API Error Response: {ex}");

                return false;
            }
        }
    }
}
