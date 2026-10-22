namespace VAA.EAPI.Microservices.Retail.Basket.Domain.Trips
{
    public class SeatAssignment
    {
        public string SegmentId { get; set; }
        public string SeatNumber { get; set; }
        public SeatCabin SeatClass { get; set; }
        public string PassengerId { get; set; }
        public string CustomerId { get; set; }
    }
}
