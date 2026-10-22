namespace VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount
{
    public class HouseHoldInviteActionRequest
    {
        public string InviterFcNumber { get; set; }
        public string InviteeFcNumber { get; set; }
        public InviteActionType ActionType { get; set; }
    }
}
