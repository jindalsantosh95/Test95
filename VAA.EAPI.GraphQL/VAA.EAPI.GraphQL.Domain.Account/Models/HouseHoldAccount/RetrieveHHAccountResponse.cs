namespace VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount
{
    public class RetrieveHHAccountResponse
    {
        public string Name { get; set; }
        public string TotalPoints { get; set; }
        public LeaderInfo LeaderInfo { get; set; }
        public IEnumerable<LinkedMemberInfo> LinkedMemberInfo { get; set; }
        public IEnumerable<OtherLinkedMemberInfo> OtherLinkedMemberInfo { get; set; }
        public PageResponseType PageResponse { get; set; }
        public IEnumerable<InviteeInfo> InviteeInfo { get; set; }
        public InviterInfo InviterInfo { get; set; }
        public int TotalMemberandInviteesCount { get; set; }
    }

}
