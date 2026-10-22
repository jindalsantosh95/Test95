using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability
{
    public class SeatAvailabilityDocument : CosmosDocument
    {
        public Parties Parties { get; set; }
        public Pos Pos { get; set; }
        public string OfferExpirationTimeLimit { get; set; }
        public string SeatOfferId { get; set; }
        public IEnumerable<FlightSegmentSeatMap> FlightSegmentSeatMap { get; set; }
        public string DocType { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string OrderId { get; set; }
    }
}
