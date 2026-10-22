namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class Alternative
    {
        public string AlternativeId { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public bool? IsSelected { get; set; }
        public List<string> AvailableDates { get; set; }
        public string UpgradeResultIndex { get; set; }
    }
}

