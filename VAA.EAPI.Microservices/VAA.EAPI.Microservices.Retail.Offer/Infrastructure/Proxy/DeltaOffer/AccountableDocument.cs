using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class AccountableDocument
    {
        public string AccountableDocumentNum { get; set; }
        public Price AccountableDocumentTotalAmt { get; set; }
        public IEnumerable<Price> Amounts { get; set; }
        public IEnumerable<AccountableDocumentInConnection> AssociatedAccountableDocuments { get; set; }
        public Price AvailableCredit { get; set; }
        public int AwardPointsFareTotalCnt { get; set; }
        public Price BaseFareAmt { get; set; }
        public Price CountryOfCommencementBaseFareTotalAmt { get; set; }
        public IEnumerable<Coupon> Coupons { get; set; }
        public IEnumerable<CouponEndorsement> CouponEndorsements { get; set; }
        public string DocumentCategoryCode { get; set; }
        public string DocumentConnectionTypeCode { get; set; }
        public string EffectiveLocalDate { get; set; }
        public string ElectronicDocumentNum { get; set; }
        public IEnumerable<ElectronicMiscellaneousDocument> ElectronicMiscellaneousDocument { get; set; }
        public string EndorsementText { get; set; }
        public Price EquivalentFare { get; set; }
        public string ExpirationDate { get; set; }
        public FareCalculation FareCalculation { get; set; }
        public string FareCalculationText { get; set; }
        public IEnumerable<FeeDocumentIssued> FeeDocumentsIssued { get; set; }
        public IEnumerable<FormOfPayment> FormsOfPayment { get; set; }
        public IEnumerable<FopHistory> FopHistory { get; set; }
        public string GroundHandlingCarrierCode { get; set; }
        public string IssueDate { get; set; }
        public IEnumerable<LoyaltyMember> LoyaltyMembers { get; set; }
        public LoyaltyMemberRedemption LoyaltyMilesRedemption { get; set; }
        public string NonCommisionableCode { get; set; }
        public Price OriginalTicketTotalAmt { get; set; }
        public Passenger Passenger { get; set; }
        public PointOfSale PointOfSale { get; set; }
        public string ReasonForIssuanceCode { get; set; }
        public string ReasonForIssuanceDesc { get; set; }
        public bool RefundableTicket { get; set; }
        public string RefundDocumentNum { get; set; }
        public Price SaleFareTotalAmt { get; set; }
        public Price TaxTotalAmt { get; set; }
        public string TicketTypeCode { get; set; }
        public Price TotalPaymentAmt { get; set; }
        public IEnumerable<TravelRelatedServiceTax> TravelRelatedServiceTaxes { get; set; }
        public Price ChangeFeeAmt { get; set; }
        public Price CancelFeeAmt { get; set; }
    }
}
