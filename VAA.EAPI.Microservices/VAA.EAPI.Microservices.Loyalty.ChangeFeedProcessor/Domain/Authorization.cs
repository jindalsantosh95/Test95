
namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Domain
{
    public class Authorization
    {
        public string AuthorizerCustomerId { get; set; }

        public Consent[] Consents { get; set; }

        public string CountryCode { get; set; }

        public string AuthorizerDateOfBirth { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string RelationType { get; set; }

        public string AuthorizerFCMemberNumber { get; set; }

        public string AuthorizationId { get; set; }

        public string SourceIdentifier { get; set; }

        public string SourceSystem { get; set; }
    }
}
