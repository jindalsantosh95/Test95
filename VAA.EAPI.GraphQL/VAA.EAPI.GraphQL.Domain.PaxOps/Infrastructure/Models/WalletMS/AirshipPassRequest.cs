using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.WalletMS
{
    public class AirshipPassRequest
    {
        public IEnumerable<WalletAirportDetails> Airports { get; set; } = [];
        public IEnumerable<WalletSegmentRequest> Segments { get; set; } = [];
        public IEnumerable<WalletBoardingPassRequest> BoardingPasses { get; set; } = [];
        public IEnumerable<WalletPassengerRequest> Passengers { get; set; } = [];

        public static explicit operator AirshipPassRequest((IEnumerable<BoardingPass> boardingPasses, IEnumerable<Passenger> passengers, IEnumerable<Segment> segments, IEnumerable<PaxOps.Models.ReferenceData.AirportDetails> airportDetails) source)
        {
            return new AirshipPassRequest
            {
                BoardingPasses = source.boardingPasses.Select(bp => (WalletBoardingPassRequest)bp),
                Passengers = source.passengers.Select(p => (WalletPassengerRequest)p),
                Segments = source.segments.Select(s => (WalletSegmentRequest)s),
                Airports = source.airportDetails.Select(ad => (WalletAirportDetails)ad)
            };
        }
    }
}
