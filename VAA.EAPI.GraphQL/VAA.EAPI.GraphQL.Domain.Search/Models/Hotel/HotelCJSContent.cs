namespace VAA.EAPI.GraphQL.Domain.Search.Models.Hotel
{
    public class HotelCJSContent
    {
        public string? Name { get; set; }
        public string? VirginView { get; set; }
        public bool TelephoneBookableOnly { get; set; }
        public string? VRating { get; set; }
        public string? HotelDescription { get; set; }
        public IEnumerable<string> AtAGlance { get; set; } = Enumerable.Empty<string>();
        public Location? Location { get; set; }
        public string? ParentLocation { get; set; }
        public IEnumerable<HotelImage> Images { get; set; } = Enumerable.Empty<HotelImage>();
        public IEnumerable<KeyFeature> KeyFeatures { get; set; } = Enumerable.Empty<KeyFeature>();
        public string? UrlName { get; set; }
        public string? Url { get; set; }
        public string? ParentUrlName { get; set; }
        public IEnumerable<string> HolidayType { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> BoardBasis { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> HotelLocation { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> AccommodationType { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> HotelFacilities { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> FamilyKids { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> Activities { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> Features { get; set; } = Enumerable.Empty<string>();
        public bool ResortFees { get; set; }
        public IEnumerable<SalesMessage> SalesMessages { get; set; } = Enumerable.Empty<SalesMessage>();
        public string? PropertyType { get; set; }
        public string? StarRating { get; set; }
        public Video? Video { get; set; }
    }
}
