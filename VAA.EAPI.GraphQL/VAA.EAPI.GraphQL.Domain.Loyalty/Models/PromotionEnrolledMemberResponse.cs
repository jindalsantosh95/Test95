namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class PromotionEnrolledMemberResponse
    {
        public string PromotionId { get; set; }
        public string EnrolledMembersListFileURL { get; set; }
        public string Message { get; set; }
    }
}
