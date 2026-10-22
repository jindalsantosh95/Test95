using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Models;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Services;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Service
{
    public class OAuthTokenGenerator : ITokenGenerator
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, string> cache = new();

        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<OAuthTokenGenerator> _logger;

        public OAuthTokenGenerator(IHttpConnector httpConnector, ILogger<OAuthTokenGenerator> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GenerateToken(string key, string uri, Dictionary<string, string> clientCredentials)
        {
            if (cache.ContainsKey(key) && IsTokeValid(cache[$"{key}_expiresIn"])) return cache[key];

            await _semaphoreSlim.WaitAsync();

            if (cache.ContainsKey(key) && IsTokeValid(cache[$"{key}_expiresIn"])) return cache[key];

            var requestBody = $"grant_type={clientCredentials[Constants.Application.GrantType]}&client_id={clientCredentials[Constants.Application.ClientId]}&client_secret={clientCredentials[Constants.Application.ClientSecret]}";
            var content = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

            try
            {
                var httpResponse = (await _httpConnector.PostAsync(uri, content)).EnsureSuccessStatusCode();

                var responseMessage = await httpResponse.Content.ReadAsStringAsync(CancellationToken.None);

                var result = Json.Deserialize<Token>(responseMessage);

                var resultExpiresIn = DateTime.UtcNow.AddSeconds(result.ExpiresIn - 60).ToString();

                cache.AddOrUpdate(key, result.AccessToken, (k, v) => result.AccessToken);
                cache.AddOrUpdate($"{key}_expiresIn", resultExpiresIn, (k, v) => resultExpiresIn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OAuthTokenGenerator - failed due to Exception");
            }
            finally
            {
                _semaphoreSlim.Release();
            }
            return cache.ContainsKey(key) && IsTokeValid(cache[$"{key}_expiresIn"]) ? cache[key] : string.Empty;
        }

        private bool IsTokeValid(string expiresIn)
        {
            return DateTime.UtcNow < Convert.ToDateTime(expiresIn);
        }
    }
}
