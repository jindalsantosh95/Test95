namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{

    public class HouseHoldLinkingRequest
    {
        public string InitiatedBy { get; set; }
        public HouseHoldAccount HeadAccount { get; set; }
        public HouseHoldAccount ChildAccount { get; set; }
    }
}
