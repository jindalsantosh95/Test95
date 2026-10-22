using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Vred.Constants;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services
{
    public class VredServiceHandler : IRequestHandler<VredServiceQuery, VRedEnrolmentResponse>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<VredServiceHandler> _logger;

        public VredServiceHandler(IHttpConnector httpConnector, ILogger<VredServiceHandler> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<VRedEnrolmentResponse> Handle(VredServiceQuery request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            #region Get OAuth Token
            string token;
            try
            {
                if (string.IsNullOrEmpty(Constants.Application.VRedAuthTokenCached))
                {
                    token = await GetAuthTokenGeneric();
                    _logger.LogInformation($"VRed token not present in Cache || fetched from Service call");
                }
                else
                {
                    JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                    JwtSecurityToken decodedValue = handler.ReadJwtToken(Constants.Application.VRedAuthTokenCached);
                    DateTime jwtTokenEpiryDate = decodedValue.ValidTo;
                    DateTime jwtTokenEpiryDateMinusFiveMinutes = jwtTokenEpiryDate.AddMinutes(-5);
                    if (jwtTokenEpiryDateMinusFiveMinutes > DateTime.UtcNow)
                    {
                        _logger.LogInformation($"VRed token fetched from cache");
                        token = Constants.Application.VRedAuthTokenCached;
                    }
                    else
                    {
                        token = await GetAuthTokenGeneric();
                        _logger.LogInformation($"VRed token expired ||  fetched from Service call");
                    }
                }

            }
            catch
            {
                _logger.LogInformation($"VRed token aquisition ||  failed Service call");

                return new VRedEnrolmentResponse()
                {
                    ErrorResponse = new VRedErrorResponse()
                    {
                        Description = "Unable to connect to OAuth authority endpoint"
                    }
                };
            }
            #endregion

            #region Call the Enrolment Service
            Constants.Application.VRedOcpApimSubscriptionKeyCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Environment.VRedOcpApimSubscriptionKeySettingName, Constants.Application.VRedOcpApimSubscriptionKeyCached);
            string url = ConfigReader.Get(Constants.Environment.VRedEnrolmentAPISettingName);
            string vRedprofileContent = JsonConvert.SerializeObject(request.Request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = ConfigReader.Get<bool>(Application.VredEnableSignDate) && !string.IsNullOrEmpty(request.EnrolmentChannel) ? DefaultValueHandling.Include : DefaultValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
            });



            VRedEnrolmentResponse enrolmentResponse = new VRedEnrolmentResponse();
            try
            {
                KeyValuePair<bool, string> response = await PostRequest(url, token, Constants.Application.VRedOcpApimSubscriptionKeyCached, vRedprofileContent, cancellationToken);
                _logger.LogInformation($"MemberEnrolmentService.EnrolMemberInVRed Response Key : {response.Key}");
                if (response.Key)
                {
                    VRedEnrolmentResponse redProfile = JsonConvert.DeserializeObject<VRedEnrolmentResponse>(response.Value);
                    enrolmentResponse.Enrollments = redProfile.Enrollments;
                    enrolmentResponse.ErrorResponse = redProfile.ErrorResponse;
                }
                else
                {
                    VRedErrorResponse redProfileError = JsonConvert.DeserializeObject<VRedErrorResponse>(response.Value);
                    enrolmentResponse.Enrollments = new Dictionary<string, string>();
                    enrolmentResponse.ErrorResponse = new VRedErrorResponse
                    {
                        Code = redProfileError.Code,
                        Status = redProfileError.Status,
                        Description = redProfileError.Description,
                        Message = redProfileError.Message,
                        Url = redProfileError.Url
                    };
                }
            }
            catch (Exception)
            {
                _logger.LogInformation($"Unable to connect to enrolment endpoint");

                return new VRedEnrolmentResponse()
                {
                    ErrorResponse = new VRedErrorResponse()
                    {
                        Description = "Unable to connect to enrolment endpoint"
                    }
                };
            }
            #endregion

            return enrolmentResponse;
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

        private async Task<KeyValuePair<bool, string>> PostRequest(string url, string bearerToken, string ocpApimSubscriptionKey, string content, CancellationToken cancellationToken)
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
            HttpResponseMessage httpResponse = await _httpConnector.SendAsync(request, cancellationToken);
            if (!httpResponse.IsSuccessStatusCode)
            {
                string error = await httpResponse.Content.ReadAsStringAsync();
                return new KeyValuePair<bool, string>(false, error);
            }
            string response = await httpResponse.Content.ReadAsStringAsync();
            return new KeyValuePair<bool, string>(true, response);
        }

        private async Task<KeyValuePair<bool, string>> PostRequest(string url, FormUrlEncodedContent data)
        {
            HttpResponseMessage response = await _httpConnector.PostAsync(url, data);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                return new KeyValuePair<bool, string>(false, error);
            }
            string reponse = await response.Content.ReadAsStringAsync();
            return new KeyValuePair<bool, string>(true, reponse);
        }
    }
}
