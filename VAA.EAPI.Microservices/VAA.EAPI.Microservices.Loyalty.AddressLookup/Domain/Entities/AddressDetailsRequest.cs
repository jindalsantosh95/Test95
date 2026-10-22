namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities
{
    public class AddressDetailsRequest
    {
        public string CountryCode { get; set; }
        public string PostCode { get; set; }
        public string AddressLine1 { get; set; }
    }
}
