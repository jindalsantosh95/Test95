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
    public class InRuleMutationProvider : IInRuleMutationProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public InRuleMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<CreditPointsResponse> CalculatePoints(AirlineEarnedPointsRequest request)
        {
            string uri = ConfigReader.Get(Common.Constants.Environment.InRuleUrl).TrimEnd('/') + Common.Constants.Application.AirlinePointsEarnCalculatorEndPoint;
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.InRuleAppId));

            Dictionary<string, string> headers = new()
                            {
                                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.InRuleOcpSubID) }
                            };

            return (await _connector.SendAsync<CreditPointsResponse>(uri, headers, token, HttpMethod.Post, Common.Serialisation.Json.Serialize(request))).Response;
        }
    }
}
