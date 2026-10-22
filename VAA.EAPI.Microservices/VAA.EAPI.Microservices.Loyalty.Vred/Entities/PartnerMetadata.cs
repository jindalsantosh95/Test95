
namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities
{
    public class PartnerMetadata
    {
        public Address Address { get; set; }
        public string Gender { get; set; }
        public string JoinDatetime { get; set; }
        public string MemberType { get; set; }
        public TierInfo TierInfo { get; set; }
        public string Schema { get; set; }
    }
}
