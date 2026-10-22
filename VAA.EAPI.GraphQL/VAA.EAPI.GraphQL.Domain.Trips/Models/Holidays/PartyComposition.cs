using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayPartyComposition")]
    public class PartyComposition
    {
        public int Adults { get; set; }
        public int Children { get; set; }
        public int Infant { get; set; }
        public int Oap { get; set; }

        public static explicit operator PartyComposition(TravellerInfo source)
        {
            if (source == null) return null;
            
            return new PartyComposition()
            {
                Adults = source.AdultCount,
                Children = source.ChildCount,
                Infant = source.InfantCount               
            };
        }
    }
}
