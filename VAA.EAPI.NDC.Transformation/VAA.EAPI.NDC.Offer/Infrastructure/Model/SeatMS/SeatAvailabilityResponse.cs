using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS
{
    public class SeatMSAvailabilityResponse
    {
        public Parties Parties { get; set; }
        public Pos Pos { get; set; }        
        public string SeatOfferId { get; set; }        
        public string OfferExpirationTimeLimit { get; set; }
        public IEnumerable<FlightSegmentSeatMap> FlightSegmentSeatMap { get; set; }      
        public string OrderId { get; set; }
    }
}
