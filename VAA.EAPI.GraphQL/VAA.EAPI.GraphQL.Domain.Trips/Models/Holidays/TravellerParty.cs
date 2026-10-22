using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayTravellerParty")]
    public class TravellerParty
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? LeadDriver { get; set; }

        public static explicit operator TravellerParty(Infrastructure.Models.HolidaysMS.TravellerParty source)
        {
            if (source == null) return null;

            return new TravellerParty()
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                LeadDriver = source.LeadDriver,
            };
        }

        internal TravellerParty Clone()
        {
            return new TravellerParty
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                LeadDriver = this.LeadDriver
            };
        }
    }
}