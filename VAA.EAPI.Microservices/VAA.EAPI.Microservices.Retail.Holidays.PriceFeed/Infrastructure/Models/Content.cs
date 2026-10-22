namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Models
{
    public class Content
    {
        public string Name { get; set; }
        public string VirginView { get; set; }
        public bool TelephoneBookableOnly { get; set; }
        public string VRating { get; set; }
        public string HotelDescription { get; set; }
        public IEnumerable<string> AtAGlance { get; set; }
        public Location Location { get; set; }
        public string ParentLocation { get; set; }
        public IEnumerable<Image> Images { get; set; }
        public Video Video { get; set; }
        public IEnumerable<KeyFeature> KeyFeatures { get; set; }
        public IEnumerable<SalesMessage> SalesMessages { get; set; }
        public IEnumerable<HotelDetail> HotelDetails { get; set; }
        public string UrlName { get; set; }
        public string Url { get; set; }
        public string ParentUrlName { get; set; }
        public string ResortFees { get; set; }
        public string ResortFeesDetail { get; set; }
        public string Priority { get; set; }
        public string ManualBoost { get; set; }
        public IEnumerable<object> AdditionalFields { get; set; }

    }
}
