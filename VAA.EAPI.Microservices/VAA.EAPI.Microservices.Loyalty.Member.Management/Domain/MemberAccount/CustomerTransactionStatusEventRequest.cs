namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.MemberAccount
{
    public class CustomerTransactionStatusEventRequest
    {
        public FlyingClubMemberDocument FlyingClubMemberDocument { get; set; }
        public string CustomerId { get; set; }
    }
    public class FlyingClubMemberDocument
    {
        public string Id { get; set; }
        public string CustomerId {  set; get; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string Status { get; set; }
    }
}

