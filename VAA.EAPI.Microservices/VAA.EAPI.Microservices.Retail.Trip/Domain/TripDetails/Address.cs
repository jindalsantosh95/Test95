namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class Address
    {
        public string AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string PreferredFlag { get; set; }
    }
}
