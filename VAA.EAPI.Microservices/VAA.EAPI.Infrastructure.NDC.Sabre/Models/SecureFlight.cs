namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class SecureFlight
    {
        public string SegmentNumber { get; set; }
        public PersonName PersonName { get; set; }
        public VendorPrefs VendorPrefs { get; set; }
        public string IssueCountry { get; set; }
        public string KnownTravelerNumber { get; set; }
        public string RedressNumber { get; set; }

    }
}
