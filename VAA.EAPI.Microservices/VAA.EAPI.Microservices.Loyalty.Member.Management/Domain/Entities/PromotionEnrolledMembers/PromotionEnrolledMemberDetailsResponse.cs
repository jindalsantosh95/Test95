
namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers
{
    public class PromotionEnrolledMemberDetailsResponse
    {
        public string PromotionId { get; set; }
        public string EnrolledMembersListFileURL { get; set; }

        public string Message { get; set; }
    }
}
