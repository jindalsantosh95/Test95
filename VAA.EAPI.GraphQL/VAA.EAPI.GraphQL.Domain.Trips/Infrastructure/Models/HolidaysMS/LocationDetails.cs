using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.Holidays
{
    public class LocationDetails
    {
        public string DepotName { get; set; }
        public Address Address { get; set; }
        public GeoCoordinates GeoLocation { get; set; }
    }
}
