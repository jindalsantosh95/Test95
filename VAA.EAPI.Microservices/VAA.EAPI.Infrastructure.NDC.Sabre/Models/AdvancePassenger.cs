namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class AdvancePassenger
    {
        public Document Document { get; set; }
        public PersonName PersonName { get; set; }
        public ResidentDestinationAddress ResidentDestinationAddress { get; set; }
        public VendorPrefs VendorPrefs { get; set; }
        public string SegmentNumber {  get; set; }
    }
}
