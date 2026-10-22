using MediatR;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Domain
{
    public class CreateCustomerEntities : IRequest<bool>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public string Gender { get; set; }

        public string Title { get; set; }

        public string CustomerType { get; set; }

        public string DateOfBirth { get; set; }

        public string Password { get; set; }


        public AddressDetail[] AddressDetails { get; set; }

        public ContactDetails ContactDetails { get; set; }

        public string CurrentTier { get; set; }

        public string ApprovalDate { get; set; }

        public string TierStartDate { get; set; }

        public string TierEndDate { get; set; }

        public string MemberType { get; set; }

        public string FlyingClubCardNumber { get; set; }

        public string CampaignCode { get; set; }

        public string MediaCode { get; set; }

        public string Status { get; set; }

        public string TestMember { get; set; }

        public string EnrollmentChannel { get; set; }

        public bool TermsAndConditionsAccepted { get; set; }

        public SecurityQuestion[] SecurityQuestions { get; set; }

        public string CustomerId { get; set; }

        public string FcNumber { get; set; }

        public bool MarketingAccepted { get; set; }
    }
}

