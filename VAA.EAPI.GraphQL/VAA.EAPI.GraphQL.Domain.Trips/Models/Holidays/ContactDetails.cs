using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayPropertyContact")]
    public class ContactDetails
    {
        public IEnumerable<string> Emails { get; set; }
        public IEnumerable<Phone> Phones {  get; set; }
    }
}
