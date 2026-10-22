namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL
{
    public class CustomerDetails
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Title { get; set; }
        public ContactDetails ContactDetails { get; set; }
        public Addressdetail[] AddressDetails { get; set; }
        public LoyaltyMember LoyaltyMember { get; set; }
    }

    public class ContactDetails
    {
        public EmailDetails[] EmailDetails { get; set; }
        public PhoneDetails[] PhoneDetails { get; set; }
    }

    public class EmailDetails
    {
        public string Usage { get; set; }
        public string EmailAddress { get; set; }
        public string Description { get; set; }
        public string Valid { get; set; }
        public string Primary { get; set; }
        public string EmailAddressId { get; set; }
        public string LastUpdatedSource { get; set; }
    }

    public class PhoneDetails
    {
        public string ContactSubType { get; set; }
        public string Usage { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Description { get; set; }
        public string Valid { get; set; }
        public string Primary { get; set; }
        public string PhoneId { get; set; }
        public string LastUpdatedBy { get; set; }
    }

    public class LoyaltyMember
    {
        public string MemberType { get; set; }
        public string ProgramName { get; set; }
        public string Status { get; set; }
        public string Id { get; set; }
        public string MemberNumber { get; set; }
        public string FlyingClubCardNumber { get; set; }
        public DateTime StartDate { get; set; }
        public string HeadOfHouseholdMemberNumber { get; set; }
        public string FastTrackId { get; set; }
        public string GoldCampaignId { get; set; }
        public string SilverCampaignId { get; set; }
        public string MediaCode { get; set; }
        public string EnrollmentChannel { get; set; }
        public bool TermsAndConditionsAccepted { get; set; }
    }

    public class Addressdetail
    {
        public string AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string CompanyName { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Valid { get; set; }
        public string Primary { get; set; }
        public string AddressId { get; set; }
    }

}
