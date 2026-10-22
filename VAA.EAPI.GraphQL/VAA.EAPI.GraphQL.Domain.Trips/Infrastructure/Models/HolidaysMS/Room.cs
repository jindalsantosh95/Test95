namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class Room
    {
        public string RoomName { get; set; }
        public string RoomType { get; set; }
        public string BoardBasis { get; set; }
        public string RoomNumber { get; set; }
        public int RoomCount { get; set; }
        public int PaxCount { get; set; }
        public TravellerInfo TravellerInfo { get; set; }
        public RateInfo Rate { get; set; }
        public string MealPlanName { get; set; }

    }
}