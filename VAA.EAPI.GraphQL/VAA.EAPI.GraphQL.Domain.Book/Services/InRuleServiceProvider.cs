using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.InRuleEngine;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Services
{
    public class InRuleServiceProvider : IInRuleServiceProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Try<string>(Common.Constants.Environment.InRuleOcpSubID) } };

        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public InRuleServiceProvider(IHttpConnector connector, IOAuthTokenProvider tokenProvider)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }


        public async Task<PointsEarnCalculatorResponse> AirlinePointsEarnCalculator(PointsEarnCalculatorRequest request)
        {
            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Try<string>(Common.Constants.Environment.InRuleAppId));

            string pointsEarnURL = $"{ConfigReader.Try<string>(Common.Constants.Environment.InRuleUrl).TrimEnd('/')}{Common.Constants.Application.AirlinePointsEarnCalculatorEndPoint}";

            return (await _connector.SendAsync<PointsEarnCalculatorResponse>(pointsEarnURL, _headers, token, HttpMethod.Post, Json.Serialize(request))).Response;
        }



    }
}