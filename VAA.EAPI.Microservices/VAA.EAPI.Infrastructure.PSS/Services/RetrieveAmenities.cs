using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.PSS.Entities.RetrieveAmenities;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Services
{
    public class RetrieveAmenities : IRetrieveAmenities
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ILogger<RetrieveAmenities> _logger;

        private static string token = string.Empty;
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);

        public RetrieveAmenities(ILogger<RetrieveAmenities> logger, IHttpConnector apiConnector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<RetrieveAmenitiesResponse> Retrieve(RetrieveAmenitiesRequest request)
        {
            await GetToken();

            try
            {
                var headers = new Dictionary<string, string>
                {
                    { "Accept", Constants.Application.ContenTypeapplicationjson }
                };

                return await _apiConnector.SendAsync<RetrieveAmenitiesResponse>(ConfigReader.Get(Constants.Environment.BrandAPIUrl), headers, token, HttpMethod.Post, Json.Serialize(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error from Brand Token API - {ex.Message}");

                return null;
            }
        }

        private async Task GetToken()
        {
            if (token.IsJwtTokenValid()) return;

            await semaphoreSlim.WaitAsync();

            if (token.IsJwtTokenValid()) return;

            try
            {
                var tokenRequest = new Dictionary<string, string>
                {
                    { Constants.Application.ClientId, ConfigReader.Get(Constants.Environment.BrandAPITokenClientId) },
                    { Constants.Application.ClientSecret, ConfigReader.Get(Constants.Environment.BrandAPITokenClientSecret) },
                    { Constants.Application.GrantType, Constants.Application.ClientCredentials }
                };

                var tokenResponse = await _apiConnector.PostAsync(ConfigReader.Get(Constants.Environment.BrandAPITokenUrl), new FormUrlEncodedContent(tokenRequest));

                if (tokenResponse.StatusCode == HttpStatusCode.OK)
                {
                    var response = Xml.Deserialize<BrandTokenResponse>(await tokenResponse.Content.ReadAsStringAsync());

                    token = response.Access_token;
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return;
        }
    }
}
