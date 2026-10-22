namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class FlightCarrier
    {
        public string? FlightNum { get; set; }
        public string? CarrierCode { get; set; }
        public string? CarrierName { get; set; }

        public static explicit operator FlightCarrier((DeltaTrm.Carrier? carrier, string flightNum) source)
        {
            return new FlightCarrier
            {
                FlightNum = source.flightNum,
                CarrierCode = source.carrier?.Code,
                CarrierName = source.carrier?.Name ?? source.carrier?.Code
            };
        }
    }
}