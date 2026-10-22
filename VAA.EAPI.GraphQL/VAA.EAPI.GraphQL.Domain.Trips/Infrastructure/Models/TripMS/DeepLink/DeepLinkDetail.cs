namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.DeepLink
{
    public class DeepLinkDetail
    {
        public EndpointDetail Endpoint { get; set; }
        public IEnumerable<PostParameter> Parameters { get; set; }

        public IEnumerable<dynamic> Items { get; set; } = [];
    }
}
