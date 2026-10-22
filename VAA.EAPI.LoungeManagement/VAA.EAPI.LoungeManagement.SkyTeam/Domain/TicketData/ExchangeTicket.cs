
namespace VAA.EAPI.LoungeManagement.SkyTeam.Domain.TicketData
{
    public class ExchangeTicket
    {
        public TicketDocument[] EtTktDocRef
        {
            get;
            set;
        }
        public TicketDocument[] EtTktDocInx
        {
            get;
            set;
        }
        public TicketDocument[] EtTktDocExc
        {
            get;
            set;
        }
        public TicketDocumentSource[] EtTktDocSrcOp
        {
            get;
            set;
        }
        public TicketDocumentExchangeRelation[] EtTkDcExcgRln
        {
            get;
            set;
        }
        public TicketDocumentExchangeResidualTax[] EtTkDcExRsdlTx
        {
            get;
            set;
        }
        public TicketDocRelationship[] EtTktDocRlnOp
        {
            get;
            set;
        }
        public TicketMiscellaneousDoc[] EtMiscDocOp
        {
            get;
            set;
        }
        public TicketCoupon[] CouponOp
        {
            get;
            set;
        }
        public TicketDocumentMscOp[] EtTktDocMscOp
        {
            get;
            set;
        }
    }
}
