namespace VAA.EAPI.Microservices.Retail.Insurance.Domain
{
    public class RequestDetail
    {
        public string BusinessUnit { get; set; }
        public string Currency { get; set; }
        public string Cxid { get; set; }
        public string Language { get; set; }
        public int OfferVersionId { get; set; }
        public IEnumerable<string> RequestCategories { get; set; }
        public string RequestedDate { get; set; }
        public string RequestOrigin { get; set; }
        public string RequestOriginPage { get; set; }
        public string SessionId { get; set; }
        public string PaymentMethod { get; set; }
        public string RedemptionPoints { get; set; }
    }
}