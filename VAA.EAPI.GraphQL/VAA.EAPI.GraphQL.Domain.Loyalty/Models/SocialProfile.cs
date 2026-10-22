
namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class SocialProfile
    {
        public string LastUpdatedBy { get; set; }

        public string LastUpdatedOn { get; set; }

        public string Profile { get; set; }

        public string ProfileValue { get; set; }

        public string CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public string ProfileId { set; get; }

        public string SourceIdentifier { get; set; }

        public string SourceSystem { get; set; }
    }
}
