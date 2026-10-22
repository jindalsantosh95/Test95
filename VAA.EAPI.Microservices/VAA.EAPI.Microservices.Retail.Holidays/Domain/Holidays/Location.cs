using VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Holidays
{
    public class Location
    {
        public string DepotName { get; set; }
        public Address Address { get; set; }
        public GeoCoordinates GeoLocation { get; set; }
    }
}
