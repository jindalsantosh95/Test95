namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment
{
    public class PhoneDetail
    {
        public enum PhoneTypes
        {
            Landline,
            Mobile
        }
        public PhoneTypes? PhoneType { get; set; }
        public enum PhoneUsages
        {
            Business,
            Personal
        }
        public PhoneUsages? Usage { get; set; }
        public string? CountryCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PreferredFlag { get; set; }
        public string? Description { get; set; }
    }
}
