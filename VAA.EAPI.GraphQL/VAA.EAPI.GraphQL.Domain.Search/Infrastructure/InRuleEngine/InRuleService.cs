using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.InRuleEngine;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.InRuleEngine
{
    public class InRuleService : IInRuleService
    {
        private static readonly Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Try<string>(Common.Constants.Environment.InRuleOcpSubID) } };
        private readonly ILogger<InRuleService> _logger;

        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public InRuleService(IHttpConnector connector, IOAuthTokenProvider tokenProvider, ILogger<InRuleService> logger)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<PointsEarnCalculatorResponse> AirlinePointsEarnCalculator(PointsEarnCalculatorRequest inruleRequest)
        {
            try
            {
                // Get a token
                var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Try<string>(Common.Constants.Environment.InRuleAppId));

                string pointsEarnURL = $"{ConfigReader.Try<string>(Common.Constants.Environment.InRuleUrl)}{Common.Constants.Application.AirlinePointsEarnCalculatorEndPoint}";

                return (await _connector.SendAsync<PointsEarnCalculatorResponse>(pointsEarnURL, headers, token, HttpMethod.Post, Json.Serialize(inruleRequest))).Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex.Message}", ex);
                return null;
            }
        }



    }
}
