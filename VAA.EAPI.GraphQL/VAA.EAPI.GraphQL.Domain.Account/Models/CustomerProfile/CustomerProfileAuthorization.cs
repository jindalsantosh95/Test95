namespace VAA.EAPI.GraphQL.Domain.Account.Models.CustomerProfile
{
    public class CustomerProfileAuthorization
    {
        public string AuthorizerCustomerId { get; set; }
        public IEnumerable<CustomerProfileConsent> Consents { get; set; }
        public string AuthorizerDateOfBirth { get; set; }
        public string RelationType { get; set; }
        public string AuthorizationId { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
    }
}
