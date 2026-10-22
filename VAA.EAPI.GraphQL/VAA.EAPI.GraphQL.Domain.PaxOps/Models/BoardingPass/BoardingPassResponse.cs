using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass
{
    public class BoardingPassResponse
    {
        public IEnumerable<BoardingPass> BoardingPasses { get; set; }
        public PassengersCheckInStatus CheckInStatus { get; set; }
        public string? TripsBookingId { get; set; }
        public string? IosPnrMultiBoardingPassUrl { get; set; }
        public string? AndroidPnrMultiBoardingPassUrl { get; set; }
        public string? PnrMultiBoardingPassUrl { get; set; }

        public static explicit operator BoardingPassResponse(Infrastructure.Models.CheckInMS.BoardingPassResponse serviceResponse)
        {
            return new BoardingPassResponse()
            {
                BoardingPasses = serviceResponse?.BoardingPasses?.Select(bp => (BoardingPass)bp) ?? [],
                CheckInStatus = serviceResponse?.CheckInStatus ?? default,
            };
        }
    }
}
