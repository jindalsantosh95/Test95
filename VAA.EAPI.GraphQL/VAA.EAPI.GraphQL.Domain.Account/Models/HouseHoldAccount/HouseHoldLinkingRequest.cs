namespace VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount
{
    public class HouseHoldLinkingRequest
    {
        public string InitiatedBy { get; set; }
        public HouseholdAccount HeadAccount { get; set; }
        public HouseholdAccount ChildAccount { get; set; }
    }
}
