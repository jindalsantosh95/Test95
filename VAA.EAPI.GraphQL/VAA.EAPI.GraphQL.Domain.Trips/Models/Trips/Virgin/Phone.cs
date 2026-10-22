using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    [GraphQLName("TripsPhone")]
    public class Phone
    {
        public string? CountryCode { get; set; }
        public string? Number { get; set; }

        public static explicit operator Phone(Infrastructure.Models.TripMS.Phone source)
        {
            if (source == null)
                return null;

            return new Phone()
            {
                CountryCode = source.CountryCode,
                Number = source.Number
            };
        }

        public static explicit operator Phone(Infrastructure.Models.HolidaysMS.TripsCore.Phone source)
        {
            if (source == null)
                return null;

            return new Phone()
            {
                CountryCode = source.CountryCode,
                Number = source.Number
            };
        }
    }
}
