using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;


namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services
{
    public class VirginPointsServiceHandler : IRequestHandler<VirginPointsServiceQuery, VRedResponse>
    {
        private static readonly string vredAuthenticationUrl = ConfigReader.Get(Constants.Environment.VRedAuthUrlSettingName);
        private static readonly string vredTokenCacheKey = $"{ConfigReader.Get(Constants.Environment.VRedAuthClientIdSettingName)}/{ConfigReader.Get(Constants.Environment.VredAuthSecretSettingName)}";
        private static readonly Dictionary<string, string> clientCredentials = new()
        {
            { Constants.Application.ClientId, ConfigReader.Get(Constants.Environment.VRedAuthClientIdSettingName)},
            { Constants.Application.ClientSecret, ConfigReader.Get(Constants.Environment.VredAuthSecretSettingName)},
            { Constants.Application.GrantType, "client_credentials"},
            { Constants.Application.Audience, ConfigReader.Get(Constants.Environment.VRedAuthAudienceSettingName) }
        };
        private readonly IHttpConnector _httpConnector;
        private readonly ITokenHelper _tokenHelper;

        public VirginPointsServiceHandler(IHttpConnector http, ITokenHelper tokenHelper)
        {
            _httpConnector = http ?? throw new ArgumentNullException(nameof(http));
            _tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper)); 
        }
        public async Task<VRedResponse> Handle(VirginPointsServiceQuery request, CancellationToken cancellationToken)
        {
            string token = await _tokenHelper.GetToken(vredTokenCacheKey, vredAuthenticationUrl, new FormUrlEncodedContent(clientCredentials));

            string url = $"{ConfigReader.Get(Constants.Environment.VRedEndpointUrl)}{ConfigReader.Get(Constants.Environment.UrlVersion)}";
            if (request != null)
            {
                url += $"{"?memberId=" + request.MemberID}";
            }

            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage httpResponse = await _httpConnector.SendAsync(httpRequest, cancellationToken);
            string response = await httpResponse.Content.ReadAsStringAsync();
            KeyValuePair<bool, string> res = new KeyValuePair<bool, string>(true, response);

            return JsonConvert.DeserializeObject<VRedResponse>(res.Value);
        }

    }
}
