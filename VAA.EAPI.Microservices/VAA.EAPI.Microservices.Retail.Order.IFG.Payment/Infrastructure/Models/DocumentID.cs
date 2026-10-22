namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{

    public class DocumentID
    {
        public string TicketDocumentNumber { get; set; }
        public string TicketingAirlineCodeNumber { get; set; }
        public string TicketCRC { get; set; }
        public string TicketingModeIndicator { get; set; }
    }

}
