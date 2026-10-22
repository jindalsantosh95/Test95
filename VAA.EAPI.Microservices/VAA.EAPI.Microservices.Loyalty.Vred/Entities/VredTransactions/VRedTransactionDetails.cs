using System;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions
{
    public class VRedTransactionDetails
    {
        public string parentTransactionId { get; set; }
        public string TransactionId { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string MemberNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ActivityType { get; set; }
        public int Points { get; set; }
        public int? TierPoints { get; set; }
        public DateTime ActivityDate { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Updated { get; set; }
        public string UpdatedBy { get; set; }
        public string TransactionChannel { get; set; }
        public string Status { get; set; }
        public string BookingClass { get; set; }
        public string TransactionNumber { get; set; }
        public string BonusMiles { get; set; }
        public string ActivityNumber { get; set; }
        public string FlightNumber { get; set; }
        public string NoofPassengers { get; set; }
        public string NoOfSegments { get; set; }
        public string ProductName { get; set; }
        public string TicketNumber { get; set; }
        public DateTime TicketIssueDate { get; set; }
        public string TransactionSubType { get; set; }
        public string CancellationReason { get; set; }
        public string Comments { get; set; }
        public string CancelledTxnId { get; set; }
        public string VoucherNumber { get; set; }
        public string VoucherType { get; set; }
        public string VoucherStatus { get; set; }
        public string VoucherDescription { get; set; }
        public DateTime VoucherIssueDate { get; set; }
        public string PartnerCategory { get; set; }
        public string ProductCategory { get; set; }
        public string ActivityMultiplier { get; set; }
        public string VAPDMDocNum { get; set; }
        public string PANNumber { get; set; }
        public string PDMFileType { get; set; }
        public string GiftAdjustments { get; set; }
        public string NonGiftAdjustments { get; set; }
        public string PDMPartnerFileName { get; set; }
        public string CanxBy { get; set; }
        public string ProdDescription { get; set; }
        public string RetroIndicatorFlag { get; set; }
        public string RetroOverrideFlag { get; set; }
        public string RetroOverrideReason { get; set; }
        public string StandardMiles { get; set; }
        public string TransferMemberNumber { get; set; }
        public string VAPDMRecordId { get; set; }
        public DateTime VoucherExpiryDate { get; set; }
        public string ActivityDifferentiator { get; set; }
        public string SegmentBookingClass { get; set; }
        public string MarketingBookingClass { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string SegmentFlightNumber { get; set; }
        public string AuthorizationNumber { get; set; }
        public string PostingType { get; set; }
        public string OriginationAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string TravelDistance { get; set; }
        public string BasePoints { get; set; }
        public string ClassOfServicePoints { get; set; }
        public string EliteTierPoints { get; set; }
        public string PointType { get; set; }
        public string PromotionBonusPoints { get; set; }
        public string PromotionType { get; set; }
        public string ClassCode { get; set; }
        public string BookingReference { get; set; }
        public string CabinClass { get; set; }
        public string FareBasis { get; set; }
        public string LifeTimeMiles { get; set; }
        public string PartnerSubType { get; set; }
        public string PromotionBonusIndicator { get; set; }
    }
}