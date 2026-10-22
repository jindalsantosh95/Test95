
namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class HouseHoldDelinkingMutationRequest
    {
        public string InitiatedBy { get; set; }
        public string HeadCustomerId { get; set; }
        public string HeadMemberNumber { get; set; }
        public string ChildCustomerId { get; set; }
        public string ChildMemberNumber { get; set; }
    }
}
