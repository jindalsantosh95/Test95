using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Payment
{
    public class PaymentSessionBasketMSRequest
    {
        public decimal AmountToPay { get; set; }
        public string PaymentLink { get; set; }
        public string ParentPaymentReferenceId { get; set; }
        public string PaymentReferenceId { get; set; }
        public string HppPaymentStatus { get; set; }
        public LoyaltyPointsEquivalentCurrency SelectedPayWithPointsOption { get; set; }
    }
}
