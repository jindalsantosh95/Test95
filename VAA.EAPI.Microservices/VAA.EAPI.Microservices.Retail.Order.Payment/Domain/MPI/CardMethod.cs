using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.MPI
{
    public class CardMethod
    {
        public string CardType { get; set; }

        public string TokenisedCardNumber { get; set; }

        public bool IsTokenised { get; set; }

        public string IssueDate { get; set; }

        public string ExpiryDate { get; set; }

        public string CardholderName { get; set; }

        public string Cvv { get; set; }

        public BillingAddress BillingAddress { get; set; }

        public PaymentCardAuthentication AuthenticationData { get; set; }
    }
}
