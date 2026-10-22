
namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData
{
    public class SecondaryTicket
    {
        public TicketDocument EtTktDocSec
        {
            get;
            set;
        }

        public TicketDocumentSource EtTktDocSrc
        {
            get;
            set;
        }

        public TicketDocRelationship[] EtTktDocRln
        {
            get;
            set;
        }

        public TicketDocRefundTransactionRelation[] EtTkDcRTrnsRln
        {
            get;
            set;
        }

        public TicketDocRefundTransaction[] EtTkDcRfndTrns
        {
            get;
            set;
        }

        public TicketCoupon[] Coupon
        {
            get;
            set;
        }
        
        public ExchangeTicket[] SecondaryExchange
        {
            get;
            set;
        }
    }
}
