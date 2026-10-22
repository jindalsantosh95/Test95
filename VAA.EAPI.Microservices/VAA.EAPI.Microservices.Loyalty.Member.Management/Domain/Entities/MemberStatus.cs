namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities
{
    public class MemberStatus
    {
        public string CustomerID { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public string MemberType { get; set; }
        public string GroupID { get; set; }
        public string HeadOfGroupMemberNumber { get; set; }
        public string InitiatedBy { get; set; }
    }
}
