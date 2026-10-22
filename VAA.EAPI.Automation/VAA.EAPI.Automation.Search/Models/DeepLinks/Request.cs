namespace VAA.EAPI.Automation.Search.Models.DeepLinks
{
    public class Request
    {
        public string metaPartnerId { get; set; }
        public List<SegmentRequest> flightSegments { get; set; }
        public TotalOfferedAmount totalOfferedAmount { get; set; }
        public Customers customers { get; set; }
        public string dominantSegmentBrandId { get; set; }
        public string summaryLink { get; set; }
    }
}