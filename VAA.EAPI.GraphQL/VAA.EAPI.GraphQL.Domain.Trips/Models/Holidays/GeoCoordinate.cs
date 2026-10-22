using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayLocationGeoCoordinates")]
    public class GeoCoordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        internal GeoCoordinates Clone()
        {
            return new GeoCoordinates
            {
                Latitude = Latitude,
                Longitude = Longitude
            };
        }
    }

}
