namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class PassengerType
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public static explicit operator PassengerType(Infrastructure.Proxy.DeltaMyTrips.PassengerType source)
        {
            if (source == null)
                return null;

            return new PassengerType
            {
                Category = source.Category,
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}
