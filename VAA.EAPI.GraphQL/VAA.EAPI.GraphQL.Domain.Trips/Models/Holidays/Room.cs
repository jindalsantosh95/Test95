using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayPropertyRoom")]
    public class Room
    {
        public string RoomName { get; set; }
        public string RoomType { get; set; }
        [HotChocolate.GraphQLIgnore]
        public string BoardBasis { get; set; }
        public string Surcharges { get; set; }
        public string RoomNo { get; set; }
        [HotChocolate.GraphQLIgnore]
        public int RoomCount { get; set; }
        public int PaxCount { get; set; }
        public string MealPlanName { get; set; }
        public PartyComposition? PartyComposition { get; set; }

        public static explicit operator Room(Infrastructure.Models.HolidaysMS.Room source)
        {
            if (source == null)
                return null;

            return new()
            {
                BoardBasis = source.BoardBasis,
                RoomName = source.RoomName,
                RoomCount = source.RoomCount,
                MealPlanName = source.MealPlanName,
                RoomNo = source.RoomNumber,
                RoomType = source.RoomType,
                PaxCount = source.PaxCount,
                PartyComposition = (PartyComposition)source.TravellerInfo
            };

        }
    }
}
