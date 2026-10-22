using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.Customer
{
    [GraphQLName("GraphqlTripsContacts")]
    public class Contacts
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Countrycode { get; set; }
        public string PhoneAreaNumber { get; set; }
        public string PhoneType { get; set; }
        public string CountryDailingCode { get; set; }
    }
}
