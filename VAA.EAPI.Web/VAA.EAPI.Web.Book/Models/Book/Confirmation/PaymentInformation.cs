namespace VAA.EAPI.Web.Book.Models.Book.Confirmation
{
    public class PaymentInformation
    {
        public string PaymentStatus { get; set; }
        public object ObfusticatedCard { get; set; }
        public string CardType { get; set; }
        public double PaymentAmount { get; set; }
    }
}
