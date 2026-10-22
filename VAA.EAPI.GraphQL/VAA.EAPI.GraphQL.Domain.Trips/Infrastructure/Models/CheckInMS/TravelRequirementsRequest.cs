using Graphql = VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.CheckInMS
{
    public sealed class TravelRequirementsRequest
    {
        public string PnrLocator { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }

        public static explicit operator TravelRequirementsRequest(Graphql.TripDetails source)
        {
            return new TravelRequirementsRequest
            {
                PnrLocator = source.PnrLocator,
                GivenNames = source.GivenNames,
                Surname = source.Surname
            };
        }
    }
}
