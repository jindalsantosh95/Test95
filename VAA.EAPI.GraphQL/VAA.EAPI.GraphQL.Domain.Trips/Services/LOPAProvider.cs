using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Seats;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public class LOPAProvider : ILOPAProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.LOPAMSOcpKey) } };

        private readonly IHttpConnector _connector;

        public LOPAProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<IEnumerable<SeatMap>> RetrieveAircraftDetailsAsync(string fleetTypeCode, string equipmentCode)
        {
            if (fleetTypeCode.IsNullOrWhiteSpace() || !new string[] { "333", "339", "351", "789" }.Contains(fleetTypeCode))
                fleetTypeCode = equipmentCode switch
                {
                    "L3D" => "333",
                    "L3W" => "333",
                    "L39" => "339",
                    "L5G" => "351",
                    "L5H" => "351",
                    "L9E" => "789",
                    "LH3" => "339",
                    _ => throw new NotImplementedException("No Aircraft Info Found.")
                };

            string retrieveAircraftInformationUrl = ConfigReader.Get(Common.Constants.Environment.LOPAMSUrl).Replace("{fleetTypeCode}", fleetTypeCode).Replace("{equipmentCode}", equipmentCode);

            return (await _connector.SendAsync<IEnumerable<SeatMap>>(retrieveAircraftInformationUrl, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }
    }
}