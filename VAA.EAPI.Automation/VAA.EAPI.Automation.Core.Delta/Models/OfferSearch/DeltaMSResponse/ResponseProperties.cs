namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class ResponseProperties
    {
        public bool cashPlusMilesAvailable { get; set; }
        public bool domestic { get; set; }
        public string tripTypeText { get; set; }
        public int pageResultCnt { get; set; }
        public int resultsPageNum { get; set; }
        public int resultsPerRequestNum { get; set; }
        public List<SortOptionsList> sortOptionsList { get; set; }
        public List<object> discountInEligibilityList { get; set; }
        public List<object> promotionsInfo { get; set; }
    }
}
