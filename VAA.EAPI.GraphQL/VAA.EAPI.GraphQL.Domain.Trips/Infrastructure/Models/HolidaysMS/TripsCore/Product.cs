using VAA.EAPI.GraphQL.Domain.Trips.Models.Core;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore
{
    public partial class Product 
    {
        public string BookingType { get; set; }
        public Reference Reference { get; set; }
        public IEnumerable<Slice> Slices { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public string PropertyId { get; set; }
        public string PropertyName { get; set; }
        public Address Address { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public string RoomType { get; set; }
        public string RatePlan { get; set; }
        public TotalPrice TotalPrice { get; set; }
        public string Status { get; set; }
        public bool CheckInEligible { get; set; }
    }
}
