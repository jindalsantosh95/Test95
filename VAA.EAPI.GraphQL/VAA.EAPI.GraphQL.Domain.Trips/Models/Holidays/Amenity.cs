using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayHotelAmenity")]
    public class Amenity
    {
        public string AmenityName { get; set; }
        public string AmenityCode { get; set; }
    }

}
