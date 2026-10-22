using Newtonsoft.Json;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData
{
    public class TicketDto : CosmosDocument
    {
        public OptionalRecord[] OptionalRecord { get; set; }

        public EventHeader EventHeader { get; set; }

        public TicketDocument Document { get; set; }

        public TicketDocumentSource DocumentSource { get; set; }

        public TicketDocumentElectronic[] DocumentElectronic { get; set; }

        public TicketDocGeographicAddress[] GeographicAddress { get; set; }

        public TicketDocPhoneAddress[] PhoneAddress { get; set; }

        public TicketDocRelationship[] Relationship { get; set; }

        public TicketDocWavr DocWavr { get; set; }

        public TicketDocSupplemental[] Supplemental { get; set; }

        public TicketMiscellaneousDoc[] MiscellaneousDoc { get; set; }

        public TicketMiscellaneousDocPrintLine MiscellaneousDocPrintLine { get; set; }

        public TicketMiscellaneousDocActivityOp[] MiscellaneousDocActivity { get; set; }

        public TicketDocBookingRecordLocator[] BookingRecordLocator { get; set; }

        public TicketDocPointOfSaleSource[] PointOfSaleSource { get; set; }

        public TicketDocumentCharge[] Charge { get; set; }

        public TicketAccountableCharge[] AccountableCharge { get; set; }

        public TicketDocCommission[] Commission { get; set; }

        public TicketDocTaxFacsimile[] TaxFacsimile { get; set; }

        public TicketDocFareCalculation[] FareCalculation { get; set; }

        public TicketDocPaymentCollection[] PaymentCollection { get; set; }

        public TicketDocPayment[] Payment { get; set; }

        public TicketDocPaymentRemark[] PaymentRemark { get; set; }

        public TicketDocRefundTransactionRelation[] RefundTransactionRelation { get; set; }

        public TicketDocResponse[] DocResponse { get; set; }

        public TicketDocSuspendStatusRemark[] SuspendStatusRemark { get; set; }

        public TicketDocRefundTransaction[] RefundTransaction { get; set; }

        public TicketDocRefundTransactionCharge[] RefundTransactionCharge { get; set; }

        public TicketDocRefundTransactionCommission[] RefundTransactionCommission { get; set; }

        public TicketDocRefundTransactionPayment[] RefundTransactionPayment { get; set; }

        public TicketDocTranasactionAgencyReason[] TranasactionAgencyReason { get; set; }

        public TicketCoupon[] Coupons { get; set; }

        public ExchangeTicket[] ExchangeTickets { get; set; }

        public SecondaryTicket[] SecondaryTickets { get; set; }

        public string DocType { get; set; }

        [JsonProperty(PropertyName = "_etag")]
        public string ETag { get; set; }

        [JsonProperty(PropertyName = "_ts")]
        public long Timestamp { get; set; }

        [JsonProperty(PropertyName = "_rid")]
        public string ResourceId { get; set; }

        [JsonProperty(PropertyName = "ttl")]
        public int? Ttl { get; set; } = null;
    }
}
