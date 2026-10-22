using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.FCNumberGeneration.Entities;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.FCNumberGeneration.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.FCNumberGeneration.Services
{
    public class FCNumberGenerationService : IFCNumberGenerationService
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FCNumberGenerationService> _logger;

        public FCNumberGenerationService(IHttpClientFactory httpClientFactory, ILogger<FCNumberGenerationService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<KeyValuePair<bool, string>> GenerateFCNumber()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            string baseURL = ConfigReader.Get(Constants.Environment.FCNumberGenAPIURL);

            FCNumberGenResponse fCNumberGenResponse = new FCNumberGenResponse();
            HttpResponseMessage httpResponse = null;
            int numberOfAttempts = 0;
            string apiResponse = string.Empty;
            if (!httpClient.DefaultRequestHeaders.Contains(Constants.Application.ApimSubscription))
            {
                if (Constants.Application.FCNumberGenAPIURLOCPKeyValueCached == null)
                {
                    Constants.Application.FCNumberGenAPIURLOCPKeyValueCached = ConfigReader.Get(Constants.Environment.FCNumberGenAPIURLOCPKeyValue);

                }
                httpClient.DefaultRequestHeaders.Add(Constants.Application.ApimSubscription, Constants.Application.FCNumberGenAPIURLOCPKeyValueCached);
            }
            //httpClient.BaseAddress = new Uri(baseURL);
            #region Check if ClientID value is available in static Field

            if (Constants.Application.FCNumberGenAPIClientIDCached == null)
            {
                Constants.Application.FCNumberGenAPIClientIDCached = ConfigReader.Get(Constants.Environment.FCNumberGenAPIClientID);
            }
            #endregion

            #region Check if Secret value is available in static Field
            if (Constants.Application.FCNumberGenAPIClientSecretValueCached == null)
            {
                Constants.Application.FCNumberGenAPIClientSecretValueCached = ConfigReader.Get(Constants.Environment.FCNumberGenAPIClientSecretValue);
            }
            string clientID = Constants.Application.FCNumberGenAPIClientIDCached;
            string clientSecret = Constants.Application.FCNumberGenAPIClientSecretValueCached;
            string authority = ConfigReader.Get(Constants.Environment.AzureADEndpoint);
            string resourceID = ConfigReader.Get(Constants.Environment.FCNumberGenAPIResource);
            #endregion
            do
            {
                string token = await ExternalHelper.GetBearerToken(clientID, clientSecret, authority, resourceID);

                if (!httpClient.DefaultRequestHeaders.Contains(Constants.Application.AuthorizationHeader))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.Application.AuthorizationTokenType, token);
                }

                httpResponse = await httpClient.GetAsync(baseURL);
                numberOfAttempts++;
                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    httpClient.DefaultRequestHeaders.Remove(Constants.Application.AuthorizationHeader);
                }
            } while (numberOfAttempts < 3 && httpResponse.StatusCode == HttpStatusCode.Unauthorized);

            if (!httpResponse.IsSuccessStatusCode)
            {
                string error = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogInformation($"FCNumber Generaton API Error Response: {error}");
                return new KeyValuePair<bool, string>(false, error);
            }
            string response = await httpResponse.Content.ReadAsStringAsync();
            _logger.LogInformation($"FCNumber Generaton API Success Response");
            return new KeyValuePair<bool, string>(true, response);
        }

    }
}
