
namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class Gender
    {
        public string? Code { get; set; }
        public string? Name { get; set; }

        public static explicit operator Gender(Infrastructure.Models.TripMS.Gender source)
        {
            if (source == null) return null;

            return new Gender()
            {
                Code = source.Code,
                Name = source.Name
            };
        }

        public static explicit operator Gender(Infrastructure.Models.HolidaysMS.TripsCore.Gender source)
        {
            if (source == null) return null;

            return new Gender()
            {
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}
