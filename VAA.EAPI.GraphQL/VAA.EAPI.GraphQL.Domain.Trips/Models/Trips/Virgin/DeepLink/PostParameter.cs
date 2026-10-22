using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.DeepLink
{
    [GraphQLName("TripLinkParameters")]
    public class PostParameter
    {
        public string ParamName { get; set; }
        public string ParamType { get; set; }
        public string ParamValue { get; set; }

        public static explicit operator PostParameter(Infrastructure.Models.TripMS.DeepLink.PostParameter source)
        {
            if (source == null)
                return null;
            return new PostParameter()
            {
                ParamName = source.ParamName,
                ParamType = source.ParamType,
                ParamValue = source.ParamValue
            };
        }

    }
}
