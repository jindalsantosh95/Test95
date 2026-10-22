
namespace VAA.EAPI.LoungeManagement.SkyTeam.Domain.TicketData
{
    public class SecondaryExchange
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
        public TicketDocumentMscOp[] EtTktDocMscOp
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
        public TicketDocumentExchangeResidualTax[] EtTkDcExrsdlTx
        {
            get;
            set;
        }
        public TicketDocumentMsc[] EtTktDocMsc
        {
            get;
            set;
        }
        public TicketDocRelationship[] EtTktDocRlnOp
        {
            get;
            set;
        }
        public TicketMiscellaneousDoc[] EtMiscCDoc
        {
            get;
            set;
        }
        public TicketCoupon[] CouponOp
        {
            get;
            set;
        }
    }
}
