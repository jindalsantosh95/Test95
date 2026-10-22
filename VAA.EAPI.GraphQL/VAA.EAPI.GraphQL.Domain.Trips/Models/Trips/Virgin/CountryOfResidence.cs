
namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class CountryOfResidence
    {
        public string? Code { get; set; }

        public static explicit operator CountryOfResidence(Infrastructure.Models.TripMS.CountryOfResidence source)
        {
            if (source == null)
                return null;

            return new CountryOfResidence()
            {
                Code = source.Code,
            };
        }

        public static explicit operator CountryOfResidence(Infrastructure.Models.HolidaysMS.TripsCore.CountryOfResidence source)
        {

            if (source == null)
                return null;

            return new CountryOfResidence()
            {
                Code = source.Code,
            };
        }
    }
}
