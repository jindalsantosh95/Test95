namespace VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment
{
    public class AccountPhoneDetail
    {
        public enum AccountPhoneTypes
        {
            Landline,
            Mobile
        }
        public AccountPhoneTypes? PhoneType { get; set; }
        public enum AccountPhoneUsages
        {
            Business,
            Personal
        }
        public AccountPhoneUsages? Usage { get; set; }
        public string? CountryCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PreferredFlag { get; set; }
        public string? Description { get; set; }
    }
}
