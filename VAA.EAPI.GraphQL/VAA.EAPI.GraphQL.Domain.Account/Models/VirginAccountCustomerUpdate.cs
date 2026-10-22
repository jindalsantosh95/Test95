namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class VirginAccountCustomerUpdate
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Countrycode { get; set; }
        public string TierCode { get; set; }
    }
}
