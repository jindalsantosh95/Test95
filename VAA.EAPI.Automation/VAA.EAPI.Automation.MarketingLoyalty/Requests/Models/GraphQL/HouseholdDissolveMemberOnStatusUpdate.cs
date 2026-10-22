namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL
{
    public class HouseholdDissolveMemberOnStatusUpdate
    {
        public string CustomerId { get; set; }

        public string OldStatus { get; set; }

        public string NewStatus { get; set; }

        public string MemberType { get; set; }

        public string HeadOfGroupMemberNumber { get; set; }

        public string GroupId { get; set; }

        public string InitiatedBy { get; set; }

    }
}
