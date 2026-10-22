using Newtonsoft.Json.Linq;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Models
{
    public class FlyingClubBookings
    {
        public static explicit operator FlyingClubBookings(JObject source)
        {
            return new()
            {
                Bookings = source["FlightBookingsDetails"].ToArray().Select(x => (PNR)x)
            };
        }

        public string FCNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<PNR> Bookings { get; set; }
    }
}