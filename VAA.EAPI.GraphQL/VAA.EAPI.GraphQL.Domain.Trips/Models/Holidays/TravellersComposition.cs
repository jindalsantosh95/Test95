using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayTravellersComposition")]
    public class TravellersComposition
    {
            public int? AdultsCount { get; set; }
            public int? ChildrenCount { get; set; }
            public int? InfantCount { get; set; }
            public IEnumerable<TravellerParty>? Travellers{ get; set; }

            public static explicit operator TravellersComposition(TravellerInfo source)
            {
                if (source == null) return null;

                return new TravellersComposition()
                {
                    AdultsCount = source.AdultCount,
                    ChildrenCount = source.ChildCount,
                    InfantCount = source.InfantCount,
                    Travellers = source.Travellers?.Select(x=>(TravellerParty)x)
                };
            }

        internal TravellersComposition Clone()
        {
            return new TravellersComposition
            {
                AdultsCount = this.AdultsCount,
                ChildrenCount = this.ChildrenCount,
                InfantCount = this.InfantCount,
                Travellers = this.Travellers?.Select(x => x.Clone()).ToList()
            };
        }
    }
}
               