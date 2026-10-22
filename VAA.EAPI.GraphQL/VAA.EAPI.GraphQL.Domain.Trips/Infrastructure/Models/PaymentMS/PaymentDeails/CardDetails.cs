using HotChocolate;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.Customer;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.PaymentDeails
{
    [GraphQLName("GraphqlTripsCardDetails")]
    public partial class CardDetails
    {
        public CardTypes? CardType { get; set; }
        public string CardNumber { get; set; }
        [OpenApiProperty(Description = "Is the card number provided already tokenised. This means that the airline has key to de-tokenise in order to take payment.")]
        public bool? IsTokenised { get; set; }
        public string IssueDate { get; set; }
        public string ExpiryDate { get; set; }
        public string CardholderName { get; set; }
        public string Cvv { get; set; }
        public Address BillingAddress { get; set; }
        public PaymentCardAuthentication AuthenticationData { get; set; }
        public string CardNetworkCode { get; set; }
        public string MaskedCardNumber { get; set; }
        public string CardBrandCode { get; set; }
    }
}
