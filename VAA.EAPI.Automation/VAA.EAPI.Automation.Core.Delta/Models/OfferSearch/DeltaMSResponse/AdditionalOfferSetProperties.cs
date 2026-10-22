namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class AdditionalOfferSetProperties
    {
        public bool lowestFare { get; set; }
        public bool fastest { get; set; }
        public string seatReferenceId { get; set; }
        public string offerSetId { get; set; }
        public List<object> discountInEligibilityList { get; set; }
        public List<object> promotionsInfo { get; set; }
    }
}
