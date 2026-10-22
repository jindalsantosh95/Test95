namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class CarrierDetails
    {
        public string FlightNumber { get; set; }
        public string CarrierCode { get; set; }
        public string CarrierName { get; set; }
        public string OperationalSuffix { get; set; }

        public static explicit operator CarrierDetails(string source)
        {
            if (source == null) return null;

            return new CarrierDetails()
            {
                CarrierCode = source
            };
        }

        public static explicit operator CarrierDetails((string airlineCarrier, string flightNumber) source)
        {
            var (airlineCarrier, flightNumber) = source;

            return new()
            {
                CarrierCode = airlineCarrier,
                FlightNumber = flightNumber
            };

        }
    }
}
