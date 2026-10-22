using VAA.EAPI.Automation.Core.Extensions;
using static VAA.EAPI.Automation.MyAccount.Constants.Application;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation
{
    public sealed class AccountEntraEnrolMemberRequest
    {
        public string ChannelId { get; set; } = string.Empty;
        public string ChannelUserId { get; set; } = "123456";
        public string EnrolmentToken { get; set; } = string.Empty;

        public CustomerType CustomerType { get; set; }

        public EntraCustomer Customer { get; set; } = new();
        public EntraAddress Address { get; set; } = new();
        public EntraContact Contact { get; set; } = new();
        public EntraLoyaltyMember LoyaltyMember { get; set; } = new();
        public EntraPreferences Preferences { get; set; } = new();

        public string ReasonForTravel { get; set; } = "Business";

        public void Validate()
        {
            if (ChannelId.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'ChannelId' to be supplied, but found an empty or whitespace value.");
            if (EnrolmentToken.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'EnrolmentToken' to be supplied, but found an empty or whitespace value.");
            if (ChannelUserId.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'ChannelUserId' to be supplied, but found an empty or whitespace value.");

            if (Customer is null)
                Assert.Fail("Expected 'Customer' to be supplied, but found null.");
            if (Address is null)
                Assert.Fail("Expected 'Address' to be supplied, but found null.");
            if (Contact is null)
                Assert.Fail("Expected 'Contact' to be supplied, but found null.");
            if (LoyaltyMember is null)
                Assert.Fail("Expected 'LoyaltyMember' to be supplied, but found null.");
            if (Preferences is null)
                Assert.Fail("Expected 'Preferences' to be supplied, but found null.");

            Customer.Validate();
            Address.Validate();
            Contact.Validate();
            LoyaltyMember.Validate();
            Preferences.Validate();
        }
    }

    public sealed class EntraCustomer
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public void Validate()
        {
            if (FirstName.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'FirstName' to be supplied, but found an empty or whitespace value.");
            if (LastName.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'LastName' to be supplied, but found an empty or whitespace value.");
            if (DateOfBirth.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'DateOfBirth' to be supplied, but found an empty or whitespace value.");
            if (Gender.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'Gender' to be supplied, but found an empty or whitespace value.");
            if (Password.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'Password' to be supplied, but found an empty or whitespace value.");
        }
    }

    public sealed class EntraAddress
    {
        public string AddressLine1 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;

        // defaults you don’t want to pass at call site
        public string AddressType { get; set; } = "Home";
        public string Region { get; set; } = "NY";
        public string PreferredFlag { get; set; } = "Y";
        public bool AddressVerified { get; set; } = true;

        public void Validate()
        {
            if (AddressLine1.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'AddressLine1' to be supplied, but found an empty or whitespace value.");
            if (City.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'City' to be supplied, but found an empty or whitespace value.");
            if (PostalCode.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'PostalCode' to be supplied, but found an empty or whitespace value.");
            if (CountryCode.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'CountryCode' to be supplied, but found an empty or whitespace value.");
        }
    }

    public sealed class EntraContact
    {
        public string EmailId { get; set; } = string.Empty;
        public string PhoneCountryCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // defaults you don’t want to pass
        public string EmailUsage { get; set; } = "Personal";
        public string EmailPreferredFlag { get; set; } = "Y";
        public string PhoneType { get; set; } = "Mobile";
        public string PhoneUsage { get; set; } = "Personal";
        public string PhonePreferredFlag { get; set; } = "Y";

        public void Validate()
        {
            if (EmailId.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'EmailId' to be supplied, but found an empty or whitespace value.");
            if (PhoneCountryCode.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'PhoneCountryCode' to be supplied, but found an empty or whitespace value.");
            if (PhoneNumber.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'PhoneNumber' to be supplied, but found an empty or whitespace value.");
        }
    }

    public sealed class EntraLoyaltyMember
    {
        public string EnrollmentChannel { get; set; } = string.Empty;
        public string MediaCode { get; set; } = string.Empty;

        // defaults you don’t want to pass
        public bool TermsAndConditionsAccepted { get; set; } = true;
        public string Status { get; set; } = "Active";
        public string MemberType { get; set; } = "Individual";
        public string ProgramName { get; set; } = "VirginAtlanticFlyingClub";

        public void Validate()
        {
            if (EnrollmentChannel.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'EnrollmentChannel' to be supplied, but found an empty or whitespace value.");
            if (MediaCode.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'MediaCode' to be supplied, but found an empty or whitespace value.");
        }
    }

    public sealed class EntraPreferences
    {
        public string VaaMarketingPermission { get; set; } = string.Empty;
        public string VaPartnersMarketingPermission { get; set; } = string.Empty;
        public string VaGroupMarketingPermissions { get; set; } = string.Empty;
        public string PreferredPartnerFFP { get; set; } = string.Empty;

        public void Validate()
        {
            if (VaaMarketingPermission.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'VaaMarketingPermission' to be supplied, but found an empty or whitespace value.");
            if (VaPartnersMarketingPermission.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'VaPartnersMarketingPermission' to be supplied, but found an empty or whitespace value.");
            if (VaGroupMarketingPermissions.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'VaGroupMarketingPermissions' to be supplied, but found an empty or whitespace value.");
            if (PreferredPartnerFFP.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'PreferredPartnerFFP' to be supplied, but found an empty or whitespace value.");
        }
    }
}

