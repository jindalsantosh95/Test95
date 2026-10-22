using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Sustainability.Constants;
using VAA.EAPI.Microservices.Retail.Sustainability.Domain;

namespace VAA.EAPI.Microservices.Retail.Sustainability.Application.Providers
{
    public sealed class CarbonFootprintProvider : ICarbonFootprintProvider
    {
        private static string token;
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<CarbonFootprintProvider> _logger;

        public CarbonFootprintProvider(ILogger<CarbonFootprintProvider> logger, IHttpConnector httpConnector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<IEnumerable<CarbonFootprint>> FetchCarbonFootprintsAsync(CarbonRequest request, CancellationToken cancellationToken)
        {
            HttpResponseMessage httpResponseMessage;
            await GetToken(cancellationToken);
            var requestContent = Json.Serialize(request.Flights);


            try
            {
                string choooseUri = ConfigReader.Get(Constants.Environment.ChoooseUri);

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, string.Format(choooseUri, request.Passengers?.ToString() ?? string.Empty))
                {
                    Content = new StringContent(requestContent, System.Text.Encoding.UTF8, "application/json")
                };

                httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                httpResponseMessage = await _httpConnector.SendAsync(httpRequestMessage, cancellationToken);

                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    var ex = new HttpResponseException(httpResponseMessage);

                    ex.Data.Add("CorrelationId", AsyncContext.Current.CorrelationId);
                    ex.Data.Add("Request", requestContent);
                    ex.Data.Add("Response", await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken));

                    throw ex;
                }

            }
            catch (Exception ex)
            {
                if (!ex.Data.Contains("CorrelationId"))
                    ex.Data.Add("CorrelationId", AsyncContext.Current.CorrelationId);

                if (!ex.Data.Contains("Request"))
                    ex.Data.Add("Request", requestContent);

                _logger.LogError(ex, "Unable to obtain CHOOOSE response");

                throw;
            }

            try
            {
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                    return JsonConvert.DeserializeObject<JArray>(response).Select(x => (CarbonFootprint)x);
                }
            }
            catch (Exception ex)
            {
                if (!ex.Data.Contains("CorrelationId"))
                    ex.Data.Add("CorrelationId", AsyncContext.Current.CorrelationId);

                if (!ex.Data.Contains("Request"))
                    ex.Data.Add("Request", Json.Serialize(request));

                var responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(ex, "Error parsing CHOOOSE response:" + System.Environment.NewLine + responseContent);

                throw;
            }

            return Array.Empty<CarbonFootprint>();
        }

        private async Task GetToken(CancellationToken cancellationToken)
        {
            if (token.IsJwtTokenValid()) return;

            try
            {
                await _semaphoreSlim.WaitAsync();

                if (token.IsJwtTokenValid()) return;

                string authenticationUrl = ConfigReader.Get(Constants.Environment.AuthenticationUrl);
                string tenantId = ConfigReader.Get(Constants.Environment.TenantId);
                string clientId = ConfigReader.Get(Constants.Environment.ClientId);
                string clientSecret = ConfigReader.Get(Constants.Environment.ClientSecret);
                string scope = ConfigReader.Get(Constants.Environment.ClientScope);

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{authenticationUrl}/{tenantId}/oauth2/v2.0/token")
                {
                    Content = new MultipartFormDataContent()
                    {
                        { new StringContent("client_credentials"), "grant_type" },
                        { new StringContent(clientId), "client_id" },
                        { new StringContent(clientSecret), "client_secret" },
                        { new StringContent(scope), "scope" }
                    }
                };

                var authResponse = await _httpConnector.SendAsync(httpRequestMessage, cancellationToken);

                if (!authResponse.IsSuccessStatusCode)
                {
                    var ex = new HttpResponseException(authResponse);
                    ex.Data.Add("CorrelationId", AsyncContext.Current.CorrelationId);
                    throw ex;
                }

                else
                {
                    dynamic result = JsonConvert.DeserializeObject<dynamic>(await authResponse.Content.ReadAsStringAsync(cancellationToken));

                    token = ((JObject)result)["access_token"].ToString();
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
