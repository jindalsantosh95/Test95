using VAA.EAPI.Microservices.Retail.Basket.Domain.PointsCurrencyConverter;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public class PaymentSession
    {
        public decimal AmountToPay { get; set; }
        public string PaymentLink { get; set; }
        public string ParentPaymentReferenceId { get; set; }
        public string PaymentReferenceId { get; set; }
        public string HppPaymentStatus { get; set; }
        public LoyaltyPointsEquivalentCurrency SelectedPayWithPointsOption { get; set; }
    }
}