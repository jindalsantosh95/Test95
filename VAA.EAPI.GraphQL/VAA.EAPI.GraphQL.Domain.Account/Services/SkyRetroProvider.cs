using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.SkyMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class SkyRetroProvider<T> : ISkyRetroProvider<T>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        private readonly static Dictionary<string, string> headersSkyMS = new()
        {
            {Application.OcpKeyHeader, ConfigReader.Get(Environment.CommonEAPIOcpAPIMKey)}
        };

        public SkyRetroProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<T> CheckClaimEligibility(CustomerDTO customerInfo, SkyFlightsPointsClaimRequest flightTicketInfo)
        {
            return (await _connector.SendAsync<T>($"{ConfigReader.Get(Environment.SkyMsGenericUrl)}{Application.SkyMsCreateAutoRetroUrl}", headersSkyMS, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.SkyMsAppId)), HttpMethod.Post, Json.Serialize(new SkyEligibilityCheckRequest
            {
                TicketInfo = (TicketInfo)(customerInfo, flightTicketInfo)
            }))).Response;
        }

        public async Task<T> FeedBackUpdate(CheckEligibilityClaimResponse skyResponse, SegmentResponse inRuleResponse)
        {
            return (await _connector.SendAsync<T>($"{ConfigReader.Get(Environment.SkyMsGenericUrl)}{Application.SkyMsFeedbackRequestUrl}", headersSkyMS, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.SkyMsAppId)), HttpMethod.Post, Json.Serialize(new SkyFeedbackRequest
            {
                FeedbackRequest = (FeedbackRequest)(skyResponse, inRuleResponse)
            }))).Response;
        }
    }
}
