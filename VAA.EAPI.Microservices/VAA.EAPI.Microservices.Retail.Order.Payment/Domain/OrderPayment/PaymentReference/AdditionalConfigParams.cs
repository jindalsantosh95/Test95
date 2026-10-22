namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference
{
    public class AdditionalConfigParams
    {
        public string FailureRedirectUrl { get; set; }
        public bool MultiProductFlag { get; set; }
        public string SessionID { get; set; }
        public string SuccessRedirectUrl { get; set; }
        public bool? IsInsuranceAdded { get; set; }

    }
}
