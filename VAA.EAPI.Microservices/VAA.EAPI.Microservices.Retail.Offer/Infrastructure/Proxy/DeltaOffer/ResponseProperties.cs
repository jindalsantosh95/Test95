using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class ResponseProperties
    {
        public bool CashPlusMilesAvailable { get; set; }
        public bool Domestic { get; set; }
        public string TripTypeText { get; set; }
        public bool SdtRequest { get; set; }
        public int PageResultCnt { get; set; }
        public int ResultsPageNum { get; set; }
        public string ShoppingResultId { get; set; }
        public int ResultsPerRequestNum { get; set; }
        public IEnumerable<SortOptionsList> SortOptionsList { get; set; }
        public Price MinimumFare { get; set; }
        public string CalendarYearText { get; set; }
        public string CalendarMonthText { get; set; }
        public PolicyStatus TravelPolicyStatus { get; set; }
        public IEnumerable<int> ModifiedTripIndexList { get; set; }
        public IEnumerable<KeyValue> AdditionalResponseProperties { get; set; }
        public bool MixedPassengerTypeCode { get; set; }
        public IEnumerable<string> UniquePassengerTypeCodes { get; set; }
        public DiscountInfo DiscountInfo { get; set; }
        public IEnumerable<DiscountInEligibilityInfo> DiscountInEligibilityList { get; set; }
        public IEnumerable<PromotionInfo> PromotionsInfo { get; set; }
    }
}
