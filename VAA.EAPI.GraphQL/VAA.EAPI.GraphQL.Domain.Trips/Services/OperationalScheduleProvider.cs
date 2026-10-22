using System.Globalization;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OperationalSchedule;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using CommonGraphql = VAA.EAPI.GraphQL.Common;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public class OperationalScheduleProvider : IOperationalScheduleProvider
    {
        private static readonly Dictionary<string, string> _headers =
            new()
            { { CommonGraphql.Constants.Application.OcpKeyHeader, ConfigReader.Get(CommonGraphql.Constants.Environment.FlightOpsOcpKey) } };

        protected readonly IHttpConnector _connector;

        public OperationalScheduleProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<OperationalScheduleResponse> GetOperationalScheduleAsync(OperationScheduleRequest request)
        {
            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.FlightOpsUrl).TrimEnd('/')}/operationalschedule";

            return (await _connector.SendAsync<OperationalScheduleResponse>(url, _headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;
        }
    }
}
