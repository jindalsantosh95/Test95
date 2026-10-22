namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CheckEmailAndTier.Entities
{
    public partial class MembershipAttribute
    {
        public string SourceIdentifier { get; set; }

        public string SourceSystem { get; set; }

        public string CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public string LastUpdatedOn { get; set; }

        public string LastUpdatedBy { get; set; }

        public string AttributeId { get; set; }

        public string AttributeName { get; set; }

        public string Value { get; set; }
    }
}
