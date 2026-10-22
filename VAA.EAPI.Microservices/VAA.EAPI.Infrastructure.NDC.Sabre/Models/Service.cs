using Newtonsoft.Json;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class Service
    {
        [JsonProperty(PropertyName = "SSR_Code")]
        public string SSRCode { get; set; }
        public string Text { get; set; }
        public PersonName PersonName { get; set; }
        public VendorPrefs VendorPrefs { get; set; }
        public string SegmentNumber {  get; set; }
    }
}
