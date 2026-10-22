namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold
{
    public class InviteHouseHoldCreateRequest
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
