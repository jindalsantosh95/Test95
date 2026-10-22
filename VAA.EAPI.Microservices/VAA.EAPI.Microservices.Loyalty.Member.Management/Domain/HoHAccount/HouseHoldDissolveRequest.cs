namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount
{
    public class HouseHoldDissolveRequest
    {
        public string? InitiatedBy { get; set; }
        public string? CustomerId { get; set; }
        public string? MemberId { get; set; }
        public string? GroupId { get; set; }
    }
}
