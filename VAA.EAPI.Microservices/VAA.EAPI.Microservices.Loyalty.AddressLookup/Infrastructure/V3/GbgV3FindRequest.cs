namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3
{
    public sealed class GbgV3FindRequest
    {
        public string Text { get; set; }
        public string Container { get; set; }
        public string CountryCode { get; set; }
        public int Limit { get; set; }
    }
}
