namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class MembershipChangeDocument
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string CustomerId { get; set; }
        public string DocType { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Active { get; set; }
        public string ApprovalDate { get; set; }
        public string MemberNumber { get; set; }
        public string TierPoints { get; set; }

    }
}
