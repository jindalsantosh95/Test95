using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Infrastructure.Services
{
    public class AmperityService : IAmperityService
    {
        private readonly IHttpConnector _httpConnector;        
        private readonly ILogger<AmperityService> _logger;

        public AmperityService(IHttpConnector httpConnector, ILogger<AmperityService> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AmperityResponse> GetAmperityDetails(string loyaltyId)
        {
            try
            {                
                string url = $"{ConfigReader.Get(Constants.Environment.AmerityUrl)}{ConfigReader.Get(Constants.Environment.AmerityKey)}{"/profiles/"}{loyaltyId}";

                Dictionary<string, string> headers = new() {
                    { "Authorization", $"Bearer {ConfigReader.Get(Constants.Environment.AmperityToken)}"},
                    {"api-version",ConfigReader.Get(Constants.Environment.ApiVersion) },
                    { "amperity-tenant", ConfigReader.Get(Constants.Environment.AmperityTenant) }
                };

                return await _httpConnector.SendAsync<AmperityResponse>(url, headers, null, HttpMethod.Get, string.Empty);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error calling Amerity API: {ex.Message}");
                return null;
            }
        }
    }
}
