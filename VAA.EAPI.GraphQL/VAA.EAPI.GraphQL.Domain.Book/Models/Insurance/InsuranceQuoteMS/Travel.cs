
namespace VAA.EAPI.GraphQL.Domain.Book.Models.Insurance.InsuranceQuoteMS
{
    public class Travel
    {
        public string ClassOfService { get; set; }
        public string DepartureDate { get; set; }
        public Destination Destination { get; set; }
        public bool IncludesConnection { get; set; }
        public bool IsDomestic { get; set; }
        public Origin Origin { get; set; }
        public string Refundable { get; set; }
        public string ReturnDate { get; set; }
        public double TotalCost { get; set; }
        public IEnumerable<TravelSegment> TravelSegments { get; set; }
        public string TripIndicator { get; set; }
        public string BrandedFare { get; internal set; }
        public string MembershipStatus { get; internal set; }
    }
}