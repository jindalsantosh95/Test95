using VAA.EAPI.Domain.Virgin.Models.Ticket;

namespace VAA.EAPI.Infrastructure.PSS.Models
{
    public class TicketingV3ResponseType
    {
        public TicketInformation TicketInformation { get; set; }
        public DTITaxItem[] DTITaxItems { get; set; }
        public TotalTax TotalTax { get; set; }
        public AdditionalTicketLevelData[] AdditionalTicketLevelData { get; set; }
    }
}
