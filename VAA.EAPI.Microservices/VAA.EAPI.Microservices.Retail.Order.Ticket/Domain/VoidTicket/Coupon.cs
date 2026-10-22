namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.VoidTicket
{
    public class Coupon
    {
        public string TktDocNumber { get; set; }
        public string TktDocIssueLocalDate { get; set; }
        public string TktDocSeqNumber { get; set; }
        public string CouponNumber { get; set; }
        public string FareBasTktDsgtrTxt { get; set; }
        public string TktCpnETStatusCd { get; set; }
        public string TktSetCpnSqlNumber { get; set; }
        public string TPFCpnSqlNumber { get; set; }
        public string CtrlgCrrCd { get; set; }
        public string FinalStatusCrrCd { get; set; }
        public string LastTrnsEvtGTs { get; set; }
        public string SegmentBkgGDt { get; set; }
        public string TkDocUpdtSqlNumber { get; set; }
        public string LstUpdtGTs { get; set; }
        public string RprcFareBasAmt { get; set; }
        public string RprcFBsCurrCd { get; set; }
        public string RprcSchrAmt { get; set; }
        public string RprcSchrCurrCd { get; set; }
        public string TktCpnRssTypeCd { get; set; }
        public string TPFTrEvtTypeCd { get; set; }
        public string WavrTypeCd { get; set; }
        public string IsscReasonSubCd { get; set; }
        public string IsscReasonSubCdDesc { get; set; }
        public string RefundableInd { get; set; }
        public string CommissionableInd { get; set; }
        public string ReissuableExcblInd { get; set; }
        public string ConsumedAtIsscInd { get; set; }
        public string CpnExpLDt { get; set; }
        public string AncyPrdAssdCd { get; set; }
        public string UTRSttCd { get; set; }
        public string UTRSttDt { get; set; }
        public string CpnRvdnInd { get; set; }
        public string RvdnChnlId { get; set; }
        public string RvdnGTs { get; set; }
    }
}
