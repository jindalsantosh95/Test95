namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public sealed class TermsAndConditions
    {
        public bool Accepted { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; }
    }
}
