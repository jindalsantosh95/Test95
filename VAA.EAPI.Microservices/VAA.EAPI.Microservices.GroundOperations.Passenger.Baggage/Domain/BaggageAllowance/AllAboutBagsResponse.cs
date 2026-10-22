using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.BaggageAllowance
{
    public class AllAboutBagsResponse
    {
        public IEnumerable<BaggageInfo> EveryoneGets { get; set; }
        public IEnumerable<BaggageInfo> ByCabin { get; set; }
        public string BaggageUrl { get; set; } = ConfigReader.Get(Constants.Environment.BaggageUrl);
    }
}