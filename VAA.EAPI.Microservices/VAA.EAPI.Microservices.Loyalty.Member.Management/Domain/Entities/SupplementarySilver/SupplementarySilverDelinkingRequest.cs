using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SupplementarySilver
{
    public class SupplementarySilverDelinkingRequest
    {
        public string InitiatedBy { get; set; }
        public HouseHoldAccount HeadAccount { get; set; }
        public HouseHoldAccount ChildAccount { get; set; }
    }
}
