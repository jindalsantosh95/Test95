namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment
{
    public class AddressDetail
    {
        public enum AddressTypes
        {
            Work,
            Home,
            Business
        }
        public AddressTypes? AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public bool? AddressVerified { get; set; }
        public string CompanyName { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string PreferredFlag { get; set; }
        public string? CountryName { get; set; }
        public string? MetroArea { get; set; }
    }
}
