namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class CabinClass
    {
        public string Code { get; set; }
        public string ProductCode { get; set; }
        public string SeatAssignmentType { get; set; }
        public string SeatStatus { get; set; }
        public string Name { get; set; }

        public static explicit operator CabinClass(Infrastructure.Proxy.DeltaMyTrips.CabinClass? source)
        {
            if (source == null) return null;
            return new CabinClass
            {
                Code = source.Code,
                Name = source.Name,
                ProductCode = source.ProductCode
            };
        }
    }
}