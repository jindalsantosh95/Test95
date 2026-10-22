namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity
{
    public class RetrieveResponse
    {
        public string Id { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string CountryName { get; set; }
        public string CountryIso2 { get; set; }
    }
}
