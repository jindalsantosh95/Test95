
using VAA.EAPI.Domain.Virgin.Models.Order;
namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.FlownPassenger
{
    public class FlownPassengerRequest
    {
        public Flight EventFlight { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public IEnumerable<Seat> Seats { get; set; }
        public string OrderId { get; set; }
        public bool Irop { get; set; }
        public string RecordType { get; set; }
        public string PointOfSale { get; set; }
        public string AgentIataNumber { get; set; }
        public string TicketType { get; set; }
        public string TicketDesignator { get; set; }
        public string TicketNumber { get; set; }
        public string PnrName { get; set; }
        public string TicketIssueDate { get; set; }
        public string TicketOriginalCity { get; set; }
        public string OperatingAirAuthNumber { get; set; }
        public string LoyaltyAccountNumber { get; set; }
        public string LoyaltyAirlineCode { get; set; }
        public string CouponNumber { get; set; }
        public string FareBasisCode { get; set; }
        public string ProcessingStatus { get; set; }

    }
}
