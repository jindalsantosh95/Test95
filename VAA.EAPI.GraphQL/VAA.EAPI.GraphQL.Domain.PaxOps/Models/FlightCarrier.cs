using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:FlightCarrier")]
    public class FlightCarrier
    {
        [GraphQLDescription("Flight number excluding carrier code.")]
        public string FlightNum { get; set; }

        public string CarrierCode { get; set; }

        [GraphQLDescription("Carrier display name.")]
        public string CarrierName { get; set; }

        public static explicit operator FlightCarrier(Infrastructure.Models.CheckInMS.FlightCarrier source) => new()
        {
            FlightNum = source.FlightNum,
            CarrierCode = source.CarrierCode,
            CarrierName = source.CarrierName
        };
    }
}
