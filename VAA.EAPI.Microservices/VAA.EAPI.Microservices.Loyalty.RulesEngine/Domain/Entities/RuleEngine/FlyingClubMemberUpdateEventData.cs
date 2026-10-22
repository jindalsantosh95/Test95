
namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine
{
    public class FlyingClubMemberUpdateEventData
    {
        public string CustomerId { get; set; }
        public FlyingClubMemberDocument FlyingClubMemberDocument { get; set; }
    }
}
