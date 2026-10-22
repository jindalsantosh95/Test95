namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class Address
    {
        public string ContactType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string CityName { get; set; }
    }
}
