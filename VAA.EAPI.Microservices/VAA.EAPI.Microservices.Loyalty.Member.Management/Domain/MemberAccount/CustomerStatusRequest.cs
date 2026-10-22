namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.MemberAccount
{
    public class CustomerStatusEventRequest
    {
        public string CustomerId { get; set; }
        public string Status { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
    }
}
