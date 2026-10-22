using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Order
{
    public class BookingReference
    {
        public string AirlineDesignator { get; set; }
        public string RLOC { get; set; }
        public SplitRLOC SplitRLOC { get; set; }
        public string RLOCCreateDate { get; set; }
        public string RLOCCreateDateTime { get; set; }
        public string ExternalRecordLocator { get; set; }
        public string ExternalSystem { get; set; }

        public static explicit operator BookingReference(JObject source)
        {
            return new BookingReference()
            {
                AirlineDesignator = source["AirlineDesignator"]?.ToString(),
                RLOC = source["RLOC"]?.ToString(),
                RLOCCreateDate = source["RLOCCreateDate"]?.ToString(),
                RLOCCreateDateTime = source["RLOCCreateDateTime"]?.ToString(),
                ExternalRecordLocator = source["ExternalRecordLocator"]?.ToString(),
                ExternalSystem = source["ExternalSystem"]?.ToString(),
            };
        }
    }
}