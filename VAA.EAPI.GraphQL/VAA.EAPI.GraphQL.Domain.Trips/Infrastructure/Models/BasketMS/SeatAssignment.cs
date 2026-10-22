namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS
{
    public class SeatAssignment
    {
        public string SegmentId { get; set; }
        public string SeatNumber { get; set; }
        public SeatCabin SeatClass { get; set; }
        public string PassengerId { get; set; }
        public string CustomerId { get; set; }

        public static explicit operator SeatAssignment((Infrastructure.Models.TripMS.SeatAssignment seatAssignment, string segmentId, string passengerId) source)
        {
            return new()
            {
                SegmentId = source.segmentId,
                PassengerId = source.passengerId,
                SeatNumber = source.seatAssignment.Seat.Name,
                SeatClass = new() { CabinCode = source.seatAssignment.Seat.ActualCabinClass?.FirstOrDefault()?.Code, CabinProductCode = source.seatAssignment.Seat.ActualCabinClass?.FirstOrDefault()?.ProductCode }
            };
            
        }
    }
}
