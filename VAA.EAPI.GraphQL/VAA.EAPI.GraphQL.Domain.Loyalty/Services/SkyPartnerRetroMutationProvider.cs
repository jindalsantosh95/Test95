using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class SkyPartnerRetroMutationProvider : ISkyPartnerRetroMutationProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public SkyPartnerRetroMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<FlyingActivityDetails> GetFlyingActivityDetails(SkyPartnerMSRequest ticketDetails)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.SkyMsGenericUrl) + Common.Constants.Application.SkyMsCreateAutoRetroUrl;
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.SkyMsAppId));

            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.SkyMSOcpKey) }
                            };

            return (await _connector.SendAsync<FlyingActivityDetails>($"{uri}", headers, token, HttpMethod.Post, Common.Serialisation.Json.Serialize(ticketDetails))).Response;
        }

        public async Task<SkyMSSuccessResponse> RetroCreditFeedback(SkyMSFeedbackRequest request)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.SkyMsGenericUrl) + Common.Constants.Application.SkyMsFeedbackRequestUrl;
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.SkyMsAppId));

            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.SkyMSOcpKey) }
                            };

            return (await _connector.SendAsync<SkyMSSuccessResponse>($"{uri}", headers, token, HttpMethod.Post, Common.Serialisation.Json.Serialize(request))).Response;
        }
    }
}
