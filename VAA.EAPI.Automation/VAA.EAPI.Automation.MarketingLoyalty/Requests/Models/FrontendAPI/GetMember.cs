namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI
{

    public class Address
    {
        public string addressType { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string addressLine3 { get; set; }
        public string companyName { get; set; }
        public string city { get; set; }
        public string postalCode { get; set; }
        public string region { get; set; }
        public string country { get; set; }
        public string primary { get; set; }
        public string postalAddressStatus { get; set; }
        public string addressID { get; set; }
        public string vaValidFlag { get; set; }
        public string vaTerritory { get; set; }
        public string addressRecordId { get; set; }
        public string vaqasVerified { get; set; }
    }

    public class Award
    {
        public string awardValue { get; set; }
        public string awardId { get; set; }
        public string awardNumber { get; set; }
        public string productId { get; set; }
        public string productName { get; set; }
        public string startDate { get; set; }
        public string expiryDate { get; set; }
    }

    public class Balance
    {
        public int? current { get; set; }
        public string usable { get; set; }
    }

    public class EmailAddress
    {
        public string usage { get; set; }
        public string emailId { get; set; }
        public string preferredFlag { get; set; }
        public string description { get; set; }
        public string vaValidFlag { get; set; }
        public string createdBy { get; set; }
        public string createdOn { get; set; }
        public string emailRecordId { get; set; }
        public string lastUpdatedDate { get; set; }
        public string lastUpdatedSource { get; set; }
    }

    public class EmailDetails
    {
        public string usage { get; set; }
        public string emailId { get; set; }
        public string preferredFlag { get; set; }
        public string description { get; set; }
        public string vaValidFlag { get; set; }
        public string createdBy { get; set; }
        public string createdOn { get; set; }
        public string emailRecordId { get; set; }
        public string lastUpdatedDate { get; set; }
        public string lastUpdatedSource { get; set; }
    }

    public class GuardianDetails
    {
        public string name { get; set; }
        public string dateOfBirth { get; set; }
        public string cardNumber { get; set; }
        public PhoneDetails phoneDetails { get; set; }
        public EmailDetails emailDetails { get; set; }
        public bool guardianTermsAndConditionsAccepted { get; set; }
        public string consentDate { get; set; }
    }

    public class PhoneDetails
    {
        public string phoneType { get; set; }
        public string usage { get; set; }
        public string countryCode { get; set; }
        public string phoneNumber { get; set; }
        public string preferredFlag { get; set; }
        public string description { get; set; }
        public string vaValidFlag { get; set; }
        public string phoneRecordId { get; set; }
    }

    public class PhoneNumber
    {
        public string phoneType { get; set; }
        public string usage { get; set; }
        public string countryCode { get; set; }
        public string phoneNumber { get; set; }
        public string preferredFlag { get; set; }
        public string description { get; set; }
        public string vaValidFlag { get; set; }
        public string phoneRecordId { get; set; }
    }

    public class Preference
    {
        public string preferenceType { get; set; }
        public string preferenceValue { get; set; }
    }

    public class GetMember
    {
        public string accountNumber { get; set; }
        public Name name { get; set; }
        public string tier { get; set; }
        public EmailAddress emailAddress { get; set; }
        public string accountStatus { get; set; }
        public int? tierPoints { get; set; }
        public int? milesBalance { get; set; }
        public string milesExpiryDate { get; set; }
        public DateTime? dateOfBirth { get; set; }
        public Address address { get; set; }
        public List<PhoneNumber> phoneNumbers { get; set; }
        public string pointDotComEligible { get; set; }
        public Balance balance { get; set; }
        public string authorisedUser { get; set; }
        public bool vredMember { get; set; }
        public bool lifeTimeGold { get; set; }
        public bool topMember { get; set; }
        public bool millionMiler { get; set; }
        public string centurion { get; set; }
        public bool profileChangeIn30days { get; set; }
        public bool emailInvalid { get; set; }
        public bool addressInvalid { get; set; }
        public string redUserId { get; set; }
        public string telephonePassword { get; set; }
        public string joinDate { get; set; }
        public string vATermsAndConditions { get; set; }
        public string nameChangeReason { get; set; }
        public string consecutiveYearsAtGold { get; set; }
        public string consecutiveYearsAtSilver { get; set; }
        public string pnrName { get; set; }
        public string preferedCardName { get; set; }
        public string gender { get; set; }
        public string vaMediaCode { get; set; }
        public string vaAuthorisedAccountUser { get; set; }
        public string millionMilerRanking { get; set; }
        public string millionMilerDateAchived { get; set; }
        public string lifeTimeGoldDateAchived { get; set; }
        public string topMemberDateAchived { get; set; }
        public string vaTestFlag { get; set; }
        public string goldStartDateOverride { get; set; }
        public string lastTransactionDate { get; set; }
        public string memberLinked { get; set; }
        public string memberType { get; set; }
        public string headOfHouseholdMemberNumber { get; set; }
        public GuardianDetails guardianDetails { get; set; }
        public List<Award> awards { get; set; }
        public List<Preference> preferences { get; set; }
    }
}
