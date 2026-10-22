namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class FareSegment
    {
        public string SegmentRefId { get; set; }
        public string MarketingFlightNum { get; set; }
        public string ConnectionStopOvertext { get; set; }
        public string PriceableUnitNum { get; set; }
        public string FareBasisCodeTktDesigText { get; set; }
        public string WebFareIndicator { get; set; }
        public string NotValidAfterLocalTs { get; set; }
        public string NotValidBeforeLocalTs { get; set; }
        public SalesAdjustedPrice SalesAdjustedPrice { get; set; }
        public BaggageAllowance BaggageAllowance { get; set; }
    }
}
