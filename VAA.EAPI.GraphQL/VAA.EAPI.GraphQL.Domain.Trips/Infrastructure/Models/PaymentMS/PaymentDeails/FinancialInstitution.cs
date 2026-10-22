using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.Customer;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.PaymentDeails
{
    [GraphQLName("GraphqlTripsFinancialInstitution")]
    public class FinancialInstitution
    {
        public string AccountHolderName { get; set; }
        public string ProviderId { get; set; }
        public string ReturnUrl { get; set; }
        public Address Address { get; set; }
    }
}
