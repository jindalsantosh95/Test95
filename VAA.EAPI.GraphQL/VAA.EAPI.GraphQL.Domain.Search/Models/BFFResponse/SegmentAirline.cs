using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class SegmentAirline
    {
        public string code { get; set; }

        public string? name { get; set; }

        public static explicit operator SegmentAirline((string carrierCode, IEnumerable<AirlineInfo> airlineInfo) source)
        {
            var airline = source.airlineInfo.FirstOrDefault(ai => ai.CarrierCode == source.carrierCode);
            return new SegmentAirline
            {
                code = airline?.CarrierCode ?? source.carrierCode,
                name = airline?.CarrierName ?? source.carrierCode
            };
        }
    }
}
