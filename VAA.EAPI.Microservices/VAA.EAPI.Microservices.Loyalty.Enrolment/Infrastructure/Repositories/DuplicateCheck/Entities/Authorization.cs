namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.DuplicateCheck.Entities
{
    public class Authorization
    {
        public string AuthorizedCustomerid { get; set; }

        public Consent[] Consents { get; set; }

        public string CountryCode { get; set; }

        public string DateOfBirth { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string RelationType { get; set; }

        public string AuthorizerFCMemberNumber { get; set; }
    }
}
