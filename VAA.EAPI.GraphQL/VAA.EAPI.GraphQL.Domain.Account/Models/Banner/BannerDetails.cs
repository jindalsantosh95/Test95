using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Banner
{
    public class BannerDetails
    {
        [GraphQLName("FCNumber")]
        public string MemberNumber { get; set; }
        [GraphQLName("Banners")]
        public IEnumerable<Banners> Banners { get; set; }
    }
}
