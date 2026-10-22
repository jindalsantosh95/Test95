namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class EmdDetails
    {
        public string CouponNumber { get; set; }
        public int CouponValue { get; set; }
        public string RelatedTicketDocumentNumber { get; set; }
        public string RelatedCouponNumber { get; set; }
        public string ReasonForIssuanceSubCode { get; set; }
        public string FeeOwnerAirlineDesignator { get; set; }
        public int NumberOfServices { get; set; }
    }
}
