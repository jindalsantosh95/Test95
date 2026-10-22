using VAA.EAPI.Automation.Core.Extensions;
using static VAA.EAPI.Automation.MyAccount.Constants.Application;

namespace VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation
{
    public sealed class EnrolmentRequest
    {
        public string EnrolmentToken { get; set; } = string.Empty;
        public string EnrollmentChannel { get; set; } = string.Empty;
        public string MediaCode { get; set; } = string.Empty;
        public string ChannelId { get; set; } = string.Empty;
        public bool TermsAndConditionsAccepted { get; set; } = true;

        public CustomerType CustomerType { get; set; }

        public EnrolmentCustomer Customer { get; set; } = new();
        public EnrolmentAddress Address { get; set; } = new();
        public EnrolmentContact Contact { get; set; } = new();
        public EnrolmentPreferences Preferences { get; set; } = new();

        public void Validate()
        {
            if (EnrolmentToken.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'EnrolmentToken' to be supplied, but found an empty or whitespace value.");
            if (EnrollmentChannel.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'EnrollmentChannel' to be supplied, but found an empty or whitespace value.");
            if (MediaCode.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'MediaCode' to be supplied, but found an empty or whitespace value.");
            if (ChannelId.IsNullOrWhiteSpace())
                Assert.Fail("Expected 'ChannelId' to be supplied, but found an empty or whitespace value.");

            if (Customer is null)
                Assert.Fail("Expected 'Customer' to be supplied, but found null.");
            Customer.Validate();

            if (Address is null)
                Assert.Fail("Expected 'Address' to be supplied, but found null.");
            Address.Validate();

            if (Contact is null)
                Assert.Fail("Expected 'Contact' to be supplied, but found null.");
            Contact.Validate();

            if (Preferences is null)
                Assert.Fail("Expected 'Preferences' to be supplied, but found null.");
            Preferences.Validate();
        }
    }

    public sealed class EnrolmentCustomer
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

    public sealed class EnrolmentAddress
    {
        public string AddressLine1 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;

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

    public sealed class EnrolmentContact
    {
        public string EmailId { get; set; } = string.Empty;
        public string PhoneCountryCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

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

    public sealed class EnrolmentPreferences
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

