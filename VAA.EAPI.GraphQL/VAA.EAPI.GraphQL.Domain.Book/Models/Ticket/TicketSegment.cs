using VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Ticket
{
    public class TicketSegment : Segment
    {

        public string ExpandedPassengerTypeCode { get; set; }
        public string PassengerTypeCode { get; set; }

    }

}