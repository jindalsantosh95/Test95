using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredStatements;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services
{
    public class VredStatementServiceHandler : IRequestHandler<VredStatementsServiceQuery, VredStatementResponse>
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

        public VredStatementServiceHandler(IHttpConnector httpConnector, ITokenHelper tokenHelper)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
        }

        public async Task<VredStatementResponse> Handle(VredStatementsServiceQuery request, CancellationToken cancellationToken)
        {
            var response = Json.Deserialize<VredStatementResponse>(await _httpConnector.GetAsync<string>(
                            $"{ConfigReader.Get(Constants.Environment.VRedStatementEndpointUrl)}{Constants.Application.ParterUserId + request.MemberNumber}",
                            headers: new Dictionary<string, string>(),
                            bearerToken: await _tokenHelper.GetToken(vredTokenCacheKey, vredAuthenticationUrl, new FormUrlEncodedContent(clientCredentials)))
                            );

            response.Statements.ToList().ForEach(x => x.VredDetail = JsonConvert.DeserializeObject<Detail>(x.Details));

            foreach (var stat in response.Statements.Where(stat => stat.VredDetail != null && string.IsNullOrEmpty(stat.VredDetail.TransactionId)))
            {
                stat.VredDetail.TransactionId = Guid.NewGuid().ToString();
            }

            return response;
        }
    }
}
