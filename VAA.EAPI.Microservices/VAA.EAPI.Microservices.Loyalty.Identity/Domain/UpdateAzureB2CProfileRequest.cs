
namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain
{
    public class UpdateAzureB2CProfileRequest
    {
        public string MemberNumber { get; set; }
        public string CurrentEmailId { get; set; }
        public string NewEmailId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AccountStatus { get; set; }
    }
}
