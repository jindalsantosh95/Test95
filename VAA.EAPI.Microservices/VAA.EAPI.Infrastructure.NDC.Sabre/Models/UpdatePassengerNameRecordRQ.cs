namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class UpdatePassengerNameRecordRQ
    {
        public string Version { get; set; }
        public string TargetCity { get; set; }
        public string HaltOnAirPriceError { get; set; }
        public Itinerary itinerary { get; set; }
        public TravelItineraryAddInfo TravelItineraryAddInfo { get; set; }
        public UpdatePNRAirbook AirBook { get; set; }
        public AirPrice AirPrice { get; set; }
        public SpecialReqDetails SpecialReqDetails { get; set; }
        public PostProcessing PostProcessing { get; set; }
    }
}