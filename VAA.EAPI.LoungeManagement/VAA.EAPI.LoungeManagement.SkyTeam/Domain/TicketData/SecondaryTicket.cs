
namespace VAA.EAPI.LoungeManagement.SkyTeam.Domain.TicketData
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

        public SecondaryExchange[] SecExchange
        {
            get;
            set;
        }

        public TicketDocument[] EtTktDocInx
        {
            get;
            set;
        }

        public SecTicketCouponOp[] CouponOp
        {
            get;
            set;
        }
    }
}
