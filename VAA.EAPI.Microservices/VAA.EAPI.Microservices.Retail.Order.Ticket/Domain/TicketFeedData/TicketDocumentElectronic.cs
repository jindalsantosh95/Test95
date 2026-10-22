
namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData
{
    public class TicketDocumentElectronic
    {
        public ElectronicAddress[] ElectronicAddresses
        {
            get;
            set;
        }

        public ElectronicMessage[] ElectronicMessages
        {
            get;
            set;
        }
    }
}
