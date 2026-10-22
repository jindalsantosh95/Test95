
namespace VAA.EAPI.LoungeManagement.SkyTeam.Domain.TicketData
{
    public class SecTicketCouponOp
    {
        private Coupon[] eTCPNField;

        private CouponSource eTCPNSRCField;

        private CouponState[] eTCPNSTField;

        private CouponStSrc[] eTCPNSTSRCField;

        private CouponCarrierRole[] eTCPNCRRROLEField;

        private CouponRelation[] eTCPNRLNField;

        private CouponRelationHistory[] eTCPNRLNHSTField;

        private CouponExchangeRelation[] eTTKDCCPNEXRLNField;

        private ReissueCouponFlownItinerary[] eTREISSCPNFLWNITINField;

        private TicketDocument eTTKTDOCSECField;

        private TicketDocumentSource eTTKTDOCSRCField;

        private TicketDocRelationship[] eTTKTDOCRLNField;

        public Coupon[] ETCPN
        {
            get
            {
                return this.eTCPNField;
            }
            set
            {
                this.eTCPNField = value;
            }
        }
        public CouponSource ETCPNSRC
        {
            get
            {
                return this.eTCPNSRCField;
            }
            set
            {
                this.eTCPNSRCField = value;
            }
        }
        public CouponState[] ETCPNST
        {
            get
            {
                return this.eTCPNSTField;
            }
            set
            {
                this.eTCPNSTField = value;
            }
        }
        public CouponStSrc[] ETCPNSTSRC
        {
            get
            {
                return this.eTCPNSTSRCField;
            }
            set
            {
                this.eTCPNSTSRCField = value;
            }
        }
        public CouponCarrierRole[] ETCPNCRRROLE
        {
            get
            {
                return this.eTCPNCRRROLEField;
            }
            set
            {
                this.eTCPNCRRROLEField = value;
            }
        }
        public CouponRelation[] ETCPNRLN
        {
            get
            {
                return this.eTCPNRLNField;
            }
            set
            {
                this.eTCPNRLNField = value;
            }
        }
        public CouponRelationHistory[] ETCPNRLNHST
        {
            get
            {
                return this.eTCPNRLNHSTField;
            }
            set
            {
                this.eTCPNRLNHSTField = value;
            }
        }
        public CouponExchangeRelation[] ETTKDCCPNEXRLN
        {
            get
            {
                return this.eTTKDCCPNEXRLNField;
            }
            set
            {
                this.eTTKDCCPNEXRLNField = value;
            }
        }
        public ReissueCouponFlownItinerary[] ETREISSCPNFLWNITIN
        {
            get
            {
                return this.eTREISSCPNFLWNITINField;
            }
            set
            {
                this.eTREISSCPNFLWNITINField = value;
            }
        }
        public TicketDocument ETTKTDOCSEC
        {
            get
            {
                return this.eTTKTDOCSECField;
            }
            set
            {
                this.eTTKTDOCSECField = value;
            }
        }
        public TicketDocumentSource ETTKTDOCSRC
        {
            get
            {
                return this.eTTKTDOCSRCField;
            }
            set
            {
                this.eTTKTDOCSRCField = value;
            }
        }
        public TicketDocRelationship[] ETTKTDOCRLN
        {
            get
            {
                return this.eTTKTDOCRLNField;
            }
            set
            {
                this.eTTKTDOCRLNField = value;
            }
        }
    }
}
