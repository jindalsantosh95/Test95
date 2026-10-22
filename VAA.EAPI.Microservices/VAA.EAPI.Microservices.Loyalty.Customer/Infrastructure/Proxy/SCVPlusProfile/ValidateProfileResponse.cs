namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class ValidateProfileResponse
    {
        public ValidateProfileResponse(string memberNumber = default, string firstName = default, string lastName = default, string middleName = default, string status = default, string tier = default, string emailUnique = default, string countryCode = default, string onlineActivationFLag = default, string invalidEmail = default)
        {
            MemberNumber = memberNumber;
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
            Status = status;
            Tier = tier;
            EmailUnique = emailUnique;
            CountryCode = countryCode;
            OnlineActivationFLag = onlineActivationFLag;
            InvalidEmail = invalidEmail;
        }

        public string MemberNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Status { get; set; }
        public string Tier { get; set; }
        public string EmailUnique { get; set; }
        public string CountryCode { get; set; }
        public string OnlineActivationFLag { get; set; }
        public string InvalidEmail { get; set; }
    }
}
