using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Transaction
{
    public class UpdateVredTransactionServiceHandler : IUpdateVredTransactionServiceHandler
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

        public UpdateVredTransactionServiceHandler(IHttpConnector httpConnector, ITokenHelper tokenHelper)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
        }

        public async Task<UpdateTransactionResponse> Handle(UpdateVredTransactionService request, CancellationToken cancellationToken)
        {
            Constants.Application.VRedOcpApimSubscriptionKeyCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Environment.VRedOcpApimSubscriptionKeySettingName, Constants.Application.VRedOcpApimSubscriptionKeyCached);

            Dictionary<string, string> headers = new()
            {
                { Constants.Application.CacheControl, Constants.Application.NoCache },
                { Constants.Application.AcceptEncoding, Constants.Application.GzipDeflate },
                { Constants.Application.Accept, Constants.Application.ContenTypeapplicationjson },
                { Constants.Application.OcpApimSubscriptionKeyName, Constants.Application.VRedOcpApimSubscriptionKeyCached }
            };

            string requestContent = JsonConvert.SerializeObject(request.UpdateVredTransactionRequest, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
            });

            var httpResponseMessage = await _httpConnector.SendAsync(uri: ConfigReader.Get(Constants.Environment.VredTransactionAPIV2), headers: headers, bearerToken: await _tokenHelper.GetToken(vredTokenCacheKey, vredAuthenticationUrl, new FormUrlEncodedContent(clientCredentials)), HttpMethod.Put, requestContent);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return new UpdateTransactionResponse
                {
                    SuccessResponse = Json.Deserialize<TransactionResponse>(await httpResponseMessage.Content.ReadAsStringAsync()),
                    ErrorResponse = null
                };
            }
            else
            {
                return new UpdateTransactionResponse
                {
                    ErrorResponse = Json.Deserialize<TransactionErrorResponse>(await httpResponseMessage.Content.ReadAsStringAsync()),
                    SuccessResponse = null
                };
            }
        }
    }
}
