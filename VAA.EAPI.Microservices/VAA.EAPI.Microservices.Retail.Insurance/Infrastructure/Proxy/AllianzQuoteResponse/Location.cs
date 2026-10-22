namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteResponse
{
    public class Location
    {
        public string LocationCode { get; set; }
        public string LocationType { get; set; }
        public bool AutoRender { get; set; }
        public string Output { get; set; }
        public string OutputType { get; set; }
        public string Selector { get; set; }
        public LocationStatus Status { get; set; }
        public Offer[] Offers { get; set; }
        public string TemplateType { get; set; }
        public string TemplateCode { get; set; }
        public string TreatmentCode { get; set; }
    }
}
