using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class CreatePassengerNameRecordRQ
    {
        public string Version { get; set; }
        public string TargetCity { get; set; }
        public TravelItineraryAddInfo TravelItineraryAddInfo { get; set; }
        public AirBook AirBook { get; set; }
        public IEnumerable<AirPrice> AirPrice { get; set; }
        public SpecialReqDetails SpecialReqDetails { get; set; }
        public PostProcessing PostProcessing { get; set; }
        public MiscSegment MiscSegment { get; set; }
    }
}
