namespace VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment
{
    public class AccountAddressDetail
    {
        public enum AccountAddressTypes
        {
            Work,
            Home,
            Business
        }
        public AccountAddressTypes? AddressType { get; set; }
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
    }
}
