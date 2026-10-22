namespace VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.OrderConfirmation
{
    public class Request
    {
        public string basketId { get; set; }
        public string paymentStatus { get; set; }
        public string deviceInfo { get; set; }
        public string ipAddress { get; set; }
        public string browserInfo { get; set; }
        public bool ccCheckConsent { get; set; }
    }

}
