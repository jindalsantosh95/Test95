using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class Guardian
    {
        public Guardian(string name = default, DateTime? dateOfBirth = default, DateTime? consentDate = default, string guardianMemberNumber = default, Phone phoneDetails = default, Email emailDetails = default, bool? termsAndConditionsAccepted = false)
        {
            Name = name;
            DateOfBirth = dateOfBirth;
            ConsentDate = consentDate;
            GuardianMemberNumber = guardianMemberNumber;
            PhoneDetails = phoneDetails;
            EmailDetails = emailDetails;
            // use default value if no "termsAndConditionsAccepted" provided
            if (termsAndConditionsAccepted == null)
            {
                TermsAndConditionsAccepted = false;
            }
            else
            {
                TermsAndConditionsAccepted = termsAndConditionsAccepted;
            }
        }

        public string Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? ConsentDate { get; set; }
        public string GuardianMemberNumber { get; set; }
        public Phone PhoneDetails { get; set; }
        public Email EmailDetails { get; set; }
        public bool? TermsAndConditionsAccepted { get; set; }
    }
}
