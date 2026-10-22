using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities
{
    public class CreateCustomerEntities : IRequest<bool>
    {
        public string Id { get; set; }
        public string ChannelUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string PreferredGender { get; set; }
        public string PreferredName { get; set; }
        public string PreferredSalutation { get; set; }
        public string PreferredPronouns { get; set; }
        public string Gender { get; set; }
        public string Title { get; set; }
        public string AbbreviatedTitle { get; set; }
        public CustomerTypeEnum? CustomerType { get; set; }
        public string DateOfBirth { get; set; }
        public string Password { get; set; }
        public List<AddressDetail> AddressDetails { get; set; }
        public ContactDetails ContactDetails { get; set; }
        public string MemberType { get; set; }
        public string ProgramName { get; set; }
        public string MemberNumber { get; set; }
        public string FlyingClubCardNumber { get; set; }
        public string Status { get; set; }
        public string CampaignCode { get; set; }
        public bool? TestMember { get; set; }
        public string MediaCode { get; set; }
        public string EnrollmentChannel { get; set; }
        public bool TermsAndConditionsAccepted { get; set; }
        public GuardianDetails GuardianDetails { get; set; }
        public string PartnerCode { get; set; }
        public string StartDate { get; set; }
        public string StatusEnhancementCode { get; set; }
        public string PartnerStatusCode { get; set; }
        public string PartnerMembershipMappingId { get; set; }
        public string ReasonForTravel { get; set; }
        public List<Preferences> Preferences { get; set; }
        public List<CustomerAttribute> CustomerAttribute { get; set; }
        public List<SecurityQuestions> SecurityQuestions { get; set; }
        public string CustomerId { get; set; }
        public string FcNumber { get; set; }
    }
}
