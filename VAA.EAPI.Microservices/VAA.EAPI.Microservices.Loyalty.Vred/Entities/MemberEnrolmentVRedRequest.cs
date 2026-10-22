using MediatR;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities
{
    public class MemberEnrolmentVRedRequest : IRequest<VRedEnrolmentResponse>
    {
        public string PartnerUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Country { get; set; }
        public string Postcode { get; set; }
        public string DateOfBirth { get; set; }
        public bool TermsAndConditionsAccepted { get; set; }
        public string Channel { get; set; }
        public string PartnerMetadata { get; set; }
        public string State { get; set; }
        public string MarketingPreferencesSignedAt { get; set; }
        public string TermsAndConditionsSignedAt { get; set; }
        public bool MarketingAccepted { get; set; }

    }
}
