using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.Customer;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.PaymentDeails
{
    [GraphQLName("GraphqlTripsPaymentPayer")]
    public class PaymentPayer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Contacts PayerContacts { get; set; }
        public Address PayerAddress { get; set; }
        public LoyaltyMemberDetails LoyaltyMemberDetails { get; set; }
        public bool LoyaltyMemberTravelling { get; set; }
        public string PayerType { get; set; }
    }
}
