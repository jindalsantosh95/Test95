using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class Order
    {
        public IEnumerable<Party> Parties { get; set; }
        public CorporateAccount CorporateAccount { get; set; }
        public LocalAndUTCDateTime CreationDateTime { get; set; }
        public LocalAndUTCDateTime CancelledDateTime { get; set; }
        public LocalAndUTCDateTime AmendedDateTime { get; set; }
        public OrderNumber OrderId { get; set; }
        public string OrderVersion { get; set; }
        public POS OriginalPOS { get; set; }
        public Link OrderChangeWebHook { get; set; }
        public string ValidatingCarrier { get; set; }
        public CurrencyAmount TotalPrice { get; set; }
        public CurrencyAmount TotalFeesAndTaxes { get; set; }
        public PriceComponent TotalProductPrice { get; set; }
        public IEnumerable<BookingReference> BookingReferences { get; set; }
        public IEnumerable<AccountableDocumentReference> AccountableDocumentRefs { get; set; }
        public IEnumerable<Payment> Payments { get; set; }
        public IEnumerable<OrderItem> OrderItems { get; set; }
        public string OrderStatus { get; set; }
        public string OrderSubStatus { get; set; }
        public IEnumerable<Warning> OrderWarnings { get; set; }
        public IEnumerable<Notice> OrderNotices { get; set; }
        public IEnumerable<string> OrderRemarks { get; set; }
        public IEnumerable<Alert> OrderAlerts { get; set; }
        public IEnumerable<RuleOrPolicy> OrderPolicies { get; set; }
        public IEnumerable<Penalty> OrderPenalties { get; set; }
        public Party OrderParty { get; set; }
        public TimeLimits TimeLimits { get; set; }
        public OrderCancellation CancellationInfo { get; set; }
        public IEnumerable<ProductOfferOriginDestinationAssociations> OriginDestinationAssociations { get; set; }
        public int PaymentReattempt { get; set; }

        public enum OrderStatusEnum
        {
            OpenEnum = 1,
            CancelledEnum = 2,
            ConfirmedEnum = 3,
            CompletedEnum = 4
        }

        public enum OrderSubStatusEnum
        {
            VerificationEnum = 1,
            VerifiedEnum = 2,
            AuthEnum = 3,
            SettleEnum = 4,
            UnPaidEnum = 5,

        }

    }
}
