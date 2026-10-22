using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.ManageAccount
{
    public class UpdateVredServiceHandler : IRequestHandler<UpdateVredServiceQuery, bool>
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
        private readonly ILogger<VredServiceHandler> _logger;
        private readonly ITokenHelper _tokenHelper;

        public UpdateVredServiceHandler(IHttpConnector httpConnector, ILogger<VredServiceHandler> logger, ITokenHelper tokenHelper)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
        }

        public async Task<bool> Handle(UpdateVredServiceQuery request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            string url = $"{ConfigReader.Get(Constants.Environment.UpdateVredurlSettingName)}{"?"}{request.Content}";
            string vRedprofileContent = JsonConvert.SerializeObject(request.MemberDetailsRequest, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
            });

            KeyValuePair<bool, string> result = await PutRequest(url, await _tokenHelper.GetToken(vredTokenCacheKey, vredAuthenticationUrl, new FormUrlEncodedContent(clientCredentials)), vRedprofileContent, cancellationToken);
            if (!result.Key)
            {
                VRedErrorResponse redProfileError = JsonConvert.DeserializeObject<VRedErrorResponse>(result.Value);
                _logger.LogInformation($"Error in Updating VRED Member Details {redProfileError.Message}");
            }
            return result.Key;
        }

        private async Task<KeyValuePair<bool, string>> PutRequest(string url, string bearerToken, string content, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Authorization = new AuthenticationHeaderValue(Constants.Application.AuthenticationScheme, bearerToken);
            request.Content = new StringContent(content, Encoding.UTF8, Constants.Application.ContenTypeapplicationjson);
            HttpResponseMessage httpResponse = await _httpConnector.SendAsync(request, cancellationToken);
            if (!httpResponse.IsSuccessStatusCode)
            {
                string error = await httpResponse.Content.ReadAsStringAsync();
                return new KeyValuePair<bool, string>(false, error);
            }
            string response = await httpResponse.Content.ReadAsStringAsync();
            return new KeyValuePair<bool, string>(true, response);
        }

    }
}
