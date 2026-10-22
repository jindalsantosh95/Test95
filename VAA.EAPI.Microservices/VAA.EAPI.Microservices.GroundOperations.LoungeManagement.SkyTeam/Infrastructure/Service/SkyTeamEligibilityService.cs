using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Services;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Service
{
    public class SkyTeamEligibilityService : ISkyTeamEligibilityService
    {
        private static readonly string skyAuthenticationUrl = $"{ConfigReader.Get(Constants.Environment.SkyteamEligibilityApiUrl)}{ConfigReader.Get(Constants.Environment.LoungeEligibilityToken)}";
        private static readonly string skyTokenCacheKey = $"{ConfigReader.Get(Constants.Environment.SkyteamClientIdEligibilityApi)}/ {ConfigReader.Get(Constants.Environment.SkyteamClientSecretEligibilityApi)}";
        private static readonly Dictionary<string, string> clientCredentials = new()
        {
            { Constants.Application.ClientId, ConfigReader.Get(Constants.Environment.SkyteamClientIdEligibilityApi) },
            { Constants.Application.ClientSecret, ConfigReader.Get(Constants.Environment.SkyteamClientSecretEligibilityApi) },
            { Constants.Application.GrantType,  ConfigReader.Get(Constants.Environment.SkyteamEligibilityApiGrantType) },
            { Constants.Application.Scope,  ConfigReader.Get(Constants.Environment.SkyteamEligibilityApiScope) }
        };

        private readonly IHttpConnector _httpConnector;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly ILogger<SkyTeamEligibilityService> _logger;

        public SkyTeamEligibilityService(IHttpConnector httpConnector, ITokenGenerator tokenGenerator, ILogger<SkyTeamEligibilityService> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<Response<LoungeEligibilityResponse>> Eligibility(LoungeEligibilityRequest request)
        {
            var token = await _tokenGenerator.GenerateToken(skyTokenCacheKey, skyAuthenticationUrl, clientCredentials);

            int Retries = Constants.Application.Retries;
            while (true)
            {
                try
                {
                    var response = await _httpConnector.SendAsync<LoungeEligibilityResponse>($"{ConfigReader.Get(Constants.Environment.SkyteamEligibilityApiUrl)}{ConfigReader.Get(Constants.Environment.LoungeEligibility)}", null, token, HttpMethod.Post, Json.Serialize(request,false));
                    
                    return new Response<LoungeEligibilityResponse>()
                    {
                        Status = Status.OK,
                        Value = response };
                }

                catch(Exception ex) 
                {
                    if (ex.Data != null && ex.Data.Count != 0)
                    {
                        throw (new ApiFailedException("SkyTeam-FrontEnd-Eligibility", ex));
                    }

                    if (--Retries == 0)
                    {
                        _logger.LogError(ex, "SkyTeam eligibility request failed after {RetryCount} attempts.", Constants.Application.Retries);

                        throw (new ApiFailedException("SkyTeam-FrontEnd-Eligibility", ex));
                    }
                    else
                    {
                        continue;
                    }
                    
                }
            }
        }

        public async Task<Response<LoungeEligibilityOverrideResponse>> EligibilityOverride(LoungeEligibilityOverrideRequest request)
        {
            var token = await _tokenGenerator.GenerateToken(skyTokenCacheKey, skyAuthenticationUrl, clientCredentials);

            try
            {
                return new Response<LoungeEligibilityOverrideResponse>()
                {
                    Status = Status.OK,
                    Value = await _httpConnector.SendAsync<LoungeEligibilityOverrideResponse>($"{ConfigReader.Get(Constants.Environment.SkyteamEligibilityApiUrl)}{ConfigReader.Get(Constants.Environment.LoungeEligibilityOverride)}", null, token, HttpMethod.Post, Json.Serialize(request,false))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SkyTeam eligibility override request failed.");

                throw (new ApiFailedException("SkyTeam-FrontEnd-EligibilityOverride", new Exception(ex.Message)));
            }
        }
    }
}
