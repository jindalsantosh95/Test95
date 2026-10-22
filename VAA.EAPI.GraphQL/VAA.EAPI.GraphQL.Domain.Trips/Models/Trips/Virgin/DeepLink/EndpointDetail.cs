using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.DeepLink
{
    [GraphQLName("TripEndpointDetail")]
    public class EndpointDetail
    {
        public string EndpointUrl { get; set; }
        public string Http_Method { get; set; }

        public static explicit operator EndpointDetail(Infrastructure.Models.TripMS.DeepLink.EndpointDetail source)
        {
            if (source == null)
                return null;
            return new EndpointDetail()
            {
                EndpointUrl = source.EndpointUrl,
                Http_Method = source.Http_Method
            };
        }
    }
}
