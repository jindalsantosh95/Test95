namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket
{
    public class CarrierDetails
    {
        public string FlightNumber { get; set; }
        public string CarrierCode { get; set; }
        public string CarrierName { get; set; }

        public static explicit operator CarrierDetails(Infrastructure.Models.CheckInMS.FlightCarrier carrier)
        {
            if (carrier == null) return null;

            return new CarrierDetails()
            {
                FlightNumber = carrier?.FlightNum,
                CarrierCode = carrier?.CarrierCode,
                CarrierName = carrier?.CarrierName
            };
        }
    }
}
