namespace VAA.EAPI.Automation.Book.Models.OrderCreateMS
{
    public class CardMethod
    {
        public string cardNumber { get; set; }
        public string cardNetworkCode { get; set; }
        public string expiryDate { get; set; }
        public string cardholderName { get; set; }
        public string cvv { get; set; }
        public BillingAddress BillingAddress { get; set; }
    }
}
