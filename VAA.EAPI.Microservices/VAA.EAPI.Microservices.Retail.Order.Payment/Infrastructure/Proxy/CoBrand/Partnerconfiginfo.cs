namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand
{
    public class Partnerconfiginfo
    {
        public string MerchantNumber { get; set; }
        public string SyfPartnerId { get; set; }
        public string StoreFrontId { get; set; }
        public string SyfPartnerName { get; set; }
        public string ClientProductCode { get; set; }
        public string ClientProductDescription { get; set; }
        public string MerchantType { get; set; }
        public string CapabilityId { get; set; }
        public Productconfig ProductConfig { get; set; }
    }
}
