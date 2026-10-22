using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Coupon
    {
        public IEnumerable<AccountableDocumentInConnection> AssociatedAccountableDocuments { get; set; }
        public string BookingClassOfServiceCode { get; set; }
        public string CarrierCode { get; set; }
        public string ConsumedAtIssuanceCode { get; set; }
        public string CouponNum { get; set; }
        public string CouponStatusCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public Price EmdCouponAmt { get; set; }
        public string ExpirationDate { get; set; }
        public string FareBasisCode { get; set; }
        public string FareBasisTicketDesignatorCode { get; set; }
        public string FlightNum { get; set; }
        public string NonEndorsableCode { get; set; }
        public string NonExchangableCode { get; set; }
        public string NonRefundableCode { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string OperatingClassOfServiceCode { get; set; }
        public string OperatingFlightNum { get; set; }
        public string OriginAirportCode { get; set; }
        public string ReasonForIssuanceSubCode { get; set; }
        public string ReasonForIssuanceSubCodeDesc { get; set; }
        public string ScheduledArrivalLocalDate { get; set; }
        public string ScheduledDepartureLocalDate { get; set; }
        public string ScheduledDepartureLocalTime { get; set; }
        public string ServiceDate { get; set; }
    }
}
