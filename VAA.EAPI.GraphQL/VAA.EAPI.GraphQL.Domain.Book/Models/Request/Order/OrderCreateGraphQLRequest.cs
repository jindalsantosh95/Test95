namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order
{
    public class OrderCreateGraphQLRequest
    {
        public string BasketId { get; set; }
        public string PaymentStatus { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public string BrowserInfo { get; set; }
        public bool CcCheckConsent { get; set; }
        public Dictionary<string, string> MetaPartner { get; set; }
    }
}
