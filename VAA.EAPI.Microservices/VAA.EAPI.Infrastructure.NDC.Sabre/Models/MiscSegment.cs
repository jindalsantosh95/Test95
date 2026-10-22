namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class MiscSegment
    {
        public VendorPrefs VendorPrefs { get; set; }
        public string DepartureDateTime { get;set; }
        public string NumberInParty { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public OriginLocation OriginLocation { get; set; }
        public string Status { get; set; }
    }
}
