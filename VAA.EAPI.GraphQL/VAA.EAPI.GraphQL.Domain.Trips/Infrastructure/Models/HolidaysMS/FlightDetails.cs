using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class FlightDetails
    {
        public IEnumerable<Slice> Sectors { get; set; }
        public Reference AirlineLocator { get; set; }
        public bool IsBookingSourceAir4 { get; set; }

    }
}