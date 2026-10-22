using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Brand;

namespace VAA.EAPI.Infrastructure.PSS.Entities.SeatAvailability
{
    public class RetrievePassengerMapResponse
    {
        public FlightSegment FlightSegment { get; set; }
        public List<SeatMap> PassengerCabins { get; set; }
    }
}
