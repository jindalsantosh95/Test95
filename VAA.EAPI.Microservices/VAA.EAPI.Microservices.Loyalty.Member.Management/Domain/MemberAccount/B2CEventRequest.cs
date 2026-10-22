namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.MemberAccount
{
    public class B2CEventRequest
    {
        public string MemberNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CurrentEmailId { get; set; }
        public string NewEmailId { get; set; }
        public string AccountStatus { get; set; }
    }
}
