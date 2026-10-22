using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions
{
    public class CreditMetaData
    {
        public bool IsFlight { get; set; }
        public int AccruedValueMiles { get; set; }
        public string Action { get; set; }
        public string ActivityDatetime { get; set; }
        public string ActivityDifferentiator { get; set; }
        public int ActivityMultiplier { get; set; }
        public string ActivityNumber { get; set; }
        public string ActivitySubtype { get; set; }
        public string ActivityType { get; set; }
        public int BasePoints { get; set; }
        public int BonusMiles { get; set; }
        public string BookingClassCode { get; set; }
        public string BookingReference { get; set; }
        public string CancellationReason { get; set; }
        public string CancelledBy { get; set; }
        public string CancelledTxnId { get; set; }
        public string ClassCode { get; set; }
        public int ClassOfServicePoints { get; set; }
        public string Comments { get; set; }
        public string CreatedDatetime { get; set; }
        public string CreatedBy { get; set; }
        public string CreditSourceType { get; set; }
        public int EliteTierBonusPoints { get; set; }
        public string FareBasis { get; set; }
        public int GiftAdjustments { get; set; }
        public string Id { get; set; }
        public string Info { get; set; }
        public string InsertedDatetime { get; set; }
        public bool ManualAdjustmentFlag { get; set; }
        public long MembershipNumber { get; set; }
        public int MilesPlusCashNetMiles { get; set; }
        public int NonGiftAdjustments { get; set; }
        public int NumUnits { get; set; }
        public string Owner { get; set; }
        public string PannNumber { get; set; }
        public string PartnerCategory { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string PartnerSubtype { get; set; }
        public string PdmFileType { get; set; }
        public string PdmPartnerFilename { get; set; }
        public int Points { get; set; }
        public IEnumerable<PointsComponent> PointsComponents { get; set; }
        public string PostingType { get; set; }
        public string ProductDescription { get; set; }
        public string ProductCategory { get; set; }
        public string ProductName { get; set; }
        public int PromotionBonusPoints { get; set; }
        public int Quantity { get; set; }
        public bool RetroIndicatorFlag { get; set; }
        public bool RetroOverrideFlag { get; set; }
        public string RetroOverrideReason { get; set; }
        public string Schema { get; set; }
        public IEnumerable<CreditSector> Sectors { get; set; }
        public int StandardMiles { get; set; }
        public string StatementText { get; set; }
        public string Status { get; set; }
        public string TicketIssueDatetime { get; set; }
        public string TransactionChannelId { get; set; }
        public string TransactionDatetime { get; set; }
        public string TransactionId { get; set; }
        public string TransactionStatus { get; set; }
        public long TransferMembershipNumber { get; set; }
        public int TravelMiles { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDatetime { get; set; }
        public string VapDMDocNumber { get; set; }
        public string VapDMRecordId { get; set; }
    }
}
