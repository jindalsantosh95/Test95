namespace VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount
{
    public class InviteHouseHoldCreateDocRequest
    {
        public string GroupName { get; set; }
        public string InviterMemberId { get; set; }
        public string InviteeMemberId { get; set; }
        public string InviteeEmail { get; set; }
        public string InviteeFirstName { get; set; }
        public string InviteeLastName { get; set; }
        public string InviterFirstName { get; set; }
    }
}
