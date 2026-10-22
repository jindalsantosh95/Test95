namespace VAA.EAPI.GraphQL.Domain.Account.Models.AddressLookupV3
{
    public sealed class AddressLookupV3Address
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public bool? AddressVerified { get; set; }
        public string Valid { get; set; }
    }
}
