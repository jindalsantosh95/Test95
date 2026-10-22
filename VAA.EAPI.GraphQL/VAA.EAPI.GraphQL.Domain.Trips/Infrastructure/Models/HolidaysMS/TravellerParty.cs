using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class TravellerParty
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? LeadTraveller { get; set; }
        public bool? LeadDriver { get; set; }
        public string Title { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string TravellerType { get; set; }
        public IEnumerable<Address> Address { get; set; }
    }
}