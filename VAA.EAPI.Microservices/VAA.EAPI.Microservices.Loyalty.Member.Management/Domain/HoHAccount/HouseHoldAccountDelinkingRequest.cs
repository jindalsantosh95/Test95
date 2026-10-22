namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount
{
    public class HouseHoldAccountDelinkingRequest
    {
        public string InitiatedBy { get; set; }
        public HouseHoldAccount HeadAccount { get; set; }
        public HouseHoldAccount ChildAccount { get; set; }
    }
}
