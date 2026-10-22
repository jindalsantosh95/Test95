namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold
{
    public class AdobeMemberInviteEmailCommand
    {
        public string Email { get; set; }
        public AdobeMemberInviteRequest AdobeInviteMemberRequest { get; set; }
    }
}
