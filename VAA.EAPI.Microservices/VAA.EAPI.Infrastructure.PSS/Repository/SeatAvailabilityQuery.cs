using MediatR;
using VAA.EAPI.Infrastructure.PSS.Models;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class SeatAvailabilityQuery : IRequest<Entities.SeatAvailability.RetrievePassengerMapResponse>
    {
        public Air4RequestContext Air4RequestContext { get; set; }
        public string PnrNumber { get; set; }
        public string SegmentId { get; set; }
        public string DestinationAirportCode { get; set; }
        public string OriginAirportCode { get; set; }
        public string MarketingCarrier { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OperatingCarrier { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string ScheduledDepartureTime { get; set; }
        public string CurrencyCode { get; set; }
    }
}
