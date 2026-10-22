
namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData
{
    public class TicketCoupon
    {
        public Coupon[] EtCpn
        {
            get;
            set;
        }

        public CouponSource EtCpnSrc
        {
            get;
            set;
        }

        public CouponState[] EtCpnSt
        {
            get;
            set;
        }

        public CouponStSrc[] EtCpnStSrc
        {
            get;
            set;
        }

        public CouponCarrierRole[] EtCpnCrrRole
        {
            get;
            set;
        }

        public CouponRelation[] EtCpnRln
        {
            get;
            set;
        }

        public CouponRelationHistory[] EtCpnRlnHst
        {
            get;
            set;
        }

        public CouponExchangeRelation[] EtTkDcCpnEXRln
        {
            get;
            set;
        }

        public ReissueCouponFlownItinerary[] EtReissCpnFlwnItin
        {
            get;
            set;
        }
    }
}
