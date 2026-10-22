using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.CheckInMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using CommonGraphql = VAA.EAPI.GraphQL.Common;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public sealed class TravelRequirementsQueryProvider : ITravelRequirementsQueryProvider
    {
        private readonly IHttpConnector _connector;

        public TravelRequirementsQueryProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<TravelRequirementsResponse> GetTravelRequirements(TravelRequirementsRequest travelRequirementsRequest)
        {
            ArgumentNullException.ThrowIfNull(travelRequirementsRequest);

            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.CheckInMSUrl)}/TravelRequirements";
            var headers = new Dictionary<string, string>
            {
                { CommonGraphql.Constants.Application.OcpKeyHeader, ConfigReader.Get(CommonGraphql.Constants.Environment.CheckInMsOcpKey) }
            };

            return (await _connector.SendAsync<TravelRequirementsResponse>(url, headers, string.Empty, HttpMethod.Post, Json.Serialize(travelRequirementsRequest))).Response;
        }
    }
}
