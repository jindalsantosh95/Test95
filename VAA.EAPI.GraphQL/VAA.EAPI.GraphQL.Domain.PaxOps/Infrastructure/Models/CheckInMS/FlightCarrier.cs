namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS
{
    public class FlightCarrier
    {
        public string FlightNum { get; set; }
        public string CarrierCode { get; set; }
        public string CarrierName { get; set; }

        public static explicit operator FlightCarrier(VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips.CarrierDetails carrierDetails)
        {
            return new FlightCarrier
            {
                FlightNum = carrierDetails?.FlightNumber,
                CarrierCode = carrierDetails?.CarrierCode,
                CarrierName = carrierDetails?.CarrierName
            };
        }
    }
}