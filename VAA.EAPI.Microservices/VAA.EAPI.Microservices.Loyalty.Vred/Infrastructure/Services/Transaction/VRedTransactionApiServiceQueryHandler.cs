using System;
using MediatR;
using System.Net;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using VAA.EAPI.Infrastructure.Helper;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services
{
    public class VRedTransactionApiServiceQueryHandler : IRequestHandler<VRedTransactionApiServiceQuery, VRedTransactionResponse>
    {
        private readonly IHttpConnector _httpConnector;

        public VRedTransactionApiServiceQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        private async Task<KeyValuePair<bool, string>> PostRequest(string url, FormUrlEncodedContent data)
        {
            HttpResponseMessage response = await _httpConnector.PostAsync(url, data);
            if (!response.IsSuccessStatusCode)
            {
                return new KeyValuePair<bool, string>(false, await response.Content.ReadAsStringAsync());
            }
            return new KeyValuePair<bool, string>(true, await response.Content.ReadAsStringAsync());
        }

        private async Task<VRedTransactionResponse> PostRequest(string url, string bearerToken, string ocpApimSubscriptionKey, string content, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add(Constants.Application.CacheControl, Constants.Application.NoCache);
            request.Headers.Add(Constants.Application.AcceptEncoding, Constants.Application.GzipDeflate);
            request.Headers.Add(Constants.Application.Accept, Constants.Application.Everything);
            request.Headers.Authorization = new AuthenticationHeaderValue(Constants.Application.AuthenticationScheme, bearerToken);
            request.Headers.Add(Constants.Application.OcpApimSubscriptionKeyName, ocpApimSubscriptionKey);
            request.Content = new StringContent(content,
                                    Encoding.UTF8,
                                    Constants.Application.ContenTypeapplicationjson);

            var vRedResponse = new VRedTransactionResponse();

            try
            {
                HttpResponseMessage httpResponse = await _httpConnector.SendAsync(request, cancellationToken);

                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.Accepted:
                    case HttpStatusCode.OK:
                        TransactionResponse transResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionResponse>(await httpResponse.Content.ReadAsStringAsync());
                        vRedResponse.ErrorResponse = null;
                        vRedResponse.SuccessResponse = transResponse;
                        break;

                    default:
                        VRedErrorResponse transErrorResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<VRedErrorResponse>(await httpResponse.Content.ReadAsStringAsync());
                        vRedResponse.ErrorResponse = transErrorResponse;
                        vRedResponse.SuccessResponse = null;
                        break;
                }
            }
            catch (Exception)
            {
                return new VRedTransactionResponse()
                {
                    ErrorResponse = new VRedErrorResponse()
                    {
                        Description = "Unable to connect to VRed Transaction endpoint"
                    }
                };
            }

            return vRedResponse;
        }

        private async Task<string> GetAuthTokenGeneric()
        {
            string authUrl = ConfigReader.Get(Constants.Environment.VRedAuthUrlSettingName);
            string authAudience = ConfigReader.Get(Constants.Environment.VRedAuthAudienceSettingName);
            string authClientId = ConfigReader.Get(Constants.Environment.VRedAuthClientIdSettingName);
            Constants.Application.AuthClientSecretCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Environment.VredAuthSecretSettingName, Constants.Application.AuthClientSecretCached);
            FormUrlEncodedContent formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>(Constants.Application.GrantType, Constants.Application.ClientCredentials),
                    new KeyValuePair<string, string>(Constants.Application.ClientId, authClientId),
                    new KeyValuePair<string, string>(Constants.Application.ClientSecret, Constants.Application.AuthClientSecretCached),
                    new KeyValuePair<string, string>(Constants.Application.ContentType, Constants.Application.ApplicationxWwwFormUrlencoded),
                    new KeyValuePair<string, string>(Constants.Application.Audience, authAudience),
                });

            KeyValuePair<bool, string> response = await PostRequest(authUrl, formContent);
            if (response.Key)
            {
                string content = response.Value;

                Dictionary<string, string> listContent = Common.Serialisation.Json.Deserialize<Dictionary<string, string>>(content);
                Constants.Application.VRedAuthTokenCached = listContent?.FirstOrDefault(k => k.Key.Equals(Constants.Application.VRedAuthResponseAccessToken)).Value;
            }
            return Constants.Application.VRedAuthTokenCached;
        }

        public async Task<VRedTransactionResponse> Handle(VRedTransactionApiServiceQuery request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            string token;          
            try
            {
                if (string.IsNullOrEmpty(Constants.Application.VRedAuthTokenCached))
                {
                    token = await GetAuthTokenGeneric();
                }
                else
                {
                    JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                    JwtSecurityToken decodedValue = handler.ReadJwtToken(Constants.Application.VRedAuthTokenCached);
                    DateTime jwtTokenEpiryDate = decodedValue.ValidTo;
                    DateTime jwtTokenEpiryDateMinusFiveMinutes = jwtTokenEpiryDate.AddMinutes(-5);
                    if (jwtTokenEpiryDateMinusFiveMinutes > DateTime.UtcNow)
                    {
                        token = Constants.Application.VRedAuthTokenCached;
                    }
                    else
                    {
                        token = await GetAuthTokenGeneric();
                    }
                }
            }
            catch
            {
                return new VRedTransactionResponse()
                {
                    ErrorResponse = new VRedErrorResponse()
                    {
                        Description = "Unable to connect to OAuth authority endpoint"
                    }
                };
            }
            
            Constants.Application.VRedOcpApimSubscriptionKeyCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Environment.VRedOcpApimSubscriptionKeySettingName, Constants.Application.VRedOcpApimSubscriptionKeyCached);
            return await PostRequest(ConfigReader.Get(Constants.Environment.VredTransactionAPIV2), token, Constants.Application.VRedOcpApimSubscriptionKeyCached, JsonSerializer.Serialize(request.TransactionPostRequest), cancellationToken);
        }
    }
}