namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold
{
    public class AdobeMemberInviteRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MembershipNumber { get; set; }
        public string FirstNameOfHouseholdLead { get; set; }
        public string FirstNameOfHouseholdMember { get; set; }
    }
}
