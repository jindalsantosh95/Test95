using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Models;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Vred;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.VredMs
{
    public class UpdateVredCustomerDetailsResponseQueryHandler : IVredCustomerDetailsResponseQuery
    {
        private readonly IHttpConnector _httpConnector;

        public UpdateVredCustomerDetailsResponseQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }
        public async Task<UpdateVredCustomerDetailsRS> Handle(UpdateVredCustomerDetailsResponseQuery request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.CorrelationId.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(request.CorrelationId));

            string token;

            if (string.IsNullOrEmpty(Constants.Application.VRedMSTokenCached))
            {
                token = await GetAuthToken();
            }
            else
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken decodedValue = handler.ReadJwtToken(Constants.Application.VRedMSTokenCached);
                DateTime jwtTokenEpiryDate = decodedValue.ValidTo;
                DateTime jwtTokenEpiryDateMinusFiveMinutes = jwtTokenEpiryDate.AddMinutes(-5);
                if (jwtTokenEpiryDateMinusFiveMinutes > DateTime.UtcNow)
                {
                    token = Constants.Application.VRedMSTokenCached;
                }
                else
                {
                    token = await GetAuthToken();
                }
            }

            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.VRedMSSubscriptionKey)}
            };

            var url = $"{ConfigReader.Get(Constants.Environment.UpdateVredCustomerDetailsResponseUrl)}/{request.CorrelationId}";

            var httpResponseMessage = await _httpConnector.SendAsync(url, headers, token, HttpMethod.Get, string.Empty);

            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return new UpdateVredCustomerDetailsRS { UpdateVredCustomerDetailsResponse = Json.Deserialize<UpdateVredCustomerDetailsQueryResponse>(response) };
            else
                return new UpdateVredCustomerDetailsRS
                {
                    Error = new List<ErrorDetails>
                    {
                        new ErrorDetails
                        {
                            Code = httpResponseMessage.StatusCode.ToString(),
                            Message = $"UpdateVredCustomerDetailsResponse endpoint returned {(int)httpResponseMessage.StatusCode} {httpResponseMessage.StatusCode} for CorrelationId {request.CorrelationId}."
                        }
                    }
                };
        }

        private async Task<string> GetAuthToken()
        {
            string tokenUri = $"{ConfigReader.Get(Constants.Environment.AzureADEndpoint)}{Constants.Application.AuthTokenValue}";
            string clientId = ConfigReader.Get(Constants.Environment.LoyaltyMbrMgmtAppId);
            Constants.Application.LoyaltyMbrMgmtClientSecretCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Environment.LoyaltyMbrMgmtClientSecret, Constants.Application.LoyaltyMbrMgmtClientSecretCached);
            Constants.Application.VredMSResourceIdCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Environment.VredMSAppId, Constants.Application.VredMSResourceIdCached);

            var clientCredentials = new Dictionary<string, string>
            {
                { Constants.Application.ClientId, clientId },
                { Constants.Application.ClientSecret, Constants.Application.LoyaltyMbrMgmtClientSecretCached },
                { Constants.Application.Resource, Constants.Application.VredMSResourceIdCached },
                { Constants.Application.GrantType, Constants.Application.ClientCredentials }
            };

            var httpResponse = await _httpConnector.PostAsync(tokenUri, new FormUrlEncodedContent(clientCredentials));
            var responseMessage = await httpResponse.Content.ReadAsStringAsync(CancellationToken.None);

            if (!httpResponse.IsSuccessStatusCode)
                throw new HttpRequestException($"Auth token endpoint returned {(int)httpResponse.StatusCode} {httpResponse.StatusCode}.");

            var accessToken = Json.Deserialize<Token>(responseMessage)?.AccessToken;

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new InvalidOperationException("Auth token endpoint response did not contain an access_token.");

            Constants.Application.VRedMSTokenCached = accessToken;
            return accessToken;
        }
    }
}

