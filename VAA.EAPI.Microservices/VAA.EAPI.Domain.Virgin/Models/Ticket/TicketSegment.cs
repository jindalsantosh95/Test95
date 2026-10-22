using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.Domain.Virgin.Models.Ticket
{
    public class TicketSegment : Segment
    {

        public string ExpandedPassengerTypeCode { get; set; }
        public string PassengerTypeCode { get; set; }

    }

}