using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class ContactDetails
    {
        public IEnumerable<string> Emails { get; set; }
        public IEnumerable<Phone> Phones { get; set; }
    }
}
