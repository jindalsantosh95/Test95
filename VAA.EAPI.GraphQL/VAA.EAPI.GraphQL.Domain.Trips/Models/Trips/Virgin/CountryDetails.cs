
namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class CountryDetails
    {
        public string? Code { get; set; }
        public string? Name { get; set; }

        public static explicit operator CountryDetails(Infrastructure.Models.TripMS.CountryDetails source)
        {
            if (source == null) return null;

            return new CountryDetails()
            {
                Code = source.Code,
                Name = source.Name
            };
        }

        public static explicit operator CountryDetails(Infrastructure.Models.HolidaysMS.TripsCore.CountryDetails source)
        {
            if (source == null) return null;

            return new CountryDetails()
            {
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}
