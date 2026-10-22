using HotChocolate;
using HotChocolate.Types;

#nullable enable

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.DeepLink
{
    [GraphQLName("TripLinkDetail")]
    public class LinkDetail
    {
        public EndpointDetail? Endpoint { get; set; }
        public IEnumerable<PostParameter> Parameters { get; set; } = [];
        [GraphQLType(typeof(ListType<AnyType>))]
        public IEnumerable<dynamic> Items { get; set; } = [];

        public static explicit operator LinkDetail?(Infrastructure.Models.TripMS.DeepLink.DeepLinkDetail source)
        {
            if (source == null)
                return null;

            return new LinkDetail()
            {
                Endpoint = source.Endpoint is null ? null : (EndpointDetail)source.Endpoint,
                Parameters = source.Parameters?.Select(p => (PostParameter)p).ToList() ?? [],
                Items = source.Items ?? []
            };
        }
    }
}
