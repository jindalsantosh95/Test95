using HotChocolate;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.PaymentMS.Customer;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.PaymentMS.PaymentDetails
{
    [GraphQLName("GraphqlOCICardDetails")]
    public partial class CardDetails
    {
        public CardTypes? CardType { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CardholderName { get; set; }
        public string Cvv { get; set; }
        public Address BillingAddress { get; set; }
    }
}
