namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities
{
    public class DelinkAccountMemberPatchEvent
    {
        public string CustomerID { get; set; }
        public string InitiatedBy { get; set; }
        public string GroupID { get; set; }
    }
}
