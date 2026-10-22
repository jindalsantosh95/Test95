using NodaTime;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class HotelProduct : Product
    {
        public string PropertyId { get; set; }
        public string PropertyName { get; set; }
        public LocalDate CheckIn { get; set; }
        public LocalDate CheckOut { get; set; }
        public string RoomType { get; set; }
        public string RatePlan { get; set; }
        public TotalPrice TotalPrice { get; set; }
        public string Status { get; set; }
    }
}