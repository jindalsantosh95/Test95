namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class HotelDetails
    {
        public IEnumerable<Room> RoomDetails { get; set; }
        public string Postcode { get; set; }
        public string Name { get; set; }
        public string StreetName { get; set; }
        public string TownCity { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public bool VipBooking { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string HotelCode { get; set; }
        public string AccommodationType { get; set; }
        public string HotelAddress { get; set; }
        public Location Destination { get; set; }
        public int? NumberOfRooms { get; set; }

    }
}