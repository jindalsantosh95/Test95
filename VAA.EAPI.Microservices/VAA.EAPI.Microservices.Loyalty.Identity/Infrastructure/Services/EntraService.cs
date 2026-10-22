using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class EntraService : IEntraService
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<EntraService> _logger;

        public EntraService(IHttpConnector httpConnector, ILogger<EntraService> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> Start(IEnumerable<ChallengeType> challengeTypes, string urlEntraJourney, string username,  CancellationToken cancellationToken)
        {
            string url = string.Format(Environment.EntraAPIurl, ConfigReader.Get(Environment.EntraAPIurlDomain), urlEntraJourney, "v1.0/start");

            var formData = new Dictionary<string, string>
            {
                { "client_id", ConfigReader.Get(Environment.EntraClientId) },
                { "challenge_type", string.Join(" ", challengeTypes.Select(x => x.ToString().ToLowerInvariant())) },
                { "username", username }
            };

            if (urlEntraJourney.Is("signup"))
                formData.Add("capabilities", "registration_required");

            var content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            try
            {
                HttpResponseMessage httpResponse = await _httpConnector.PostAsync(url, content);
                var responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!httpResponse.IsSuccessStatusCode || responseMessage.Contains("odata.error", StringComparison.CurrentCultureIgnoreCase))
                {
                    _logger.LogWarning($"Entra Enrolment Error Response: {responseMessage}");
                }

                return responseMessage;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error occurred during sign-up: {ex.Message}");
                return null;
            }
        }

        public async Task<string> Initiate(IEnumerable<ChallengeType> challengeTypes, string username, CancellationToken cancellationToken)
        {
            string url = string.Format(Environment.EntraAPIurl, ConfigReader.Get(Environment.EntraAPIurlDomain), "oauth2", "v2.0/initiate");

            var formData = new Dictionary<string, string>
            {
                { "client_id", ConfigReader.Get(Environment.EntraClientId) },
                { "username", username },
                { "challenge_type", string.Join(" ", challengeTypes.Select(x => x.ToString().ToLowerInvariant())) },
                { "capabilities", "mfa_required" }
            };

            var content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            try
            {
                HttpResponseMessage httpResponse = await _httpConnector.PostAsync(url, content);
                var responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!httpResponse.IsSuccessStatusCode || responseMessage.Contains("odata.error", StringComparison.CurrentCultureIgnoreCase))
                {
                    _logger.LogWarning($"Entra Initiate Error Response: {responseMessage}");
                }

                return responseMessage;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error calling API: {ex.Message}");
                return null;
            }
        }

        public async Task<string> Challenge(IEnumerable<ChallengeType> challengeTypes, string urlEntraJourney, string continuationToken, CancellationToken cancellationToken, string mfa_id)
        {
            string url = string.Format(Environment.EntraAPIurl, ConfigReader.Get(Environment.EntraAPIurlDomain), urlEntraJourney, "challenge");

            var formData = new Dictionary<string, string>
            {
                { "client_id", ConfigReader.Get(Environment.EntraClientId) },
                { "continuation_token", continuationToken }
            };

            if (!string.IsNullOrWhiteSpace(mfa_id))
                formData.Add("id", mfa_id);

            if (challengeTypes != null)
                formData.Add("challenge_type", string.Join(" ", challengeTypes.Select(x => x.ToString().ToLowerInvariant())));

            var content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            try
            {
                HttpResponseMessage httpResponse = await _httpConnector.PostAsync(url, content);
                var responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!httpResponse.IsSuccessStatusCode || responseMessage.Contains("odata.error", StringComparison.CurrentCultureIgnoreCase))
                {
                    _logger.LogWarning($"Error occurred during challenge authentication: {responseMessage}");
                }

                return responseMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred during challenge authentication: {ex.Message}");
                return null;
            }
        }

        public async Task<string> Continue(string continuationToken, string grantType, string grantTypeValue, string urlEntraJourney, CancellationToken cancellationToken)
        {
            string url = string.Format(Environment.EntraAPIurl, ConfigReader.Get(Environment.EntraAPIurlDomain), urlEntraJourney, "v1.0/continue");

            if (string.IsNullOrWhiteSpace(grantType))
            {
                throw new ArgumentException("Grant type is required.", nameof(grantType));
            }

            var normalizedGrantType = grantType.Trim().ToLowerInvariant();
            
            var valueKey = normalizedGrantType switch
            {
                "oob" => "oob",
                "attributes" => "attributes",
                "password" => "password",
                _ => throw new ArgumentOutOfRangeException(nameof(grantType), grantType, "Unsupported grant type. Expected one of: oob, attributes, password.")
            };

            var formData = new Dictionary<string, string>
            {
                ["client_id"] = ConfigReader.Get(Environment.EntraClientId),
                ["continuation_token"] = continuationToken,
                ["grant_type"] = normalizedGrantType,
                [valueKey] = grantTypeValue
            };

            var content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            try
            {
                HttpResponseMessage httpResponse = await _httpConnector.PostAsync(url, content);
                var responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!httpResponse.IsSuccessStatusCode || responseMessage.Contains("odata.error", StringComparison.CurrentCultureIgnoreCase))
                {
                    _logger.LogWarning($"Entra Continue Error Response: {responseMessage}");
                }

                return responseMessage;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error calling API: {ex.Message}");
                return null;
            }
        }

        public async Task<string> Submit(string continuationToken, string newPassword, CancellationToken cancellationToken)
        {
            string url = string.Format(Environment.EntraAPIurl, ConfigReader.Get(Environment.EntraAPIurlDomain), "resetpassword", "v1.0/submit");

            var formData = new Dictionary<string, string>
            {
                { "client_id", ConfigReader.Get(Environment.EntraClientId) },
                { "continuation_token", continuationToken },
                { "new_password", newPassword }
            };

            var content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            try
            {
                HttpResponseMessage httpResponse = await _httpConnector.PostAsync(url, content);
                var responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!httpResponse.IsSuccessStatusCode || responseMessage.Contains("odata.error", StringComparison.CurrentCultureIgnoreCase))
                {
                    _logger.LogWarning($"Entra Submit Error Response: {responseMessage}");
                }

                return responseMessage;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error calling API: {ex.Message}");
                return null;
            }
        }

        public async Task<string> PollCommunication(string continuationToken, CancellationToken cancellationToken)
        {
            string url = string.Format(Environment.EntraAPIurl, ConfigReader.Get(Environment.EntraAPIurlDomain), "resetpassword", "v1.0/poll_completion");

            var formData = new Dictionary<string, string>
            {
                { "client_id", ConfigReader.Get(Environment.EntraClientId) },
                { "continuation_token", continuationToken }
            };

            var content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            try
            {
                HttpResponseMessage httpResponse = await _httpConnector.PostAsync(url, content);
                var responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!httpResponse.IsSuccessStatusCode || responseMessage.Contains("odata.error", StringComparison.CurrentCultureIgnoreCase))
                {
                    _logger.LogWarning($"Entra Poll Communication Error Response: {responseMessage}");
                }

                return responseMessage;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error calling API: {ex.Message}");
                return null;
            }
        }

        public async Task<string> Token(string continuationToken, IEnumerable<string> grantTypes, IEnumerable<string> scopes, string username, string password, string oob, CancellationToken cancellationToken)
        {
            string url = string.Format(Environment.EntraAPIurl, ConfigReader.Get(Environment.EntraAPIurlDomain), "oauth2", "v2.0/token");

            var formData = new Dictionary<string, string>
            {
                { "client_id", ConfigReader.Get(Environment.EntraClientId) },
                { "continuation_token", continuationToken },
                { "grant_type", string.Join(" ", grantTypes) },
                { "scope", string.Join(" ", scopes) }
            };

            if (!string.IsNullOrEmpty(password))
                formData.Add("password", password);

            if (!string.IsNullOrEmpty(oob))
                formData.Add("oob", oob);

            var content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            try
            {
                HttpResponseMessage httpResponse = await _httpConnector.PostAsync(url, content);
                var responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!httpResponse.IsSuccessStatusCode || responseMessage.Contains("odata.error", StringComparison.CurrentCultureIgnoreCase))
                {
                    _logger.LogWarning($"Entra Token Error Response: {responseMessage}");
                }

                return responseMessage;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error calling API: {ex.Message}");
                return null;
            }
        }

        public async Task<string> Introspect(string urlEntraJourney, string continuationToken, CancellationToken cancellationToken)
        {
            string url = string.Format(Environment.EntraAPIurl, ConfigReader.Get(Environment.EntraAPIurlDomain), urlEntraJourney, "introspect");

            var formData = new Dictionary<string, string>
            {
                { "client_id", ConfigReader.Get(Environment.EntraClientId) },
                { "continuation_token", continuationToken }
            };

            var content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            try
            {
                HttpResponseMessage httpResponse = await _httpConnector.PostAsync(url, content);
                var responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!httpResponse.IsSuccessStatusCode || responseMessage.Contains("odata.error", StringComparison.CurrentCultureIgnoreCase))
                {
                    _logger.LogWarning($"Error occurred during inrtospect: {responseMessage}");
                }

                return responseMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred during inrtospect: {ex.Message}");
                return null;
            }
        }

        public async Task<string> RegisterIntrospect(string continuationToken, CancellationToken cancellationToken = default)
        {
            string url = string.Format(Environment.EntraAPIurl, ConfigReader.Get(Environment.EntraAPIurlDomain), "register", "v1.0/introspect");

            var formData = new Dictionary<string, string>
            {
                { "client_id", ConfigReader.Get(Environment.EntraClientId) },
                { "continuation_token", continuationToken }
            };

            return await PostRegistrationRequest(url, formData, "introspect", cancellationToken);
        }

        public async Task<string> RegisterChallenge(string continuationToken, string challengeTarget, CancellationToken cancellationToken = default)
        {
            string url = string.Format(Environment.EntraAPIurl, ConfigReader.Get(Environment.EntraAPIurlDomain), "register", "v1.0/challenge");

            var formData = new Dictionary<string, string>
            {
                { "client_id", ConfigReader.Get(Environment.EntraClientId) },
                { "continuation_token", continuationToken },
                { "challenge_type", "oob" },
                { "challenge_channel", "email" },
                { "challenge_target", challengeTarget }
            };

            return await PostRegistrationRequest(url, formData, "challenge", cancellationToken);
        }

        public async Task<string> RegisterContinue(string continuationToken, string grantType, string grantTypeValue = null, CancellationToken cancellationToken = default)
        {
            if (grantType.IsNullOrWhiteSpace())
                throw new ArgumentException("Grant type is required.", nameof(grantType));

            var normalizedGrantType = grantType.Trim().ToLowerInvariant();
            if (normalizedGrantType != "continuation_token" && normalizedGrantType != "oob")
                throw new ArgumentOutOfRangeException(nameof(grantType), grantType, "Unsupported registration grant type. Expected continuation_token or oob.");

            if (normalizedGrantType == "oob" && grantTypeValue.IsNullOrWhiteSpace())
                throw new ArgumentException("An out-of-band code is required for the oob grant type.", nameof(grantTypeValue));

            string url = string.Format(Environment.EntraAPIurl, ConfigReader.Get(Environment.EntraAPIurlDomain), "register", "v1.0/continue");

            var formData = new Dictionary<string, string>
            {
                { "client_id", ConfigReader.Get(Environment.EntraClientId) },
                { "continuation_token", continuationToken },
                { "grant_type", normalizedGrantType }
            };

            if (normalizedGrantType == "oob")
                formData.Add("oob", grantTypeValue);

            return await PostRegistrationRequest(url, formData, "continue", cancellationToken);
        }

        private async Task<string> PostRegistrationRequest(string url, Dictionary<string, string> formData, string operation, CancellationToken cancellationToken)
        {
            var content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            try
            {
                HttpResponseMessage httpResponse = await _httpConnector.PostAsync(url, content);
                var responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!httpResponse.IsSuccessStatusCode || responseMessage.Contains("odata.error", StringComparison.CurrentCultureIgnoreCase))
                    _logger.LogWarning("Entra registration {Operation} request returned {StatusCode}.", operation, httpResponse.StatusCode);

                return responseMessage;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Entra registration {Operation} endpoint.", operation);
                return null;
            }
        }
    }
}
