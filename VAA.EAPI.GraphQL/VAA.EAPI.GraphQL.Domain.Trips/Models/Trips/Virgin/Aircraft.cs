using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    [GraphQLName("TripsAircraft")]
    public class Aircraft
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? SubfleetCode { get; set; }
        public string? PlaneName { get; set; }
        public string? TailNumber { get; set; }

        public static explicit operator Aircraft(Infrastructure.Models.TripMS.Aircraft source)
        {
            if (source == null)
                return null;

            return new Aircraft()
            {
                Code = source.Code,
                Name = source.Name,
                SubfleetCode = source.SubfleetCode,
                PlaneName = source.PlaneName,
                TailNumber = source.TailNumber
            };
        }

        public static explicit operator Aircraft(Infrastructure.Models.HolidaysMS.TripsCore.Aircraft source)
        {
            if (source == null)
                return null;

            return new Aircraft()
            {
                Code = source.Code,
                Name = source.Name,
                SubfleetCode = source.SubfleetCode,
                PlaneName = source.PlaneName,
                TailNumber = source.TailNumber
            };
        }
    }
}
