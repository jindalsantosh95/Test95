

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class AncillaryOrderItem
    {
        public string CustomerId { get; set; }
        public string ServiceId { get; set; }
        public string SsrNumber { get; set; }
        public string CustomerOrderItemSequenceNumber { get; set; }
        public string MarketedProductCode { get; set; }
        public string OrderItemPurchaseAmount { get; set; }
        public string OrderItemPurchaseCurrencyCode { get; set; }
        public string OrderItemPurchaseUSDAmount { get; set; }
        public string FormOfPaymentCategoryCode { get; set; }
        public string FormOfPaymentTypeCode { get; set; }
        public string RecordLocatorIdentifier { get; set; }
        public string BookingCreationGMTTimestamp { get; set; }
        public string ReservationSystemProviderCode { get; set; }
        public string BookingACNNumber { get; set; }
        public string TicketingACNNumber { get; set; }
        public string AgentSecurityIdentifier { get; set; }
        public string AgentDutyCode { get; set; }
        public string AgentLocationCode { get; set; }
        public string WaiverCode { get; set; }
        public string EMDNumber { get; set; }
        public string EMDIssueDate { get; set; }
        public string SOAApplicationChannelIdentifier { get; set; }
        public string SOAChannelSessionName { get; set; }
        public string CreationGMTTimestamp { get; set; }
        public string LastUpdateUserIdentifier { get; set; }
        public string LastUpdateGMTTimestamp { get; set; }
        public string AssociatedTicketDocumentNumber { get; set; }
        public string AssociatedTicketDocumentIssueLocalDate { get; set; }
        public string OrderItemExpirationDate { get; set; }
        public string MarketedProductOfferingTypeCode { get; set; }
        public string OrderItemWaiverQuoteAmount { get; set; }
        public string EMDCouponNumber { get; set; }
        public string MarketedProductTypeCode { get; set; }
        public string PurchaseId { get; set; }
        public string CustomerIdentifier { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerFullName { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string LoyaltyProgramCode { get; set; }
        public string LoyaltyCarrierCode { get; set; }
        public string LoyaltyProgramTierCode { get; set; }
        public string LoyaltyProgramAccountIdentifier { get; set; }
    }
}
