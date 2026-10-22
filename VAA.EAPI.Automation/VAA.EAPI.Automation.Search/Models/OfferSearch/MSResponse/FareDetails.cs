namespace VAA.EAPI.Automation.Search.Models.OfferSearch.MSResponse
{
    public class FareDetails
    {
        public string fareBasisCode { get; set; }
        public string dominantFareProduct { get; set; }
        public string fareFamilyType { get; set; }
        public List<FareSegment> fareSegments { get; set; }
    }
}
