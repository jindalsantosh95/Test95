using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Seat;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Services
{
    public class LOPAServiceProvider : ILOPAServiceProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.LOPAMSOcpKey) } };

        private readonly IHttpConnector _connector;

        public LOPAServiceProvider(IHttpConnector connector)
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

            string retrieveAircraftInformationUrl = ConfigReader.Get(Constants.Environment.LOPAMSUrl).Replace("{fleetTypeCode}", fleetTypeCode).Replace("{equipmentCode}", equipmentCode);

            return (await _connector.SendAsync<IEnumerable<SeatMap>>(retrieveAircraftInformationUrl, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

    }
}